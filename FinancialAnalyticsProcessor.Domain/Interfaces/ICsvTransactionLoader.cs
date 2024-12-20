using FinancialAnalyticsProcessor.Domain.Entities;

namespace FinancialAnalyticsProcessor.Domain.Interfaces
{
    public interface ICsvTransactionLoader
    {
       public Task<IEnumerable<Transaction>> LoadTransactionsAsync(Stream csvStream);
    }
}
