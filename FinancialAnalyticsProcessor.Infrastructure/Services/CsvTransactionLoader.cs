using CsvHelper;
using CsvHelper.Configuration;
using FinancialAnalyticsProcessor.Domain;
using FinancialAnalyticsProcessor.Domain.Entities;
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

            // A leitura de CSV deve ser feita de forma assíncrona
            var records = await Task.Run(() => csv.GetRecords<Transaction>().ToList());
            return records;
        }
    }
}
