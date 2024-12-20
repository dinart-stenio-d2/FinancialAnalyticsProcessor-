using FinancialAnalyticsProcessor.Domain.Entities;
using FluentValidation;

namespace FinancialAnalyticsProcessor.Domain.Validations
{
    public class TransactionValidator : AbstractValidator<Transaction>
    {
        public TransactionValidator()
        {
            RuleFor(t => t.TransactionId)
                .NotEmpty().WithMessage("TransactionId is required.");

            RuleFor(t => t.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(t => t.Date)
                .NotEmpty().WithMessage("Date is required.")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Date cannot be in the future.");

            RuleFor(t => t.Amount)
                .NotEmpty().WithMessage("Amount is required.")
                .PrecisionScale(18, 2, true).WithMessage("Amount must have up to 18 digits and 2 decimal places."); // Substituído

            RuleFor(t => t.Category)
                .NotEmpty().WithMessage("Category is required.")
                .MaximumLength(100).WithMessage("Category must not exceed 100 characters.");

            RuleFor(t => t.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(255).WithMessage("Description must not exceed 255 characters.");

            RuleFor(t => t.Merchant)
                .NotEmpty().WithMessage("Merchant is required.")
                .MaximumLength(100).WithMessage("Merchant must not exceed 100 characters.");
        }
    }
}
