using AutoMapper;
using FinancialAnalyticsProcessor.Domain.Entities;
using FinancialAnalyticsProcessor.Domain.Interfaces.ApplicationServices;
using FinancialAnalyticsProcessor.Domain.Interfaces.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace FinancialAnalyticsProcessor.Application.Services
{
    public class TransactionProcessor : ITransactionProcessor
    {
        private readonly IRepository<Infrastructure.DbEntities.Transaction> _repository;
        private readonly ILogger<TransactionProcessor> _logger;
        private readonly IMapper _mapper;
        private readonly IValidator<Transaction> _transactionValidator;
        private readonly IValidator<IEnumerable<Transaction>> _transactionListValidator;

        public TransactionProcessor(IRepository<Infrastructure.DbEntities.Transaction> repository, 
            ILogger<TransactionProcessor> logger , 
            IMapper mapper,
            IValidator<Transaction> transactionValidator,
            IValidator<IEnumerable<Transaction>> transactionListValidator)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _transactionValidator = transactionValidator;   
            _transactionListValidator = transactionListValidator;   
        }

        public async Task ProcessTransactionsAsync(IEnumerable<Transaction> transactions)
        {
            _logger.LogInformation("Starting transaction processing...");

            if (transactions == null || !transactions.Any())
            {
                _logger.LogWarning("No transactions provided for processing.");
                throw new ValidationException("No transactions to process.");
            }

            try
            {
                var transactionCount = transactions.Count();
                _logger.LogInformation("Processing {TransactionCount} transactions.", transactionCount);

                var validationResult = await _transactionListValidator.ValidateAsync(transactions);
                
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var transactionsTobeSaved = _mapper.Map<IEnumerable<Infrastructure.DbEntities.Transaction>>(transactions);

                await _repository.BulkInsertAsync(transactionsTobeSaved);

                _logger.LogInformation("Successfully processed {TransactionCount} transactions.", transactionCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing transactions.");
                throw; // Re-throw the exception to propagate it up the call stack
            }
        }

        public async Task<dynamic> PerformAnalysisAsync()
        {
            _logger.LogInformation("Starting transaction analysis...");

            try
            {
                var transactions = await _repository.GetAllAsync();

                var usersSummary = transactions
                    .GroupBy(t => t.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        TotalIncome = g.Where(t => t.Amount > 0).Sum(t => t.Amount),
                        TotalExpense = g.Where(t => t.Amount < 0).Sum(t => t.Amount)
                    }).ToList();

                var topCategories = transactions
                    .GroupBy(t => t.Category)
                    .OrderByDescending(g => g.Count())
                    .Take(3)
                    .Select(g => new { Category = g.Key, TransactionsCount = g.Count() })
                    .ToList();

                var highestSpender = transactions
                    .GroupBy(t => t.UserId)
                    .OrderBy(g => g.Sum(t => t.Amount))
                    .Select(g => new { UserId = g.Key })
                    .FirstOrDefault();

                _logger.LogInformation("Transaction analysis completed successfully.");

                return new
                {
                    UsersSummary = usersSummary,
                    TopCategories = topCategories,
                    HighestSpender = highestSpender
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during transaction analysis.");
                throw;
            }
        }

        public async Task SaveReportAsync(dynamic report, string outputPath)
        {
            _logger.LogInformation("Saving report to {OutputPath}...", outputPath);

            try
            {
                var json = JsonConvert.SerializeObject(report, Formatting.Indented);

                if (File.Exists(outputPath))
                {
                    _logger.LogWarning("File already exists at {OutputPath}. Overwriting the file.", outputPath);
                    File.Delete(outputPath);
                }

                await File.WriteAllTextAsync(outputPath, json);

                _logger.LogInformation("Report saved successfully to {OutputPath}.", outputPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the report.");
                throw;
            }
        }

    }
}
