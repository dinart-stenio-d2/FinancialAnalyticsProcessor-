using FinancialAnalyticsProcessor.Domain.Entities;

namespace FinancialAnalyticsProcessor.Domain.Interfaces
{
    public interface ICsvTransactionLoader
    {
        Task<IEnumerable<Transaction>> LoadTransactionsAsync(Stream csvStream);
    }
}
