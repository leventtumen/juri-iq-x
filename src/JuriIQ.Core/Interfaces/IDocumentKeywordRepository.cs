using JuriIQ.Core.Entities;

namespace JuriIQ.Core.Interfaces;

public interface IDocumentKeywordRepository
{
    Task<List<DocumentKeyword>> GetByDocumentIdAsync(int documentId);
    Task CreateBatchAsync(List<DocumentKeyword> keywords);
    Task DeleteByDocumentIdAsync(int documentId);
}
