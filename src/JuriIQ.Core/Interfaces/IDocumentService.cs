using JuriIQ.Core.DTOs;

namespace JuriIQ.Core.Interfaces;

public interface IDocumentService
{
    Task<SearchResponse> SearchDocumentsAsync(SearchRequest request, int userId);
    Task<DocumentDetailDto?> GetDocumentDetailAsync(int documentId, int userId);
    Task<List<RelatedDocumentDto>> GetRelatedDocumentsAsync(int documentId, int limit = 5);
    Task BookmarkDocumentAsync(int userId, int documentId, string? notes);
    Task RemoveBookmarkAsync(int userId, int documentId);
    Task<List<BookmarkDto>> GetUserBookmarksAsync(int userId);
    Task<List<SearchHistoryDto>> GetSearchHistoryAsync(int userId, int limit = 10);
}
