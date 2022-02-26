using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using System;

namespace Moneybox.App.Tests;

[TestClass]
public class WithdrawMoneyUnitTests
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
    public void TestWithdrawMoneySuccess()
    {
        //Arrange
        Guid fromAccountId = Guid.NewGuid();
        var fromAccount = SetUpAccountDataAndGetAccount(1000, 800, fromAccountId);

        var amount = 80;

        var withdrawMoney = new WithdrawMoney(this.mockAccountRepository.Object, this.mockNotificationService.Object);

        // Act 

        withdrawMoney.Execute(fromAccountId, amount);

        // Assert

        mockAccountRepository.Verify(x => x.GetAccountById(fromAccountId), Times.Once);

        mockNotificationService.Verify(x => x.NotifyFundsLow(It.IsAny<string>()), Times.Never);

        mockAccountRepository.Verify(x => x.Update(fromAccount), Times.Once);

        Assert.AreEqual(fromAccount.Balance, 920);
        Assert.AreEqual(fromAccount.Withdrawn, 880);
    }

    [TestMethod]
    public void TestTransferMoneySuccessAndNotifyLowFunds()
    {
        //Arrange
        Guid fromAccountId = Guid.NewGuid();
        var fromAccount = SetUpAccountDataAndGetAccount(1000, 800, fromAccountId);

        var amount = 550;

        var withdrawMoney = new WithdrawMoney(this.mockAccountRepository.Object, this.mockNotificationService.Object);

        // Act 

        withdrawMoney.Execute(fromAccountId, amount);

        // Assert

        mockAccountRepository.Verify(x => x.GetAccountById(fromAccountId), Times.Once);

        mockNotificationService.Verify(x => x.NotifyFundsLow(It.IsAny<string>()), Times.Once);

        mockAccountRepository.Verify(x => x.Update(fromAccount), Times.Once);

        Assert.AreEqual(fromAccount.Balance, 450);
        Assert.AreEqual(fromAccount.Withdrawn, 1350);
    }

    [ExpectedException(typeof(InvalidOperationException), "Insufficient funds to make transfer")]
    [TestMethod]
    public void TransferMoneyExecuteInsufficientFundsException()
    {
        //Arrange

        Guid fromAccountId = Guid.NewGuid();
        Guid toAccountId = Guid.NewGuid();
        var fromAccount = SetUpAccountDataAndGetAccount(1000, 800, fromAccountId);

        var amount = 1010;

        var withdrawMoney = new WithdrawMoney(this.mockAccountRepository.Object, this.mockNotificationService.Object);

        // Act 

        withdrawMoney.Execute(fromAccountId, amount);
    }

    //Private methods
    private Account SetUpAccountDataAndGetAccount(decimal balance, decimal withdrawn, Guid accountId)
    {
        var account = new Account() { User = new User(), Balance = balance, Id = accountId, Withdrawn = withdrawn };

        mockAccountRepository.Setup(x => x.GetAccountById(It.Is<Guid>(v => v.Equals(accountId))))
               .Returns(account);

        return account;
    }
}