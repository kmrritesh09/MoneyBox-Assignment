using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using System;

namespace Moneybox.App.Tests;

[TestClass]
public class TransferMoneyUnitTests
{
    private Mock<IAccountRepository> mockAccountRepository;
    private Mock<INotificationService> mockNotificationService;

    [TestInitialize]
    public void TestInit()
    {
        mockAccountRepository = new Mock<IAccountRepository>();
        mockNotificationService = new Mock<INotificationService>();
    }

    [TestMethod]
    public void TestTransferMoneySuccess()
    {
        //Arrange

        Guid fromAccountId = Guid.NewGuid();
        Guid toAccountId = Guid.NewGuid();
        var fromAccount = SetUpAccountDataAndGetAccount(1000, 800, 200, fromAccountId);
        var toAccount = SetUpAccountDataAndGetAccount(100, 100, 200, toAccountId);

        var amount = 80;

        var transforMoney = new TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);

        // Act 

        transforMoney.Execute(fromAccountId, toAccountId, amount);

        // Assert

        mockAccountRepository.Verify(x => x.GetAccountById(fromAccountId), Times.Once);
        mockAccountRepository.Verify(x => x.GetAccountById(toAccountId), Times.Once);

        mockNotificationService.Verify(x => x.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Never);
        mockNotificationService.Verify(x => x.NotifyFundsLow(It.IsAny<string>()), Times.Never);

        mockAccountRepository.Verify(x => x.Update(fromAccount), Times.Once);
        mockAccountRepository.Verify(x => x.Update(toAccount), Times.Once);

        Assert.AreEqual(fromAccount.Balance, 920);
        Assert.AreEqual(toAccount.Balance, 180);
        Assert.AreEqual(fromAccount.Withdrawn, 880);
        Assert.AreEqual(toAccount.PaidIn, 280);
    }

    [TestMethod]
    public void TestTransferMoneySuccessAndNotifyLowFunds()
    {
        //Arrange

        Guid fromAccountId = Guid.NewGuid();
        Guid toAccountId = Guid.NewGuid();
        var fromAccount = SetUpAccountDataAndGetAccount(1000, 800, 200, fromAccountId);
        var toAccount = SetUpAccountDataAndGetAccount(100, 100, 200, toAccountId);

        var amount = 550;

        var transforMoney = new TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);

        // Act 

        transforMoney.Execute(fromAccountId, toAccountId, amount);

        // Assert

        mockAccountRepository.Verify(x => x.GetAccountById(fromAccountId), Times.Once);
        mockAccountRepository.Verify(x => x.GetAccountById(toAccountId), Times.Once);

        mockNotificationService.Verify(x => x.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Never);
        mockNotificationService.Verify(x => x.NotifyFundsLow(It.IsAny<string>()), Times.Once);

        mockAccountRepository.Verify(x => x.Update(fromAccount), Times.Once);
        mockAccountRepository.Verify(x => x.Update(toAccount), Times.Once);

        Assert.AreEqual(fromAccount.Balance, 450);
        Assert.AreEqual(toAccount.Balance, 650);
        Assert.AreEqual(fromAccount.Withdrawn, 1350);
        Assert.AreEqual(toAccount.PaidIn, 750);
    }

    [TestMethod]
    public void TestTransferMoneySuccessAndNotifyApproachingPayInLimit()
    {
        //Arrange

        Guid fromAccountId = Guid.NewGuid();
        Guid toAccountId = Guid.NewGuid();
        var fromAccount = SetUpAccountDataAndGetAccount(10000, 8000, 200, fromAccountId);
        var toAccount = SetUpAccountDataAndGetAccount(100, 100, 200, toAccountId);

        var amount = 3600;

        var transforMoney = new TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);

        // Act 

        transforMoney.Execute(fromAccountId, toAccountId, amount);

        // Assert

        mockAccountRepository.Verify(x => x.GetAccountById(fromAccountId), Times.Once);
        mockAccountRepository.Verify(x => x.GetAccountById(toAccountId), Times.Once);

        mockNotificationService.Verify(x => x.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Once);
        mockNotificationService.Verify(x => x.NotifyFundsLow(It.IsAny<string>()), Times.Never);

        mockAccountRepository.Verify(x => x.Update(fromAccount), Times.Once);
        mockAccountRepository.Verify(x => x.Update(toAccount), Times.Once);

        Assert.AreEqual(fromAccount.Balance, 6400);
        Assert.AreEqual(toAccount.Balance, 3700);
        Assert.AreEqual(fromAccount.Withdrawn, 11600);
        Assert.AreEqual(toAccount.PaidIn, 3800);
    }

    [ExpectedException(typeof(InvalidOperationException), "Insufficient funds to make transfer")]
    [TestMethod]
    public void TransferMoneyExecuteInsufficientFundsException()
    {
        //Arrange

        Guid fromAccountId = Guid.NewGuid();
        Guid toAccountId = Guid.NewGuid();
        var fromAccount = SetUpAccountDataAndGetAccount(1000, 800, 200, fromAccountId);
        var toAccount = SetUpAccountDataAndGetAccount(100, 100, 200, toAccountId);

        var amount = 1010;

        var transforMoney = new TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);

        // Act 

        transforMoney.Execute(fromAccountId, toAccountId, amount);
    }

    [ExpectedException(typeof(InvalidOperationException), "Account pay in limit reached")]
    [TestMethod]
    public void TransferMoneyExecuteAccountPayInLimitException()
    {
        //Arrange

        Guid fromAccountId = Guid.NewGuid();
        Guid toAccountId = Guid.NewGuid();
        SetUpAccountDataAndGetAccount(10000, 8000, 200, fromAccountId);
        SetUpAccountDataAndGetAccount(100, 100, 200, toAccountId);

        var amount = 3900;

        var transforMoney = new TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);

        // Act 

        transforMoney.Execute(fromAccountId, toAccountId, amount);
    }

    //Private methods
    private Account SetUpAccountDataAndGetAccount(decimal balance, decimal withdrawn, decimal paidIn, Guid accountId)
    {
        var account = new Account() { User = new User(), Balance = balance, Id = accountId, Withdrawn = withdrawn, PaidIn = paidIn };

        mockAccountRepository.Setup(x => x.GetAccountById(It.Is<Guid>(v => v.Equals(accountId))))
               .Returns(account);

        return account;
    }
}