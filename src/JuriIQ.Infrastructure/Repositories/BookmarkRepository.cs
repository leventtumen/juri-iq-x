using Dapper;
using JuriIQ.Core.Entities;
using JuriIQ.Core.Interfaces;
using JuriIQ.Core.DTOs;
using JuriIQ.Infrastructure.Data;

namespace JuriIQ.Infrastructure.Repositories;

public class BookmarkRepository : IBookmarkRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BookmarkRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Bookmark?> GetByUserAndDocumentAsync(int userId, int documentId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id AS Id, user_id AS UserId, document_id AS DocumentId,
                   notes AS Notes, created_at AS CreatedAt
            FROM bookmarks
            WHERE user_id = @UserId AND document_id = @DocumentId";

        return await connection.QueryFirstOrDefaultAsync<Bookmark>(sql, new { UserId = userId, DocumentId = documentId });
    }

    public async Task<List<BookmarkDto>> GetUserBookmarksAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT b.id AS Id, b.document_id AS DocumentId, d.title AS DocumentTitle,
                   d.document_type AS DocumentType, b.notes AS Notes, b.created_at AS CreatedAt
            FROM bookmarks b
            INNER JOIN documents d ON b.document_id = d.id
            WHERE b.user_id = @UserId
            ORDER BY b.created_at DESC";

        var bookmarks = await connection.QueryAsync<BookmarkDto>(sql, new { UserId = userId });
        return bookmarks.ToList();
    }

    public async Task<int> CreateAsync(Bookmark bookmark)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO bookmarks (user_id, document_id, notes, created_at)
            VALUES (@UserId, @DocumentId, @Notes, @CreatedAt)
            RETURNING id";

        var bookmarkId = await connection.ExecuteScalarAsync<int>(sql, bookmark);

        // Increment bookmark count on document
        const string updateSql = "UPDATE documents SET bookmark_count = bookmark_count + 1 WHERE id = @DocumentId";
        await connection.ExecuteAsync(updateSql, new { bookmark.DocumentId });

        return bookmarkId;
    }

    public async Task DeleteAsync(int userId, int documentId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM bookmarks WHERE user_id = @UserId AND document_id = @DocumentId";
        await connection.ExecuteAsync(sql, new { UserId = userId, DocumentId = documentId });

        // Decrement bookmark count on document
        const string updateSql = @"
            UPDATE documents
            SET bookmark_count = GREATEST(bookmark_count - 1, 0)
            WHERE id = @DocumentId";
        await connection.ExecuteAsync(updateSql, new { DocumentId = documentId });
    }

    public async Task<bool> ExistsAsync(int userId, int documentId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT COUNT(1) FROM bookmarks
            WHERE user_id = @UserId AND document_id = @DocumentId";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, DocumentId = documentId });
        return count > 0;
    }
}
