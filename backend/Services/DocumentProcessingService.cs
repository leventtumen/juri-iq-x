using JurisIQ.Backend.Data;
using JurisIQ.Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
// using Python.Runtime;
using System.Collections.Concurrent;

namespace JurisIQ.Backend.Services
{
    public interface IDocumentProcessingService
    {
        Task ProcessAllDocumentsAsync();
        Task ProcessDocumentAsync(Document document);
        Task<List<Document>> SearchDocumentsAsync(string query, int page = 1, int pageSize = 20, string? documentType = null, string? courtName = null, DateTime? dateFrom = null, DateTime? dateTo = null);
        Task<Document?> GetDocumentByIdAsync(int id);
        Task<List<DocumentKeyword>> ExtractKeywordsAsync(string text);
        Task<string> GenerateSummaryAsync(string text);
        Task<double> CalculateRelevanceAsync(string query, string documentText);
    }

    public class DocumentProcessingService : IDocumentProcessingService
    {
        private readonly JurisIQDbContext _context;
        private readonly string _sampleDocumentsPath;
        private readonly string[] _supportedExtensions;
        private readonly ConcurrentDictionary<string, object> _pythonLocks = new();

        public DocumentProcessingService(JurisIQDbContext _context, string sampleDocumentsPath, string supportedExtensions)
        {
            this._context = _context;
            _sampleDocumentsPath = sampleDocumentsPath;
            _supportedExtensions = supportedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }

