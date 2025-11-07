using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JuriIQ.Core.DTOs;
using JuriIQ.Core.Interfaces;
using System.Security.Claims;

namespace JuriIQ.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(IDocumentRepository documentRepository, ILogger<DocumentsController> logger)
        {
            _documentRepository = documentRepository;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(Guid id)
        {
            try
            {
                var document = await _documentRepository.GetByIdAsync(id);
                if (document == null)
                {
                    return NotFound(new { message = "Document not found" });
                }

                var dto = new DocumentDTO
                {
                    Id = document.Id,
                    FileName = document.FileName,
                    Title = document.Title,
                    Summary = document.Summary,
                    CourtName = document.CourtName,
                    CaseNumber = document.CaseNumber,
                    DecisionDate = document.DecisionDate,
                    DocumentType = document.DocumentType,
                    Keywords = document.Keywords.Select(k => k.Keyword).ToList(),
                    Statistics = document.Statistics != null ? new DocumentStatisticsDTO
                    {
                        WordCount = document.Statistics.WordCount,
                        SentenceCount = document.Statistics.SentenceCount,
                        ParagraphCount = document.Statistics.ParagraphCount,
                        PageCount = document.Statistics.PageCount
                    } : null
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document {DocumentId}", id);
                return StatusCode(500, new { message = "An error occurred retrieving the document" });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] SearchRequestDTO request)
        {
            try
            {
                var skip = (request.Page - 1) * request.PageSize;
                var documents = await _documentRepository.SearchAsync(request.Query, skip, request.PageSize);

                var results = documents.Select(d => new DocumentSearchResultDTO
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    Title = d.Title,
                    Summary = d.Summary,
                    CourtName = d.CourtName,
                    RelevanceScore = 1.0m, // Simplified - can be enhanced with actual NLP scoring
                    MatchedKeywords = d.Keywords.Take(5).Select(k => k.Keyword).ToList()
                }).ToList();

                var response = new SearchResponseDTO
                {
                    Results = results,
                    TotalResults = results.Count,
                    Page = request.Page,
                    PageSize = request.PageSize
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching documents with query {Query}", request.Query);
                return StatusCode(500, new { message = "An error occurred during search" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDocuments()
        {
            try
            {
                var documents = await _documentRepository.GetAllAsync();
                var dtos = documents.Select(d => new DocumentDTO
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    Title = d.Title,
                    Summary = d.Summary,
                    CourtName = d.CourtName,
                    CaseNumber = d.CaseNumber,
                    DecisionDate = d.DecisionDate,
                    DocumentType = d.DocumentType
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all documents");
                return StatusCode(500, new { message = "An error occurred retrieving documents" });
            }
        }
    }
}
