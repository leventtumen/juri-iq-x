using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using JuriIQ.Infrastructure.Data;
using JuriIQ.Infrastructure.Repositories;
using JuriIQ.Core.Interfaces;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Port=5432;Database=juriiq;Username=postgres;Password=postgres";

        // Register services
        services.AddSingleton(new DbContext(connectionString));
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        // Configure Quartz
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();

            // Create a job to process documents
            var jobKey = new JobKey("DocumentProcessorJob");
            q.AddJob<DocumentProcessorJob>(opts => opts.WithIdentity(jobKey));

            // Trigger the job every hour (or on startup)
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("DocumentProcessorTrigger")
                .WithCronSchedule("0 0 * * * ?") // Every hour
                .StartNow());
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .Build();

await host.RunAsync();

// Quartz Job for processing documents
public class DocumentProcessorJob : IJob
{
    private readonly ILogger<DocumentProcessorJob> _logger;
    private readonly IDocumentRepository _documentRepository;
    private const string DocumentsToProcessPath = "/app/documents_to_process";
    private const string DocumentsDonePath = "/app/documents_done";
    private const string DocumentsFailedPath = "/app/documents_failed";

    public DocumentProcessorJob(ILogger<DocumentProcessorJob> logger, IDocumentRepository documentRepository)
    {
        _logger = logger;
        _documentRepository = documentRepository;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Document processor job started at {Time}", DateTime.UtcNow);

        try
        {
            // Ensure directories exist
            Directory.CreateDirectory(DocumentsToProcessPath);
            Directory.CreateDirectory(DocumentsDonePath);
            Directory.CreateDirectory(DocumentsFailedPath);

            // Get all files to process
            var files = Directory.GetFiles(DocumentsToProcessPath, "*.*")
                .Where(f => new[] { ".pdf", ".doc", ".docx", ".dot", ".txt" }
                    .Contains(Path.GetExtension(f).ToLower()))
                .ToList();

            _logger.LogInformation("Found {Count} documents to process", files.Count);

            foreach (var filePath in files)
            {
                try
                {
                    await ProcessDocument(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing document {FilePath}", filePath);
                    MoveToFailed(filePath);
                }
            }

            _logger.LogInformation("Document processor job completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in document processor job");
        }
    }

    private async Task ProcessDocument(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        _logger.LogInformation("Processing document: {FileName}", fileName);

        // Create document record
        var document = new JuriIQ.Core.Models.Document
        {
            FileName = fileName,
            FilePath = filePath,
            FileType = Path.GetExtension(filePath).TrimStart('.').ToLower(),
            FileSize = new FileInfo(filePath).Length,
            ProcessingStatus = JuriIQ.Core.Models.ProcessingStatus.Processing
        };

        var documentId = await _documentRepository.CreateAsync(document);
        document.Id = documentId;

        try
        {
            // Extract text content (simplified - would use iText7, DocumentFormat.OpenXml, etc.)
            var content = await ExtractText(filePath);
            
            // Generate summary using basic text analysis
            var summary = GenerateSummary(content);
            
            // Update document with processed data
            document.Content = content;
            document.Summary = summary;
            document.Title = ExtractTitle(content, fileName);
            document.ProcessingStatus = JuriIQ.Core.Models.ProcessingStatus.Completed;
            document.ProcessedAt = DateTime.UtcNow;

            await _documentRepository.UpdateAsync(document);

            // Move to done folder
            var destPath = Path.Combine(DocumentsDonePath, fileName);
            File.Move(filePath, destPath, true);

            _logger.LogInformation("Successfully processed document: {FileName}", fileName);
        }
        catch (Exception ex)
        {
            document.ProcessingStatus = JuriIQ.Core.Models.ProcessingStatus.Failed;
            document.ProcessingError = ex.Message;
            await _documentRepository.UpdateAsync(document);
            throw;
        }
    }

    private async Task<string> ExtractText(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        
        // Simplified text extraction - in production, use proper libraries
        if (extension == ".txt")
        {
            return await File.ReadAllTextAsync(filePath);
        }
        
        // For other formats, would use iText7, DocumentFormat.OpenXml, etc.
        return $"[Content extracted from {Path.GetFileName(filePath)}]";
    }

    private string GenerateSummary(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        // Simple summary - take first 500 characters
        return content.Length > 500 ? content.Substring(0, 500) + "..." : content;
    }

    private string ExtractTitle(string content, string fileName)
    {
        // Try to extract title from content (first line) or use filename
        if (!string.IsNullOrWhiteSpace(content))
        {
            var firstLine = content.Split('\n').FirstOrDefault()?.Trim();
            if (!string.IsNullOrWhiteSpace(firstLine) && firstLine.Length < 200)
                return firstLine;
        }

        return Path.GetFileNameWithoutExtension(fileName);
    }

    private void MoveToFailed(string filePath)
    {
        try
        {
            var fileName = Path.GetFileName(filePath);
            var destPath = Path.Combine(DocumentsFailedPath, fileName);
            File.Move(filePath, destPath, true);
            _logger.LogWarning("Moved failed document to: {DestPath}", destPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving file to failed folder: {FilePath}", filePath);
        }
    }
}
