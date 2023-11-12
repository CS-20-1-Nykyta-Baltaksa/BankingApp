using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp
{
    public class TransactionLogger
    {
        private readonly string _outputFolderPath;

        public TransactionLogger(string outputFolderPath)
        {
            _outputFolderPath = outputFolderPath;
        }

        public void LogTransaction(string transactionDetails, bool isSuccess)
        {
            string filename = isSuccess ? "SuccessLog.csv" : "FailureLog.csv";
            string filepath = Path.Combine(_outputFolderPath, filename);
        }
    }

}
