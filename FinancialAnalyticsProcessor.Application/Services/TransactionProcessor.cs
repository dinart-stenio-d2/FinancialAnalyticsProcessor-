using FinancialAnalyticsProcessor.Domain.Entities;
using FinancialAnalyticsProcessor.Domain.Interfaces.ApplicationServices;
using FinancialAnalyticsProcessor.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace FinancialAnalyticsProcessor.Application.Services
{
    public class TransactionProcessor : ITransactionProcessor
    {
        private readonly IRepository<Transaction> _repository;
        private readonly ILogger<TransactionProcessor> _logger;

        public TransactionProcessor(IRepository<Transaction> repository, ILogger<TransactionProcessor> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task ProcessTransactionsAsync(IEnumerable<Transaction> transactions)
        {
            _logger.LogInformation("Starting transaction processing...");

            if (transactions == null || !transactions.Any())
            {
                _logger.LogWarning("No transactions provided for processing.");
                throw new ArgumentException("No transactions to process.");
            }

            try
            {
                var transactionCount = transactions.Count();
                _logger.LogInformation("Processing {TransactionCount} transactions.", transactionCount);

                await _repository.BulkInsertAsync(transactions);

                _logger.LogInformation("Successfully processed {TransactionCount} transactions.", transactionCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing transactions.");
                throw; // Re-throw the exception to propagate it up the call stack
            }
        }
    }
}
