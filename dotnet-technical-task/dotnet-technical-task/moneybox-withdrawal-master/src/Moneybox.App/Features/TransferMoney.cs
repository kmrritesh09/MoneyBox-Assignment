using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class TransferMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;

        public TransferMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            var from = this.accountRepository.GetAccountById(fromAccountId);
            var to = this.accountRepository.GetAccountById(toAccountId);

            //Validate from and to accounts based on transfer amount and their paid and withdrawl so far
            from.ValidateTranfer(notificationService, amount, Account.TransactionType.Withdrawl);
            to.ValidateTranfer(notificationService, amount, Account.TransactionType.PaidIn);

            //Update balance and paid in or withdrawl field based on transfer direction
            from.UpdateBalanceDetailsForTransfer(amount, Account.TransactionType.Withdrawl);
            to.UpdateBalanceDetailsForTransfer(amount, Account.TransactionType.PaidIn);

            this.accountRepository.Update(from);
            this.accountRepository.Update(to);
        }
    }
}
