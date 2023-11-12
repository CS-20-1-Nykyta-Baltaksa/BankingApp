using BankingApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingTest
{
    public class FakeTransactionLogger : TransactionLogger
    {
        public FakeTransactionLogger() : base("dummy_path") { }

        public virtual void LogTransaction(string message, bool isSuccess)
        {
        }
    }
}