        public async Task ProcessAllDocumentsAsync()
        {
            if (!Directory.Exists(_sampleDocumentsPath))
            {
                Console.WriteLine($"Sample documents directory not found: {_sampleDocumentsPath}");
                return;
            }

            var files = Directory.GetFiles(_sampleDocumentsPath, "*.*", SearchOption.AllDirectories)
                .Where(f => _supportedExtensions.Contains(Path.GetExtension(f).ToLower()));

            foreach (var filePath in files)
            {
                try
                {
                    var existingDoc = await _context.Documents
                        .FirstOrDefaultAsync(d => d.FilePath == filePath);

                    if (existingDoc == null || existingDoc.UpdatedAt < File.GetLastWriteTime(filePath))
                    {
                        await ProcessDocumentFileAsync(filePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
                }
            }
        }

        public async Task ProcessDocumentAsync(Document document)
        {
            if (File.Exists(document.FilePath))
            {
                await ProcessDocumentFileAsync(document.FilePath);
            }
        }

        private async Task ProcessDocumentFileAsync(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var fileExtension = fileInfo.Extension.ToLower();
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            // Extract text based on file type
            var text = await ExtractTextFromFileAsync(filePath, fileExtension);
            
            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine($"No text extracted from file: {filePath}");
                return;
            }

            // Check if document exists
            var existingDoc = await _context.Documents
                .FirstOrDefaultAsync(d => d.FilePath == filePath);

            if (existingDoc != null)
            {
                // Update existing document
                existingDoc.Title = fileName;
                existingDoc.FullText = text;
                existingDoc.UpdatedAt = DateTime.UtcNow;
                existingDoc.FileSize = fileInfo.Length;
            }
            else
            {
                // Create new document
                existingDoc = new Document
                {
                    Title = fileName,
                    FileType = fileExtension.Trim('.'),
                    FilePath = filePath,
                    FullText = text,
                    FileSize = fileInfo.Length,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Documents.Add(existingDoc);
            }

            // Extract metadata from filename
            ExtractMetadataFromFileName(fileName, existingDoc);

            await _context.SaveChangesAsync();

            // Process AI analysis
            await ProcessDocumentAnalysisAsync(existingDoc);
        }

        private void ExtractMetadataFromFileName(string fileName, Document document)
        {
            // Try to extract court name and case number from filename
            var patterns = new[]
            {
                @"CASEOF(.+?)TURKEY",
                @"(\d+)-(\d+)",
                @"D\.?\s*(\d+)-(\d+)",
                @"(\d{4})"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(fileName.ToUpper(), pattern);
                if (match.Success)
                {
                    if (pattern.Contains("CASEOF"))
                    {
                        document.CourtName = match.Groups[1].Value.Trim();
                    }
                    else if (match.Groups.Count == 3)
                    {
                        document.CaseNumber = match.Groups[1].Value + "-" + match.Groups[2].Value;
                        
                        // Try to extract year
                        if (int.TryParse(match.Groups[2].Value, out int year) && year > 1900 && year < DateTime.Now.Year + 1)
                        {
                            document.DecisionDate = new DateTime(year, 1, 1);
                        }
                    }
                    break;
                }
            }
        }

        private async Task<string> ExtractTextFromFileAsync(string filePath, string extension)
        {
            try
            {
                switch (extension)
                {
                    case ".pdf":
                        return await ExtractPdfTextAsync(filePath);
                    case ".doc":
                    case ".docx":
                        return await ExtractDocTextAsync(filePath);
                    case ".txt":
                        return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                    case ".dot":
                        return await ExtractDocTextAsync(filePath); // Treat .dot as .doc
                    default:
                        return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting text from {filePath}: {ex.Message}");
                return string.Empty;
            }
        }

        private async Task<string> ExtractPdfTextAsync(string filePath)
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "pdftotext",
                        Arguments = $"-layout -enc UTF-8 &quot;{filePath}&quot; -",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();
                
                return output;
            }
            catch
            {
                // Fallback to PyPDF2 if pdftotext is not available
                return await ExtractPdfWithPyPdf2Async(filePath);
            }
        }

        private async Task<string> ExtractPdfWithPyPdf2Async(string filePath)
        {
            try
            {
                // Fallback: try to read as text file or return error
                Console.WriteLine($"PDF extraction not available for: {filePath}");
                return $"PDF file: {Path.GetFileName(filePath)} - Extraction not available without Python runtime";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting PDF with PyPdf2: {ex.Message}");
                return string.Empty;
            }
        }

        private async Task<string> ExtractDocTextAsync(string filePath)
        {
            try
            {
                // Fallback: try to read as text file or return error
                Console.WriteLine($"DOC/DOCX extraction not available for: {filePath}");
                return $"Document file: {Path.GetFileName(filePath)} - Extraction not available without Python runtime";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting DOC/DOCX: {ex.Message}");
                return string.Empty;
            }
        }

        private async Task ProcessDocumentAnalysisAsync(Document document)
        {
            try
            {
                // Generate summary
                if (!string.IsNullOrEmpty(document.FullText))
                {
                    var summary = await GenerateSummaryAsync(document.FullText);
                    document.Summary = summary;

                    // Extract keywords
                    var keywords = await ExtractKeywordsAsync(document.FullText);
                    
                    // Clear existing keywords
                    var existingKeywords = await _context.DocumentKeywords
                        .Where(k => k.DocumentId == document.Id)
                        .ToListAsync();
                    
                    _context.DocumentKeywords.RemoveRange(existingKeywords);

                    // Add new keywords
                    foreach (var keyword in keywords.Take(20)) // Limit to top 20 keywords
                    {
                        _context.DocumentKeywords.Add(new DocumentKeyword
                        {
                            DocumentId = document.Id,
                            Keyword = keyword.Keyword,
                            RelevanceScore = keyword.RelevanceScore,
                            Frequency = keyword.Frequency
                        });
                    }

                    document.ProcessedAt = DateTime.UtcNow;
                    document.IsProcessed = true;
                    
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing document analysis for {document.Id}: {ex.Message}");
            }
        }

        public async Task<List<DocumentKeyword>> ExtractKeywordsAsync(string text)
        {
            Console.WriteLine("Using fallback keyword extraction without Python runtime");
            
            // Fallback to simple word frequency
            var keywords = ExtractSimpleKeywords(text);
            
            return keywords.OrderByDescending(k => k.RelevanceScore).ToList();
        }

        private List<DocumentKeyword> ExtractSimpleKeywords(string text)
        {
            var keywords = new List<DocumentKeyword>();
            var words = Regex.Split(text.ToLower(), @"\W+")
                .Where(w => w.Length > 3)
                .GroupBy(w => w, StringComparer.OrdinalIgnoreCase)
                .Select(g => new { Word = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(20);

            foreach (var word in words)
            {
                keywords.Add(new DocumentKeyword
                {
                    Keyword = word.Word,
                    RelevanceScore = (double)word.Count / text.Split(' ').Length,
                    Frequency = word.Count
                });
            }

            return keywords;
        }

        public async Task<string> GenerateSummaryAsync(string text)
        {
            try
            {
                // Simple extractive summarization
                var sentences = Regex.Split(text, @"(?<=[.!?])\s+")
                    .Where(s => s.Length > 20)
                    .ToList();

                if (sentences.Count <= 3)
                    return string.Join(" ", sentences);

                // Simple approach: return first few sentences
                return string.Join(" ", sentences.Take(3));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating summary: {ex.Message}");
                return text.Length > 500 ? text.Substring(0, 500) + "..." : text;
            }
        }

        public async Task<double> CalculateRelevanceAsync(string query, string documentText)
        {
            try
            {
                var queryWords = Regex.Split(query.ToLower(), @"\W+").Where(w => w.Length > 2).ToList();
                var docWords = Regex.Split(documentText.ToLower(), @"\W+").ToList();
                
                if (!queryWords.Any()) return 0;
                
                var matchingWords = queryWords.Count(qw => docWords.Contains(qw, StringComparer.OrdinalIgnoreCase));
                var relevance = (double)matchingWords / queryWords.Count;
                
                // Boost relevance for exact phrase matches
                if (documentText.ToLower().Contains(query.ToLower()))
                {
                    relevance = Math.Min(1.0, relevance + 0.3);
                }
                
                return relevance;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<List<Document>> SearchDocumentsAsync(string query, int page = 1, int pageSize = 20, string? documentType = null, string? courtName = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var queryLower = query.ToLower();
            
            var documentsQuery = _context.Documents
                .Include(d => d.DocumentKeywords)
                .Where(d => d.IsProcessed && !string.IsNullOrEmpty(d.FullText));

            // Apply filters
            if (!string.IsNullOrEmpty(documentType))
                documentsQuery = documentsQuery.Where(d => d.FileType == documentType);

            if (!string.IsNullOrEmpty(courtName))
                documentsQuery = documentsQuery.Where(d => d.CourtName != null && d.CourtName.ToLower().Contains(courtName.ToLower()));

            if (dateFrom.HasValue)
                documentsQuery = documentsQuery.Where(d => d.DecisionDate >= dateFrom);

            if (dateTo.HasValue)
                documentsQuery = documentsQuery.Where(d => d.DecisionDate <= dateTo);

            var allDocuments = await documentsQuery.ToListAsync();
            
            // Calculate relevance for each document
            var documentsWithRelevance = new List<(Document doc, double relevance)>();
            
            foreach (var doc in allDocuments)
            {
                var relevance = await CalculateRelevanceAsync(queryLower, doc.FullText);
                
                // Boost relevance for keyword matches
                var keywordMatches = doc.DocumentKeywords
                    .Where(k => queryLower.Contains(k.Keyword.ToLower()) || k.Keyword.ToLower().Contains(queryLower))
                    .Sum(k => k.RelevanceScore);
                
                relevance += keywordMatches * 0.5;
                
                if (relevance > 0)
                {
                    documentsWithRelevance.Add((doc, relevance));
                }
            }
            
            return documentsWithRelevance
                .OrderByDescending(d => d.relevance)
                .Select(d => d.doc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public async Task<Document?> GetDocumentByIdAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.DocumentKeywords)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}