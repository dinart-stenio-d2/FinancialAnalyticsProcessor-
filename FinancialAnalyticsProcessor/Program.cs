using FinancialAnalyticsProcessor;
using FinancialAnalyticsProcessor.Domain.Interfaces.Repositories;
using FinancialAnalyticsProcessor.Infrastructure.Data;
using FinancialAnalyticsProcessor.Infrastructure.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<TransactionDbContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddHostedService<Worker>(); 
    });

await builder.Build().RunAsync();