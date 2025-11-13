using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using JuriIQ.Core.Interfaces;
using JuriIQ.Infrastructure.Data;
using JuriIQ.Infrastructure.Repositories;
using JuriIQ.AI.Services;
using JuriIQ.Scheduler.Jobs;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Register Database Connection Factory
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// Register Repositories
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentKeywordRepository, DocumentKeywordRepository>();

// Register Services
builder.Services.AddScoped<IDocumentProcessor, DocumentProcessor>();

// Configure Quartz
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    // Register the document processing job
    var jobKey = new JobKey("DocumentProcessingJob");

    q.AddJob<DocumentProcessingJob>(opts => opts.WithIdentity(jobKey));

    // Create trigger to run on startup
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("DocumentProcessingJob-StartupTrigger")
        .StartNow());

    // Create trigger for daily scheduled processing
    var scheduleCron = builder.Configuration["DocumentProcessing:ScheduleCron"] ?? "0 0 2 * * ?"; // 2 AM daily default

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("DocumentProcessingJob-DailyTrigger")
        .WithCronSchedule(scheduleCron));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await host.RunAsync();
