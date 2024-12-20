using FinancialAnalyticsProcessor.Domain.Entities;

namespace FinancialAnalyticsProcessor.Domain.Interfaces.ApplicationServices
{
    public interface ITransactionProcessor
    {
        Task ProcessTransactionsAsync(IEnumerable<Transaction> transactions);
        Task<dynamic> PerformAnalysisAsync();
        Task SaveReportAsync(dynamic report, string outputPath);

    }
}
