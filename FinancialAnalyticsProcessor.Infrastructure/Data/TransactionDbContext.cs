using FinancialAnalyticsProcessor.Infrastructure.DbEntities;
using Microsoft.EntityFrameworkCore;

namespace FinancialAnalyticsProcessor.Infrastructure.Data
{
    public class TransactionDbContext : DbContext
    {
        public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>().HasKey(t => t.TransactionId);
        }
    }
}
