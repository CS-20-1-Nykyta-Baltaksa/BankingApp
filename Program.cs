using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BankingApp;

class Program
{
    static async Task Main(string[] args)
    {
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

        var tasks = new List<Task>();
        var threads = new List<Thread>();

        foreach (var fileName in inputFileNames)
        {
            tasks.Add(Task.Run(async () =>
            {
                await ProcessFileAsync(fileName, debitCard, ref successfulTransactionCount, ref unsuccessfulTransactionCount);
            }));

            Thread thread = new Thread(() =>
            {
                ProcessFile(fileName, debitCard, ref successfulTransactionCount, ref unsuccessfulTransactionCount);
            });
            threads.Add(thread);

            ThreadPool.QueueUserWorkItem((state) =>
            {
                ProcessFile(fileName, debitCard, ref successfulTransactionCount, ref unsuccessfulTransactionCount);
            });
        }

        await Task.WhenAll(tasks);

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
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

    static async Task ProcessFileAsync(string fileName, DebitCard debitCard, ref int successfulTransactionCount, ref int unsuccessfulTransactionCount)
    {
        var lines = await File.ReadAllLinesAsync(fileName);

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
                    Interlocked.Increment(ref successfulTransactionCount);
                    break;

                case "Withdrawal":
                    bool isSuccess = debitCard.Withdraw(amount);
                    if (isSuccess)
                    {
                        Interlocked.Increment(ref successfulTransactionCount);
                    }
                    else
                    {
                        Interlocked.Increment(ref unsuccessfulTransactionCount);
                    }
                    break;

                default:
                    Console.WriteLine($"Unknown transaction type: {transactionType}");
                    break;
            }
        }
    }

    static void ProcessFile(string fileName, DebitCard debitCard, ref int successfulTransactionCount, ref int unsuccessfulTransactionCount)
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
                    Interlocked.Increment(ref successfulTransactionCount);
                    break;

                case "Withdrawal":
                    bool isSuccess = debitCard.Withdraw(amount);
                    if (isSuccess)
                    {
                        Interlocked.Increment(ref successfulTransactionCount);
                    }
                    else
                    {
                        Interlocked.Increment(ref unsuccessfulTransactionCount);
                    }
                    break;

                default:
                    Console.WriteLine($"Unknown transaction type: {transactionType}");
                    break;
            }
        }
    }
}
