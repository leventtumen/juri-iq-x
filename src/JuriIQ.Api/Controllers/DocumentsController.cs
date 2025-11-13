using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using JuriIQ.Core.DTOs;
using JuriIQ.Core.Interfaces;

namespace JuriIQ.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(IDocumentService documentService, ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] SearchRequest request)
    {
        try
        {
            var userId = GetUserId();
            var response = await _documentService.SearchDocumentsAsync(request, userId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching documents");
            return StatusCode(500, new { message = "An error occurred while searching documents." });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocument(int id)
    {
        try
        {
            var userId = GetUserId();
            var document = await _documentService.GetDocumentDetailAsync(id, userId);

            if (document == null)
            {
                return NotFound(new { message = "Document not found." });
            }

            return Ok(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the document." });
        }
    }

    [HttpGet("{id}/related")]
    public async Task<IActionResult> GetRelatedDocuments(int id, [FromQuery] int limit = 5)
    {
        try
        {
            var relatedDocuments = await _documentService.GetRelatedDocumentsAsync(id, limit);
            return Ok(relatedDocuments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting related documents for {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving related documents." });
        }
    }

    [HttpPost("{id}/bookmark")]
    public async Task<IActionResult> BookmarkDocument(int id, [FromBody] BookmarkRequest? request)
    {
        try
        {
            var userId = GetUserId();
            await _documentService.BookmarkDocumentAsync(userId, id, request?.Notes);
            return Ok(new { message = "Document bookmarked successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bookmarking document {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while bookmarking the document." });
        }
    }

    [HttpDelete("{id}/bookmark")]
    public async Task<IActionResult> RemoveBookmark(int id)
    {
        try
        {
            var userId = GetUserId();
            await _documentService.RemoveBookmarkAsync(userId, id);
            return Ok(new { message = "Bookmark removed successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing bookmark for document {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while removing the bookmark." });
        }
    }

    [HttpGet("bookmarks")]
    public async Task<IActionResult> GetBookmarks()
    {
        try
        {
            var userId = GetUserId();
            var bookmarks = await _documentService.GetUserBookmarksAsync(userId);
            return Ok(bookmarks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user bookmarks");
            return StatusCode(500, new { message = "An error occurred while retrieving bookmarks." });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetSearchHistory([FromQuery] int limit = 10)
    {
        try
        {
            var userId = GetUserId();
            var history = await _documentService.GetSearchHistoryAsync(userId, limit);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search history");
            return StatusCode(500, new { message = "An error occurred while retrieving search history." });
        }
    }
}

public class BookmarkRequest
{
    public string? Notes { get; set; }
}
