using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JuriIQ.Core.Models;

namespace JuriIQ.Core.Interfaces
{
    public interface IDocumentRepository
    {
        Task<Document?> GetByIdAsync(Guid id);
        Task<List<Document>> GetAllAsync();
        Task<List<Document>> GetPendingDocumentsAsync();
        Task<Guid> CreateAsync(Document document);
        Task UpdateAsync(Document document);
        Task DeleteAsync(Guid id);
        Task<List<Document>> SearchAsync(string query, int skip, int take);
    }
}