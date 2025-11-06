using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JurisIQ.Backend.Models.DTOs;
using JurisIQ.Backend.Services;
using JurisIQ.Backend.Data;
using JurisIQ.Backend.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace JurisIQ.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentProcessingService _documentService;
        private readonly JurisIQDbContext _context;

        public DocumentsController(IDocumentProcessingService documentService, JurisIQDbContext context)
        {
            _documentService = documentService;
            _context = context;
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<SearchResultDto>>> SearchDocuments([FromQuery] SearchRequest request)
        {
            try
            {
                var documents = await _documentService.SearchDocumentsAsync(
                    request.Query,
                    request.Page,
                    request.PageSize,
                    request.DocumentType,
                    request.CourtName,
                    request.DateFrom,
                    request.DateTo
                );

                var userId = GetCurrentUserId();
                
                var documentDtos = documents.Select(d => MapToDocumentDto(d, userId)).ToList();

                // Get total count for pagination
                var totalCount = await GetSearchResultCountAsync(request);

                var searchResult = new SearchResultDto
                {
                    Documents = documentDtos,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
                };

                // Log search history
                await LogSearchHistoryAsync(userId, request, documentDtos.Count);

                return Ok(ApiResponse<SearchResultDto>.SuccessResult(searchResult));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<SearchResultDto>.ErrorResult($"Search failed: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DocumentDetailDto>>> GetDocument(int id)
        {
            try
            {
                var document = await _documentService.GetDocumentByIdAsync(id);
                if (document == null)
                {
                    return NotFound(ApiResponse<DocumentDetailDto>.ErrorResult("Document not found"));
                }

                var userId = GetCurrentUserId();
                
                // Log document view
                await LogDocumentViewAsync(userId, id);

                var documentDetail = MapToDocumentDetailDto(document, userId);

                return Ok(ApiResponse<DocumentDetailDto>.SuccessResult(documentDetail));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<DocumentDetailDto>.ErrorResult($"Failed to get document: {ex.Message}"));
            }
        }

        [HttpPost("{id}/bookmark")]
        public async Task<ActionResult<ApiResponse<BookmarkDto>>> BookmarkDocument(int id, [FromBody] BookmarkRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var document = await _context.Documents.FindAsync(id);
                
                if (document == null)
                {
                    return NotFound(ApiResponse<BookmarkDto>.ErrorResult("Document not found"));
                }

                // Check if bookmark already exists
                var existingBookmark = await _context.Bookmarks
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.DocumentId == id);

                if (existingBookmark != null)
                {
                    return BadRequest(ApiResponse<BookmarkDto>.ErrorResult("Document already bookmarked"));
                }

                var bookmark = new Bookmark
                {
                    UserId = userId,
                    DocumentId = id,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Bookmarks.Add(bookmark);
                await _context.SaveChangesAsync();

                var bookmarkDto = new BookmarkDto
                {
                    Id = bookmark.Id,
                    DocumentId = bookmark.DocumentId,
                    DocumentTitle = document.Title,
                    Notes = bookmark.Notes,
                    CreatedAt = bookmark.CreatedAt
                };

                return Ok(ApiResponse<BookmarkDto>.SuccessResult(bookmarkDto, "Document bookmarked successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BookmarkDto>.ErrorResult($"Failed to bookmark document: {ex.Message}"));
            }
        }

        [HttpDelete("{id}/bookmark")]
        public async Task<ActionResult<ApiResponse<object>>> RemoveBookmark(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var bookmark = await _context.Bookmarks
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.DocumentId == id);

                if (bookmark == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Bookmark not found"));
                }

                _context.Bookmarks.Remove(bookmark);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResult(null, "Bookmark removed successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed to remove bookmark: {ex.Message}"));
            }
        }

        [HttpGet("bookmarks")]
        public async Task<ActionResult<ApiResponse<List<BookmarkDto>>>> GetBookmarks()
        {
            try
            {
                var userId = GetCurrentUserId();
                var bookmarks = await _context.Bookmarks
                    .Include(b => b.Document)
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                var bookmarkDtos = bookmarks.Select(b => new BookmarkDto
                {
                    Id = b.Id,
                    DocumentId = b.DocumentId,
                    DocumentTitle = b.Document.Title,
                    Notes = b.Notes,
                    CreatedAt = b.CreatedAt
                }).ToList();

                return Ok(ApiResponse<List<BookmarkDto>>.SuccessResult(bookmarkDtos));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BookmarkDto>>.ErrorResult($"Failed to get bookmarks: {ex.Message}"));
            }
        }

        [HttpPost("{id}/generate-summary")]
        public async Task<ActionResult<ApiResponse<string>>> GenerateSummary(int id)
        {
            try
            {
                var document = await _documentService.GetDocumentByIdAsync(id);
                if (document == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResult("Document not found"));
                }

                if (string.IsNullOrEmpty(document.FullText))
                {
                    return BadRequest(ApiResponse<string>.ErrorResult("Document text not available for summarization"));
                }

                var summary = await _documentService.GenerateSummaryAsync(document.FullText);

                // Update document with new summary
                document.Summary = summary;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<string>.SuccessResult(summary, "Summary generated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult($"Failed to generate summary: {ex.Message}"));
            }
        }

        [HttpGet("{id}/statistics")]
        public async Task<ActionResult<ApiResponse<object>>> GetDocumentStatistics(int id)
        {
            try
            {
                var document = await _context.Documents
                    .Include(d => d.Views)
                    .Include(d => d.Bookmarks)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (document == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Document not found"));
                }

                var statistics = new
                {
                    ViewCount = document.Views.Count,
                    BookmarkCount = document.Bookmarks.Count,
                    CreatedAt = document.CreatedAt,
                    ProcessedAt = document.ProcessedAt
                };

                return Ok(ApiResponse<object>.SuccessResult(statistics));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Failed to get statistics: {ex.Message}"));
            }
        }

        [HttpGet("{id}/related")]
        public async Task<ActionResult<ApiResponse<List<DocumentDto>>>> GetRelatedDocuments(int id, [FromQuery] int limit = 5)
        {
            try
            {
                var document = await _documentService.GetDocumentByIdAsync(id);
                if (document == null)
                {
                    return NotFound(ApiResponse<List<DocumentDto>>.ErrorResult("Document not found"));
                }

                if (string.IsNullOrEmpty(document.Keywords))
                {
                    return Ok(ApiResponse<List<DocumentDto>>.SuccessResult(new List<DocumentDto>()));
                }

                // Extract keywords from document
                var keywords = document.DocumentKeywords
                    .OrderByDescending(k => k.RelevanceScore)
                    .Take(5)
                    .Select(k => k.Keyword)
                    .ToList();

                if (!keywords.Any())
                {
                    return Ok(ApiResponse<List<DocumentDto>>.SuccessResult(new List<DocumentDto>()));
                }

                // Search for related documents using keywords
                var relatedDocuments = await _documentService.SearchDocumentsAsync(
                    string.Join(" ", keywords),
                    1,
                    limit + 1 // +1 to exclude the current document
                );

                var userId = GetCurrentUserId();

                // Filter out the current document and map to DTOs
                var relatedDtos = relatedDocuments
                    .Where(d => d.Id != id)
                    .Take(limit)
                    .Select(d => MapToDocumentDto(d, userId))
                    .ToList();

                return Ok(ApiResponse<List<DocumentDto>>.SuccessResult(relatedDtos));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<DocumentDto>>.ErrorResult($"Failed to get related documents: {ex.Message}"));
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 0;
        }

        private DocumentDto MapToDocumentDto(Document document, int userId)
        {
            return new DocumentDto
            {
                Id = document.Id,
                Title = document.Title,
                Description = document.Description,
                FileType = document.FileType,
                CourtName = document.CourtName,
                CaseNumber = document.CaseNumber,
                DecisionDate = document.DecisionDate,
                CreatedAt = document.CreatedAt,
                Summary = document.Summary,
                Keywords = document.Keywords,
                IsBookmarked = _context.Bookmarks.Any(b => b.UserId == userId && b.DocumentId == document.Id),
                ViewCount = document.Views.Count
            };
        }

        private DocumentDetailDto MapToDocumentDetailDto(Document document, int userId)
        {
            return new DocumentDetailDto
            {
                Id = document.Id,
                Title = document.Title,
                Description = document.Description,
                FileType = document.FileType,
                CourtName = document.CourtName,
                CaseNumber = document.CaseNumber,
                DecisionDate = document.DecisionDate,
                CreatedAt = document.CreatedAt,
                Summary = document.Summary,
                Keywords = document.Keywords,
                IsBookmarked = _context.Bookmarks.Any(b => b.UserId == userId && b.DocumentId == document.Id),
                ViewCount = document.Views.Count,
                FullText = document.FullText ?? string.Empty,
                DocumentKeywords = document.DocumentKeywords.Select(k => new DocumentKeywordDto
                {
                    Keyword = k.Keyword,
                    RelevanceScore = k.RelevanceScore,
                    Frequency = k.Frequency
                }).ToList()
            };
        }

        private async Task<int> GetSearchResultCountAsync(SearchRequest request)
        {
            // This is a simplified count - in a real implementation, you'd optimize this
            var allResults = await _documentService.SearchDocumentsAsync(request.Query, 1, int.MaxValue, request.DocumentType, request.CourtName, request.DateFrom, request.DateTo);
            return allResults.Count;
        }

        private async Task LogSearchHistoryAsync(int userId, SearchRequest request, int resultCount)
        {
            try
            {
                var searchHistory = new SearchHistory
                {
                    UserId = userId,
                    Query = request.Query,
                    Filter = request.DocumentType ?? request.CourtName,
                    ResultCount = resultCount,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    SearchedAt = DateTime.UtcNow
                };

                _context.SearchHistory.Add(searchHistory);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Log error but don't fail the request
            }
        }

        private async Task LogDocumentViewAsync(int userId, int documentId)
        {
            try
            {
                var documentView = new DocumentView
                {
                    UserId = userId,
                    DocumentId = documentId,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    ViewedAt = DateTime.UtcNow
                };

                _context.DocumentViews.Add(documentView);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Log error but don't fail the request
            }
        }
    }
}