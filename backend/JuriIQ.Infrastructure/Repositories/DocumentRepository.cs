using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using JuriIQ.Core.Interfaces;
using JuriIQ.Core.Models;
using JuriIQ.Infrastructure.Data;

namespace JuriIQ.Infrastructure.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly DbContext _dbContext;

        public DocumentRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Document?> GetByIdAsync(Guid id)
        {
            using var connection = _dbContext.CreateConnection();
            var sql = "SELECT * FROM documents WHERE id = @Id";
            var document = await connection.QueryFirstOrDefaultAsync<Document>(sql, new { Id = id });
            
            if (document != null)
            {
                var keywordsSql = "SELECT * FROM document_keywords WHERE document_id = @DocumentId";
                var keywords = await connection.QueryAsync<DocumentKeyword>(keywordsSql, new { DocumentId = id });
                document.Keywords = keywords.ToList();
                
                var statsSql = "SELECT * FROM document_statistics WHERE document_id = @DocumentId";
                document.Statistics = await connection.QueryFirstOrDefaultAsync<DocumentStatistics>(statsSql, new { DocumentId = id });
            }
            
            return document;
        }

        public async Task<List<Document>> GetAllAsync()
        {
            using var connection = _dbContext.CreateConnection();
            var sql = "SELECT * FROM documents ORDER BY created_at DESC";
            var documents = await connection.QueryAsync<Document>(sql);
            return documents.ToList();
        }

        public async Task<List<Document>> GetPendingDocumentsAsync()
        {
            using var connection = _dbContext.CreateConnection();
            var sql = "SELECT * FROM documents WHERE processing_status = @Status ORDER BY created_at ASC";
            var documents = await connection.QueryAsync<Document>(sql, new { Status = ProcessingStatus.Pending.ToString().ToLower() });
            return documents.ToList();
        }

        public async Task<Guid> CreateAsync(Document document)
        {
            using var connection = _dbContext.CreateConnection();
            var sql = @"
                INSERT INTO documents (file_name, file_path, file_type, file_size, processing_status)
                VALUES (@FileName, @FilePath, @FileType, @FileSize, @ProcessingStatus)
                RETURNING id";
            
            return await connection.QuerySingleAsync<Guid>(sql, document);
        }

        public async Task UpdateAsync(Document document)
        {
            using var connection = _dbContext.CreateConnection();
            var sql = @"
                UPDATE documents 
                SET title = @Title, content = @Content, summary = @Summary,
                    court_name = @CourtName, case_number = @CaseNumber,
                    decision_date = @DecisionDate, document_type = @DocumentType,
                    processing_status = @ProcessingStatus, processing_error = @ProcessingError,
                    processed_at = @ProcessedAt, updated_at = CURRENT_TIMESTAMP
                WHERE id = @Id";
            
            await connection.ExecuteAsync(sql, document);
        }

        public async Task DeleteAsync(Guid id)
        {
            using var connection = _dbContext.CreateConnection();
            var sql = "DELETE FROM documents WHERE id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<List<Document>> SearchAsync(string query, int skip, int take)
        {
            using var connection = _dbContext.CreateConnection();
            var sql = @"
                SELECT d.* 
                FROM documents d
                WHERE d.processing_status = 'completed'
                  AND (
                    to_tsvector('english', d.title) @@ plainto_tsquery('english', @Query)
                    OR to_tsvector('english', d.content) @@ plainto_tsquery('english', @Query)
                    OR d.case_number ILIKE @LikeQuery
                  )
                ORDER BY d.created_at DESC
                OFFSET @Skip LIMIT @Take";
            
            var documents = await connection.QueryAsync<Document>(sql, new { 
                Query = query,
                LikeQuery = $"%{query}%",
                Skip = skip,
                Take = take
            });
            
            return documents.ToList();
        }
    }
}
