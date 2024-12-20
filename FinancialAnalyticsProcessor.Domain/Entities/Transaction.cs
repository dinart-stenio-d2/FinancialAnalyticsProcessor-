using FinancialAnalyticsProcessor.Core.Domain.DomainObjects;

namespace FinancialAnalyticsProcessor.Domain.Entities
{
    public class Transaction : Entity, IAggregateRoot  
    {
        public Guid UserId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Merchant { get; set; }

    }
}
