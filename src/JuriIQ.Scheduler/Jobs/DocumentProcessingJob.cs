using Quartz;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using JuriIQ.Core.Entities;
using JuriIQ.Core.Enums;
using JuriIQ.Core.Interfaces;

namespace JuriIQ.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class DocumentProcessingJob : IJob
{
    private readonly ILogger<DocumentProcessingJob> _logger;
    private readonly IConfiguration _configuration;
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentKeywordRepository _keywordRepository;
    private readonly IDocumentProcessor _documentProcessor;

    public DocumentProcessingJob(
        ILogger<DocumentProcessingJob> logger,
        IConfiguration configuration,
        IDocumentRepository documentRepository,
        IDocumentKeywordRepository keywordRepository,
        IDocumentProcessor documentProcessor)
    {
        _logger = logger;
        _configuration = configuration;
        _documentRepository = documentRepository;
        _keywordRepository = keywordRepository;
        _documentProcessor = documentProcessor;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting document processing job at {Time}", DateTime.UtcNow);

        var inputFolder = _configuration["DocumentProcessing:InputFolder"] ?? "/app/documents_to_process";
        var processedFolder = _configuration["DocumentProcessing:ProcessedFolder"] ?? "/app/documents_done";
        var failedFolder = _configuration["DocumentProcessing:FailedFolder"] ?? "/app/documents_failed";

        // Ensure folders exist
        Directory.CreateDirectory(inputFolder);
        Directory.CreateDirectory(processedFolder);
        Directory.CreateDirectory(failedFolder);

        var supportedExtensions = new[] { ".pdf", ".docx", ".doc", ".txt" };
        var files = Directory.GetFiles(inputFolder)
            .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .ToList();

        _logger.LogInformation("Found {Count} documents to process", files.Count);

        foreach (var filePath in files)
        {
            await ProcessDocument(filePath, processedFolder, failedFolder);
        }

        _logger.LogInformation("Document processing job completed at {Time}", DateTime.UtcNow);
    }

    private async Task ProcessDocument(string filePath, string processedFolder, string failedFolder)
    {
        var fileName = Path.GetFileName(filePath);
        _logger.LogInformation("Processing document: {FileName}", fileName);

        try
        {
            // Extract text from document
            var content = await _documentProcessor.ExtractTextFromFileAsync(filePath);

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new Exception("No content extracted from document");
            }

            // Generate summary
            var summary = await _documentProcessor.GenerateSummaryAsync(content);

            // Extract keywords
            var keywordsData = await _documentProcessor.ExtractKeywordsAsync(content);

            // Determine document type from filename or content analysis
            var documentType = DetermineDocumentType(fileName, content);

            // Create document record
            var document = new Document
            {
                Title = ExtractTitle(fileName, content),
                FileName = fileName,
                FilePath = filePath,
                FileExtension = Path.GetExtension(filePath),
                FileSize = new FileInfo(filePath).Length,
                DocumentType = documentType,
                Status = DocumentStatus.Completed,
                Content = content,
                Summary = summary,
                ProcessedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            // Save document to database
            var documentId = await _documentRepository.CreateAsync(document);

            // Save keywords
            var keywords = keywordsData.Select(k => new DocumentKeyword
            {
                DocumentId = documentId,
                Keyword = k.keyword,
                Relevance = k.relevance,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _keywordRepository.CreateBatchAsync(keywords);

            // Move file to processed folder
            var processedPath = Path.Combine(processedFolder, fileName);
            File.Move(filePath, processedPath, overwrite: true);

            _logger.LogInformation("Successfully processed document: {FileName}", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document: {FileName}", fileName);

            try
            {
                // Save failed document record
                var document = new Document
                {
                    Title = fileName,
                    FileName = fileName,
                    FilePath = filePath,
                    FileExtension = Path.GetExtension(filePath),
                    FileSize = new FileInfo(filePath).Length,
                    DocumentType = DocumentType.Decision,
                    Status = DocumentStatus.Failed,
                    Content = string.Empty,
                    ErrorMessage = ex.Message,
                    ProcessedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _documentRepository.CreateAsync(document);

                // Move file to failed folder
                var failedPath = Path.Combine(failedFolder, fileName);
                File.Move(filePath, failedPath, overwrite: true);
            }
            catch (Exception moveEx)
            {
                _logger.LogError(moveEx, "Error moving failed document: {FileName}", fileName);
            }
        }
    }

    private DocumentType DetermineDocumentType(string fileName, string content)
    {
        var lowerFileName = fileName.ToLowerInvariant();
        var lowerContent = content.ToLowerInvariant();

        if (lowerFileName.Contains("banka") || lowerContent.Contains("bankacılık"))
        {
            return DocumentType.BankingLaw;
        }

        if (lowerFileName.Contains("mevzuat") || lowerFileName.Contains("kanun") ||
            lowerContent.Contains("madde") && lowerContent.Contains("yasa"))
        {
            return DocumentType.Legislation;
        }

        return DocumentType.Decision;
    }

    private string ExtractTitle(string fileName, string content)
    {
        // Try to extract title from first line of content
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > 0)
        {
            var firstLine = lines[0].Trim();
            if (firstLine.Length > 10 && firstLine.Length < 200)
            {
                return firstLine;
            }
        }

        // Fallback to filename without extension
        return Path.GetFileNameWithoutExtension(fileName);
    }
}
