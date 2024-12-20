using FinancialAnalyticsProcessor.Domain.Interfaces;
using FinancialAnalyticsProcessor.Domain.Interfaces.ApplicationServices;

namespace FinancialAnalyticsProcessor.Worker.Jobs
{
    public class TransactionJob
    {
        private readonly ICsvTransactionLoader _loader;
        private readonly ITransactionProcessor _processor;

        public TransactionJob(ICsvTransactionLoader loader, ITransactionProcessor processor)
        {
            _loader = loader;
            _processor = processor;
        }

        public async Task ExecuteAsync(string csvFilePath, string outputPath)
        {
            using var stream = new FileStream(csvFilePath, FileMode.Open, FileAccess.Read);
            var transactions =  await _loader.LoadTransactionsAsync(stream);

             await _processor.ProcessTransactionsAsync(transactions);

            //TODO: implement this logic using automapper and fluent validations 
            //var analysis = await _processor.ProcessTransactionsAsync();
            //await _processor.SaveReportAsync(analysis, outputPath);
        }
    }
}
