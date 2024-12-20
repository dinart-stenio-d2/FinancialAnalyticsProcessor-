using CsvHelper;
using CsvHelper.Configuration;
using FinancialAnalyticsProcessor.Domain.Entities;
using FinancialAnalyticsProcessor.Domain.Interfaces;
using System.Globalization;

namespace FinancialAnalyticsProcessor.Infrastructure.Services
{
    public class CsvTransactionLoader : ICsvTransactionLoader
    {
        public async Task<IEnumerable<Transaction>> LoadTransactionsAsync(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ","
            });


            var records = await Task.Run(() => csv.GetRecords<Transaction>().ToList());
            return records;
        }


        public async Task RecreateTransactionAsync(string csvFilePath, Guid transactionId, string newDescription)
        {
            var tempFilePath = $"{csvFilePath}.tmp";

            using (var reader = new StreamReader(csvFilePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ","
            }))
            using (var writer = new StreamWriter(tempFilePath))
            using (var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ","
            }))
            {
                var transactions = csv.GetRecords<Transaction>().ToList();
                var transactionToRecreate = transactions.FirstOrDefault(t => t.TransactionId == transactionId);

                if (transactionToRecreate == null)
                {
                    throw new ArgumentException($"Transaction with ID {transactionId} not found in the file.");
                }

                // Recreate the transaction with the new description
                var recreatedTransaction = new Transaction
                {
                    TransactionId = transactionId,
                    UserId = transactionToRecreate.UserId,
                    Date = transactionToRecreate.Date,
                    Amount = transactionToRecreate.Amount,
                    Category = transactionToRecreate.Category,
                    Description = newDescription,
                    Merchant = transactionToRecreate.Merchant
                };

                // Write all transactions to the new file, including the recreated one
                csvWriter.WriteHeader<Transaction>();
                await csvWriter.NextRecordAsync();

                foreach (var transaction in transactions)
                {
                    if (transaction.TransactionId  == transactionId)
                    {
                        csvWriter.WriteRecord(recreatedTransaction); // Write the new transaction
                    }
                    else
                    {
                        csvWriter.WriteRecord(transaction); // Write the original transactions
                    }
                    await csvWriter.NextRecordAsync();
                }
            }

            // Replace the original file with the temporary file
            File.Delete(csvFilePath);
            File.Move(tempFilePath, csvFilePath);
        }
    }
}
