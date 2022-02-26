using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App
{
    public class Account
    {
        public enum TransactionType { Withdrawl, PaidIn };

        public const decimal PayInLimit = 4000m;

        public Guid Id { get; set; }

        public User User { get; set; }

        public decimal Balance { get; set; }

        public decimal Withdrawn { get; set; }

        public decimal PaidIn { get; set; }

        //validate balance and paid In based on withdrawl or paid into the account
        public void ValidateTranfer(INotificationService notificationService, decimal amount, TransactionType type)
        {
            if (type == TransactionType.Withdrawl)
            {
                var fromBalance = this.Balance - amount;
                if (fromBalance < 0m)
                {
                    throw new InvalidOperationException("Insufficient funds to make transfer");
                }

                if (fromBalance < 500m)
                {
                    notificationService.NotifyFundsLow(this.User.Email);
                }
            }
            else if (type == TransactionType.PaidIn)
            {
                var paidIn = this.PaidIn + amount;

                if (paidIn > PayInLimit)
                {
                    throw new InvalidOperationException("Account pay in limit reached");
                }

                if (PayInLimit - paidIn < 500m)
                {
                    notificationService.NotifyApproachingPayInLimit(this.User.Email);
                }
            }
        }

        //Update balance and withdrawn or paid In based on transaction is withdrawl or paid into
        public void UpdateBalanceDetailsForTransfer(decimal amount, TransactionType type)
        {
            if (type == TransactionType.Withdrawl)
            {
                Balance -= amount;
                Withdrawn += amount;
            }
            else if (type == TransactionType.PaidIn)
            {
                Balance += amount;
                PaidIn += amount;
            }
        }
    }
}
