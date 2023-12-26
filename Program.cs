using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
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

        var inputFileNames = Directory.GetFiles(inputFolderPath, "*.csv");
        Array.Sort(inputFileNames);

        var tasks = new List<Task<TaskResult>>();

        foreach (var fileName in inputFileNames)
        {
            tasks.Add(ProcessFileAsync(fileName, debitCard));
        }

        await Task.WhenAll(tasks);

        int successfulTransactionCount = 0;
        int unsuccessfulTransactionCount = 0;

        foreach (var task in tasks)
        {
            var result = await task;
            successfulTransactionCount += result.SuccessfulCount;
            unsuccessfulTransactionCount += result.UnsuccessfulCount;
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

    static async Task<TaskResult> ProcessFileAsync(string fileName, DebitCard debitCard)
    {
        var lines = await File.ReadAllLinesAsync(fileName);
        int successfulTransactionCount = 0;
        int unsuccessfulTransactionCount = 0;

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

        return new TaskResult(successfulTransactionCount, unsuccessfulTransactionCount);
    }
}

class TaskResult
{
    public int SuccessfulCount { get; }
    public int UnsuccessfulCount { get; }

    public TaskResult(int successfulCount, int unsuccessfulCount)
    {
        SuccessfulCount = successfulCount;
        UnsuccessfulCount = unsuccessfulCount;
    }
}
