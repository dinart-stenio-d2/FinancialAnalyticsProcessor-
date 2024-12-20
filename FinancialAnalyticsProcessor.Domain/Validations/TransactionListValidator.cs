using FinancialAnalyticsProcessor.Domain.Entities;
using FluentValidation;

namespace FinancialAnalyticsProcessor.Domain.Validations
{
    public class TransactionListValidator : AbstractValidator<IEnumerable<Transaction>>
    {
        public TransactionListValidator()
        {
            RuleForEach(transactions => transactions)
                .SetValidator(new TransactionValidator())
                .WithMessage("One or more transactions are invalid.");
        }
    }
}
