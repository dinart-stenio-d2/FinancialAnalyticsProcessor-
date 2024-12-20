using FinancialAnalyticsProcessor.Domain.Entities;

namespace FinancialAnalyticsProcessor.Domain.Interfaces.ApplicationServices
{
    public interface ITransactionProcessor
    {
        Task ProcessTransactionsAsync(IEnumerable<Transaction> transactions);
    }
}
