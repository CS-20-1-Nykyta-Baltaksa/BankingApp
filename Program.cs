using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using BankingApp;
// Include any other necessary namespaces

class Program
{
    static void Main(string[] args)
    {
        // Argument validation
        if (args.Length < 2)
        {
            Console.WriteLine("Please provide input and output folder paths.");
            return;
        }

        var inputFolderPath = args[0];
        var outputFolderPath = args[1];

        var transactionLogger = new TransactionLogger(outputFolderPath);
        var debitCard = new DebitCard(transactionLogger);

        Directory.CreateDirectory(outputFolderPath);

        int successfulTransactionCount = 0;
        int unsuccessfulTransactionCount = 0;

        string[] inputFileNames = Directory.GetFiles(inputFolderPath, "*.csv");
        Array.Sort(inputFileNames);

        foreach (var fileName in inputFileNames)
        {
            var lines = File.ReadAllLines(fileName);

            foreach (var line in lines)
            {
                var parts = line.Split(',');

                if (parts.Length != 2)
                {
                    Console.WriteLine($"Invalid line format: {line}");
                    continue;
                }

                string transactionType = parts[0].Trim();
                if (!decimal.TryParse(parts[1].Trim(), out decimal amount) || amount <= 0)
                {
                    Console.WriteLine($"Invalid transaction amount: {parts[1]}");
                    continue;
                }

                switch (transactionType)
                {
                    case "Deposit":
                        debitCard.Deposit(amount);
                        successfulTransactionCount++;
                        break;

                    case "Withdrawal":
                        bool isSuccess = debitCard.Withdraw(amount);
                        if (isSuccess)
                        {
                            successfulTransactionCount++;
                        }
                        else
                        {
                            unsuccessfulTransactionCount++;
                        }
                        break;

                    default:
                        Console.WriteLine($"Unknown transaction type: {transactionType}");
                        break;
                }
            }
        }

        var resultData = new
        {
            FinalBalance = debitCard.Balance,
            TotalSuccessfulTransactions = successfulTransactionCount,
            TotalUnsuccessfulTransactions = unsuccessfulTransactionCount,
            TotalInputFilesProcessed = inputFileNames.Length
        };

        string jsonString = JsonSerializer.Serialize(resultData);
        File.WriteAllText(Path.Combine(outputFolderPath, "result.json"), jsonString);

        Console.WriteLine("Processing completed. Results are stored in the output folder.");
    }
}
