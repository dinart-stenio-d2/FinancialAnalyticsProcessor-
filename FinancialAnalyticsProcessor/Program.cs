using FinancialAnalyticsProcessor;
using FinancialAnalyticsProcessor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<TransactionDbContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));


        services.AddHostedService<Worker>(); 
    });

await builder.Build().RunAsync();