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
    }
}
