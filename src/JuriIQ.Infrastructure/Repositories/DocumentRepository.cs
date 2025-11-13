using Dapper;
using JuriIQ.Core.Entities;
using JuriIQ.Core.Interfaces;
using JuriIQ.Core.DTOs;
using JuriIQ.Infrastructure.Data;

namespace JuriIQ.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DocumentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Document?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id AS Id, title AS Title, file_name AS FileName, file_path AS FilePath,
                   file_extension AS FileExtension, file_size AS FileSize, document_type AS DocumentType,
                   status AS Status, court_name AS CourtName, case_number AS CaseNumber,
                   decision_date AS DecisionDate, law_number AS LawNumber, category AS Category,
                   content AS Content, summary AS Summary, error_message AS ErrorMessage,
                   view_count AS ViewCount, bookmark_count AS BookmarkCount,
                   processed_at AS ProcessedAt, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM documents WHERE id = @Id";

        return await connection.QueryFirstOrDefaultAsync<Document>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Document document)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO documents (title, file_name, file_path, file_extension, file_size, document_type,
                                 status, court_name, case_number, decision_date, law_number, category,
                                 content, summary, processed_at, created_at)
            VALUES (@Title, @FileName, @FilePath, @FileExtension, @FileSize, @DocumentType,
                   @Status, @CourtName, @CaseNumber, @DecisionDate, @LawNumber, @Category,
                   @Content, @Summary, @ProcessedAt, @CreatedAt)
            RETURNING id";

        return await connection.ExecuteScalarAsync<int>(sql, document);
    }

    public async Task UpdateAsync(Document document)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE documents
            SET title = @Title, status = @Status, court_name = @CourtName,
                case_number = @CaseNumber, decision_date = @DecisionDate, law_number = @LawNumber,
                category = @Category, content = @Content, summary = @Summary,
                error_message = @ErrorMessage, processed_at = @ProcessedAt, updated_at = @UpdatedAt
            WHERE id = @Id";

        await connection.ExecuteAsync(sql, document);
    }

    public async Task<List<Document>> SearchAsync(SearchRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        // Full-text search on title, content, and summary
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            whereConditions.Add(@"(
                to_tsvector('turkish', title) @@ plainto_tsquery('turkish', @Query) OR
                to_tsvector('turkish', content) @@ plainto_tsquery('turkish', @Query) OR
                to_tsvector('turkish', COALESCE(summary, '')) @@ plainto_tsquery('turkish', @Query)
            )");
            parameters.Add("Query", request.Query);
        }

        // Filter by document type
        if (request.DocumentType.HasValue)
        {
            whereConditions.Add("document_type = @DocumentType");
            parameters.Add("DocumentType", request.DocumentType.Value);
        }

        // Filter by court name
        if (!string.IsNullOrWhiteSpace(request.CourtName))
        {
            whereConditions.Add("court_name ILIKE @CourtName");
            parameters.Add("CourtName", $"%{request.CourtName}%");
        }

        // Filter by date range
        if (request.DateFrom.HasValue)
        {
            whereConditions.Add("decision_date >= @DateFrom");
            parameters.Add("DateFrom", request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            whereConditions.Add("decision_date <= @DateTo");
            parameters.Add("DateTo", request.DateTo.Value);
        }

        // Only return completed documents
        whereConditions.Add("status = 3"); // Completed

        var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";

        // Calculate offset for pagination
        var offset = (request.Page - 1) * request.PageSize;
        parameters.Add("PageSize", request.PageSize);
        parameters.Add("Offset", offset);

        var sql = $@"
            SELECT id AS Id, title AS Title, file_name AS FileName, file_path AS FilePath,
                   file_extension AS FileExtension, file_size AS FileSize, document_type AS DocumentType,
                   status AS Status, court_name AS CourtName, case_number AS CaseNumber,
                   decision_date AS DecisionDate, law_number AS LawNumber, category AS Category,
                   content AS Content, summary AS Summary,
                   view_count AS ViewCount, bookmark_count AS BookmarkCount,
                   processed_at AS ProcessedAt, created_at AS CreatedAt
            FROM documents
            {whereClause}
            ORDER BY processed_at DESC
            LIMIT @PageSize OFFSET @Offset";

        var documents = await connection.QueryAsync<Document>(sql, parameters);
        return documents.ToList();
    }

    public async Task<int> GetSearchCountAsync(SearchRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            whereConditions.Add(@"(
                to_tsvector('turkish', title) @@ plainto_tsquery('turkish', @Query) OR
                to_tsvector('turkish', content) @@ plainto_tsquery('turkish', @Query) OR
                to_tsvector('turkish', COALESCE(summary, '')) @@ plainto_tsquery('turkish', @Query)
            )");
            parameters.Add("Query", request.Query);
        }

        if (request.DocumentType.HasValue)
        {
            whereConditions.Add("document_type = @DocumentType");
            parameters.Add("DocumentType", request.DocumentType.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.CourtName))
        {
            whereConditions.Add("court_name ILIKE @CourtName");
            parameters.Add("CourtName", $"%{request.CourtName}%");
        }

        if (request.DateFrom.HasValue)
        {
            whereConditions.Add("decision_date >= @DateFrom");
            parameters.Add("DateFrom", request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            whereConditions.Add("decision_date <= @DateTo");
            parameters.Add("DateTo", request.DateTo.Value);
        }

        whereConditions.Add("status = 3");

        var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";
        var sql = $"SELECT COUNT(*) FROM documents {whereClause}";

        return await connection.ExecuteScalarAsync<int>(sql, parameters);
    }

    public async Task IncrementViewCountAsync(int documentId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE documents SET view_count = view_count + 1 WHERE id = @DocumentId";
        await connection.ExecuteAsync(sql, new { DocumentId = documentId });
    }

    public async Task<List<Document>> GetRelatedDocumentsAsync(int documentId, int limit)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Get related documents based on shared keywords
        const string sql = @"
            WITH doc_keywords AS (
                SELECT keyword FROM document_keywords WHERE document_id = @DocumentId
            )
            SELECT DISTINCT d.id AS Id, d.title AS Title, d.document_type AS DocumentType,
                   COUNT(dk.keyword) as shared_keywords
            FROM documents d
            INNER JOIN document_keywords dk ON d.id = dk.document_id
            WHERE dk.keyword IN (SELECT keyword FROM doc_keywords)
              AND d.id != @DocumentId
              AND d.status = 3
            GROUP BY d.id, d.title, d.document_type
            ORDER BY shared_keywords DESC, d.view_count DESC
            LIMIT @Limit";

        var documents = await connection.QueryAsync<Document>(sql, new { DocumentId = documentId, Limit = limit });
        return documents.ToList();
    }
}
