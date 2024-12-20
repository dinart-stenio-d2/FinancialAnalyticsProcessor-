using AutoMapper;
using FinancialAnalyticsProcessor.Application.Services;
using FinancialAnalyticsProcessor.Configurations;
using FinancialAnalyticsProcessor.Domain.Interfaces;
using FinancialAnalyticsProcessor.Domain.Interfaces.ApplicationServices;
using FinancialAnalyticsProcessor.Domain.Interfaces.Repositories;
using FinancialAnalyticsProcessor.Domain.Validations;
using FinancialAnalyticsProcessor.FaultResiliencePolicies;
using FinancialAnalyticsProcessor.Infrastructure.Data;
using FinancialAnalyticsProcessor.Infrastructure.Repositories.Generic;
using FinancialAnalyticsProcessor.Infrastructure.Services;
using FinancialAnalyticsProcessor.Mappings;
using FinancialAnalyticsProcessor.Worker.Jobs;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Polly.Retry;
using Serilog;
using Serilog.Events;

var builder = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, services, configuration) =>
    {
        // Configures Serilog to log to the console
        configuration
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    })
    .ConfigureServices((context, services) =>
    {
        // DbContext Configuration
        services.AddDbContext<TransactionDbContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

        // FluentValidation Configuration
        services.AddValidatorsFromAssemblyContaining<TransactionValidator>();
        services.AddValidatorsFromAssemblyContaining<TransactionListValidator>();
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();

        // AutoMapper Configuration
        var mappings = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new TransactionMappingProfile());
        });

        mappings.AssertConfigurationIsValid();
        var mapper = mappings.CreateMapper();

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddSingleton(mapper);
        services.AddSingleton(mappings);

        // Add configuration for the Cron Job
        var jobConfig = context.Configuration.GetSection("JobSchedule").Get<JobScheduleConfig>();
        services.AddSingleton(jobConfig);

        // Hangfire Configuration
        services.AddHangfire(config =>
            config.UseSqlServerStorage(context.Configuration.GetConnectionString("HangfireConnection")));
        services.AddHangfireServer();

        // Service Registration
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ITransactionProcessor, TransactionProcessor>();
        services.AddScoped<ICsvTransactionLoader, CsvTransactionLoader>();

        services.AddSingleton<AsyncRetryPolicy>(provider =>
        {
            // Configures Polly policy
            var csvTransactionLoader = provider.GetRequiredService<ICsvTransactionLoader>();
            var logger = provider.GetRequiredService<ILogger<TransactionJob>>();
            return PollyPolicy.CreateRetryPolicy(csvTransactionLoader, logger);
        });

        services.AddScoped<TransactionJob>();
    })
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.Configure(app =>
        {
            // Retrieves Cron Job Schedule
            var jobConfig = app.ApplicationServices.GetService<JobScheduleConfig>();

            // Configures Hangfire Dashboard
            app.UseHangfireDashboard("/hangfire");

            //// Retrieves input and output file directories from appsettings.json
            //var config = app.ApplicationServices.GetService<IConfiguration>();
            //var inputDirectory = config["DirectoryPaths:InputDirectory"];
            //var outputDirectory = config["DirectoryPaths:OutputDirectory"];

            //// Creates full file paths
            //var baseDirectory = Directory.GetCurrentDirectory();
            //var inputFilePath = Path.Combine(baseDirectory, inputDirectory, "input.csv");
            //var outputFilePath = Path.Combine(baseDirectory, outputDirectory, "output.json");

            //// Ensures directories exist
            //Directory.CreateDirectory(Path.Combine(baseDirectory, inputDirectory));
            //Directory.CreateDirectory(Path.Combine(baseDirectory, outputDirectory));
            
            // Fallback to a default cron expression if the interval is invalid
            var cronExpression = jobConfig.IntervalInSeconds >= 60
              ? $"*/{jobConfig.IntervalInSeconds / 60} * * * *"
              : "*/1 * * * *"; // Run every minute if invalid

            // Schedule the job
            RecurringJob.AddOrUpdate<TransactionJob>(
                "process-transactions",
                job => job.ExecuteAsync(jobConfig.InputFilePath, jobConfig.OutputFilePath),
                cronExpression
            );
        });
    });

await builder.Build().RunAsync();
