using JuriIQ.Core.Entities;
using JuriIQ.Core.DTOs;

namespace JuriIQ.Core.Interfaces;

public interface IBookmarkRepository
{
    Task<Bookmark?> GetByUserAndDocumentAsync(int userId, int documentId);
    Task<List<BookmarkDto>> GetUserBookmarksAsync(int userId);
    Task<int> CreateAsync(Bookmark bookmark);
    Task DeleteAsync(int userId, int documentId);
    Task<bool> ExistsAsync(int userId, int documentId);
}
