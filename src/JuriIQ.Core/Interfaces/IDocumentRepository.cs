using JuriIQ.Core.Entities;
using JuriIQ.Core.Enums;
using JuriIQ.Core.DTOs;

namespace JuriIQ.Core.Interfaces;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(int id);
    Task<int> CreateAsync(Document document);
    Task UpdateAsync(Document document);
    Task<List<Document>> SearchAsync(SearchRequest request);
    Task<int> GetSearchCountAsync(SearchRequest request);
    Task IncrementViewCountAsync(int documentId);
    Task<List<Document>> GetRelatedDocumentsAsync(int documentId, int limit);
}
