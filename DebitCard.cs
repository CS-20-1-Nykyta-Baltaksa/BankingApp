using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp
{
    public class DebitCard
    {
        private readonly object _balanceLock = new object();
        private readonly TransactionLogger _logger;

        public decimal Balance { get; private set; }

        public DebitCard(TransactionLogger logger)
        {
            Balance = 0;
            _logger = logger;
        }

        public void Deposit(decimal amount)
        {
            if (amount <= 0)
            {
                _logger.LogTransaction($"Invalid deposit amount: {amount}", false);
                return;
            }

            lock (_balanceLock)
            {
                Balance += amount;
            }
            _logger.LogTransaction($"Deposited {amount}. New Balance: {Balance}", true);
        }

        public bool Withdraw(decimal amount)
        {
            if (amount <= 0)
            {
                _logger.LogTransaction($"Invalid withdrawal amount: {amount}", false);
                return false;
            }

            lock (_balanceLock)
            {
                if (Balance >= amount)
                {
                    Balance -= amount;
                    _logger.LogTransaction($"Withdrew {amount}. New Balance: {Balance}", true);
                    return true;
                }
            }

            _logger.LogTransaction($"Failed withdrawal of {amount}. Insufficient Balance: {Balance}", false);
            return false;
        }
    }
}
