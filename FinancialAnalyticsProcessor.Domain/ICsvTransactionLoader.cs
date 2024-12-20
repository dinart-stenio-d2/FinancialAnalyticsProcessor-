using FinancialAnalyticsProcessor.Domain.Entities;

namespace FinancialAnalyticsProcessor.Domain
{
    public interface ICsvTransactionLoader
    {
        Task<IEnumerable<Transaction>> LoadTransactionsAsync(Stream csvStream);
    }
}
