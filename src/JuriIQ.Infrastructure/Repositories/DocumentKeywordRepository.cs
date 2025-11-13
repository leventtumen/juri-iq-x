using Dapper;
using JuriIQ.Core.Entities;
using JuriIQ.Core.Interfaces;
using JuriIQ.Infrastructure.Data;

namespace JuriIQ.Infrastructure.Repositories;

public class DocumentKeywordRepository : IDocumentKeywordRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DocumentKeywordRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<DocumentKeyword>> GetByDocumentIdAsync(int documentId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id AS Id, document_id AS DocumentId, keyword AS Keyword,
                   relevance AS Relevance, created_at AS CreatedAt
            FROM document_keywords
            WHERE document_id = @DocumentId
            ORDER BY relevance DESC";

        var keywords = await connection.QueryAsync<DocumentKeyword>(sql, new { DocumentId = documentId });
        return keywords.ToList();
    }

    public async Task CreateBatchAsync(List<DocumentKeyword> keywords)
    {
        if (!keywords.Any()) return;

        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO document_keywords (document_id, keyword, relevance, created_at)
            VALUES (@DocumentId, @Keyword, @Relevance, @CreatedAt)";

        await connection.ExecuteAsync(sql, keywords);
    }

    public async Task DeleteByDocumentIdAsync(int documentId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM document_keywords WHERE document_id = @DocumentId";
        await connection.ExecuteAsync(sql, new { DocumentId = documentId });
    }
}
