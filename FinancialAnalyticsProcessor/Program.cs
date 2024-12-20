using AutoMapper;
using FinancialAnalyticsProcessor.Application.Services;
using FinancialAnalyticsProcessor.Domain.Interfaces.ApplicationServices;
using FinancialAnalyticsProcessor.Domain.Interfaces.Repositories;
using FinancialAnalyticsProcessor.Domain.Validations;
using FinancialAnalyticsProcessor.Infrastructure.Data;
using FinancialAnalyticsProcessor.Infrastructure.Repositories.Generic;
using FinancialAnalyticsProcessor.Mappings;
using FinancialAnalyticsProcessor.Worker.Jobs;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

var builder = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, services, configuration) =>
    {
        // Configura o Serilog para logar no console
        configuration
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    })
    .ConfigureServices((context, services) =>
    {
        // Configuração do DbContext
        services.AddDbContext<TransactionDbContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

        // FluentValidation Configuration
        services.AddValidatorsFromAssemblyContaining<TransactionValidator>(); 
        services.AddValidatorsFromAssemblyContaining<TransactionListValidator>(); 
        services.AddFluentValidationAutoValidation(); 
        services.AddFluentValidationClientsideAdapters(); 

        // Configuração do AutoMapper
        var mappings = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new TransactionMappingProfile());
        });

        mappings.AssertConfigurationIsValid();
        var mapper = mappings.CreateMapper();

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddSingleton(mapper);
        services.AddSingleton(mappings);

        // Configuração do Hangfire
        services.AddHangfire(config =>
            config.UseSqlServerStorage(context.Configuration.GetConnectionString("HangfireConnection")));
        services.AddHangfireServer();

        // Registro de serviços
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ITransactionProcessor, TransactionProcessor>();
        services.AddScoped<TransactionJob>();
    })
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.Configure(app =>
        {
            // Configura o Hangfire Dashboard
            app.UseHangfireDashboard("/hangfire");

            // Recupera os diretórios dos arquivos de entrada e saída do appsettings.json
            var config = app.ApplicationServices.GetService<IConfiguration>();
            var inputDirectory = config["DirectoryPaths:InputDirectory"];
            var outputDirectory = config["DirectoryPaths:OutputDirectory"];

            // Cria os caminhos completos para os arquivos
            var baseDirectory = Directory.GetCurrentDirectory();
            var inputFilePath = Path.Combine(baseDirectory, inputDirectory, "input.csv");
            var outputFilePath = Path.Combine(baseDirectory, outputDirectory, "output.json");

            // Certifique-se de que os diretórios existem
            Directory.CreateDirectory(Path.Combine(baseDirectory, inputDirectory));
            Directory.CreateDirectory(Path.Combine(baseDirectory, outputDirectory));

            // Agenda o job
            RecurringJob.AddOrUpdate<TransactionJob>(
                "process-transactions",
                job => job.ExecuteAsync(inputFilePath, outputFilePath),
                Cron.Daily
            );
        });
    });

await builder.Build().RunAsync();