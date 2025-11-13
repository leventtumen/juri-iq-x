using System.Diagnostics;
using JuriIQ.Core.DTOs;
using JuriIQ.Core.Entities;
using JuriIQ.Core.Interfaces;

namespace JuriIQ.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentKeywordRepository _keywordRepository;
    private readonly IBookmarkRepository _bookmarkRepository;
    private readonly ISearchHistoryRepository _searchHistoryRepository;
    private readonly IDocumentProcessor _documentProcessor;

    public DocumentService(
        IDocumentRepository documentRepository,
        IDocumentKeywordRepository keywordRepository,
        IBookmarkRepository bookmarkRepository,
        ISearchHistoryRepository searchHistoryRepository,
        IDocumentProcessor documentProcessor)
    {
        _documentRepository = documentRepository;
        _keywordRepository = keywordRepository;
        _bookmarkRepository = bookmarkRepository;
        _searchHistoryRepository = searchHistoryRepository;
        _documentProcessor = documentProcessor;
    }

    public async Task<SearchResponse> SearchDocumentsAsync(SearchRequest request, int userId)
    {
        var stopwatch = Stopwatch.StartNew();

        // Get documents from repository
        var documents = await _documentRepository.SearchAsync(request);
        var totalCount = await _documentRepository.GetSearchCountAsync(request);

        // Get user's bookmarks for checking bookmark status
        var bookmarks = await _bookmarkRepository.GetUserBookmarksAsync(userId);
        var bookmarkedIds = bookmarks.Select(b => b.DocumentId).ToHashSet();

        // Calculate NLP similarity scores for each document
        var results = new List<SearchResultDto>();

        foreach (var doc in documents)
        {
            var similarity = string.IsNullOrWhiteSpace(request.Query)
                ? 1.0
                : _documentProcessor.CalculateSimilarity(request.Query, doc.Title + " " + doc.Content);

            results.Add(new SearchResultDto
            {
                Id = doc.Id,
                Title = doc.Title,
                DocumentType = doc.DocumentType,
                CourtName = doc.CourtName,
                CaseNumber = doc.CaseNumber,
                DecisionDate = doc.DecisionDate,
                LawNumber = doc.LawNumber,
                Category = doc.Category,
                Summary = doc.Summary ?? doc.Content.Substring(0, Math.Min(200, doc.Content.Length)) + "...",
                RelationPercentage = Math.Round(similarity * 100, 0), // Convert to percentage
                ViewCount = doc.ViewCount,
                BookmarkCount = doc.BookmarkCount,
                IsBookmarked = bookmarkedIds.Contains(doc.Id)
            });
        }

        // Sort by relation percentage (descending)
        results = results.OrderByDescending(r => r.RelationPercentage).ToList();

        stopwatch.Stop();

        // Save search history
        await _searchHistoryRepository.CreateAsync(new SearchHistory
        {
            UserId = userId,
            Query = request.Query,
            ResultCount = totalCount,
            CreatedAt = DateTime.UtcNow
        });

        return new SearchResponse
        {
            TotalResults = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            SearchTimeSeconds = stopwatch.Elapsed.TotalSeconds,
            Results = results
        };
    }

    public async Task<DocumentDetailDto?> GetDocumentDetailAsync(int documentId, int userId)
    {
        var document = await _documentRepository.GetByIdAsync(documentId);
        if (document == null)
        {
            return null;
        }

        // Increment view count
        await _documentRepository.IncrementViewCountAsync(documentId);

        // Get keywords
        var keywords = await _keywordRepository.GetByDocumentIdAsync(documentId);

        // Check if bookmarked by user
        var isBookmarked = await _bookmarkRepository.ExistsAsync(userId, documentId);

        // Get related documents
        var relatedDocs = await GetRelatedDocumentsAsync(documentId, 5);

        return new DocumentDetailDto
        {
            Id = document.Id,
            Title = document.Title,
            DocumentType = document.DocumentType,
            CourtName = document.CourtName,
            CaseNumber = document.CaseNumber,
            DecisionDate = document.DecisionDate,
            LawNumber = document.LawNumber,
            Category = document.Category,
            Content = document.Content,
            Summary = document.Summary,
            Keywords = keywords.Select(k => k.Keyword).ToList(),
            ViewCount = document.ViewCount + 1, // Include the current view
            BookmarkCount = document.BookmarkCount,
            IsBookmarked = isBookmarked,
            RelatedDocuments = relatedDocs
        };
    }

    public async Task<List<RelatedDocumentDto>> GetRelatedDocumentsAsync(int documentId, int limit = 5)
    {
        var relatedDocuments = await _documentRepository.GetRelatedDocumentsAsync(documentId, limit);

        var currentDocument = await _documentRepository.GetByIdAsync(documentId);
        if (currentDocument == null)
        {
            return new List<RelatedDocumentDto>();
        }

        var results = new List<RelatedDocumentDto>();

        foreach (var doc in relatedDocuments)
        {
            var similarity = _documentProcessor.CalculateSimilarity(
                currentDocument.Content,
                doc.Content
            );

            results.Add(new RelatedDocumentDto
            {
                Id = doc.Id,
                Title = doc.Title,
                DocumentType = doc.DocumentType,
                SimilarityScore = Math.Round(similarity * 100, 0)
            });
        }

        return results.OrderByDescending(r => r.SimilarityScore).ToList();
    }

    public async Task BookmarkDocumentAsync(int userId, int documentId, string? notes)
    {
        // Check if already bookmarked
        if (await _bookmarkRepository.ExistsAsync(userId, documentId))
        {
            throw new InvalidOperationException("Document is already bookmarked.");
        }

        var bookmark = new Bookmark
        {
            UserId = userId,
            DocumentId = documentId,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        await _bookmarkRepository.CreateAsync(bookmark);
    }

    public async Task RemoveBookmarkAsync(int userId, int documentId)
    {
        await _bookmarkRepository.DeleteAsync(userId, documentId);
    }

    public async Task<List<BookmarkDto>> GetUserBookmarksAsync(int userId)
    {
        return await _bookmarkRepository.GetUserBookmarksAsync(userId);
    }

    public async Task<List<SearchHistoryDto>> GetSearchHistoryAsync(int userId, int limit = 10)
    {
        return await _searchHistoryRepository.GetUserRecentSearchesAsync(userId, limit);
    }
}
