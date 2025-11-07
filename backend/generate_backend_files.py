#!/usr/bin/env python3
"""
Script to generate all backend files for Juri-IQ system
"""
import os

# Base directory
BASE_DIR = "/home/user/webapp/backend"

# File templates
FILES = {
    # Additional Core Models
    "JuriIQ.Core/Models/UserDevice.cs": '''using System;

namespace JuriIQ.Core.Models
{
    public class UserDevice
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string? DeviceName { get; set; }
        public string? DeviceType { get; set; }
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}''',

    "JuriIQ.Core/Models/SearchHistory.cs": '''using System;

namespace JuriIQ.Core.Models
{
    public class SearchHistory
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Query { get; set; } = string.Empty;
        public int ResultsCount { get; set; }
        public string? Filters { get; set; }
        public DateTime SearchTime { get; set; } = DateTime.UtcNow;
    }
}''',

    "JuriIQ.Core/Models/UserBookmark.cs": '''using System;

namespace JuriIQ.Core.Models
{
    public class UserBookmark
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid DocumentId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public Document? Document { get; set; }
    }
}''',

    "JuriIQ.Core/Models/LoginAttempt.cs": '''using System;

namespace JuriIQ.Core.Models
{
    public class LoginAttempt
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public DateTime AttemptTime { get; set; } = DateTime.UtcNow;
        public bool Success { get; set; }
        public string? FailureReason { get; set; }
    }
}''',

    # DTOs
    "JuriIQ.Core/DTOs/AuthDTOs.cs": '''using System;

namespace JuriIQ.Core.DTOs
{
    public class LoginRequestDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string? DeviceName { get; set; }
        public string? DeviceType { get; set; }
    }

    public class LoginResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public UserDTO User { get; set; } = null!;
    }

    public class RegisterRequestDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class UserDTO
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string SubscriptionType { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }
}''',

    "JuriIQ.Core/DTOs/DocumentDTOs.cs": '''using System;
using System.Collections.Generic;

namespace JuriIQ.Core.DTOs
{
    public class DocumentDTO
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Summary { get; set; }
        public string? CourtName { get; set; }
        public string? CaseNumber { get; set; }
        public DateTime? DecisionDate { get; set; }
        public string? DocumentType { get; set; }
        public List<string> Keywords { get; set; } = new();
        public DocumentStatisticsDTO? Statistics { get; set; }
    }

    public class DocumentSearchResultDTO
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Summary { get; set; }
        public string? CourtName { get; set; }
        public decimal RelevanceScore { get; set; }
        public List<string> MatchedKeywords { get; set; } = new();
    }

    public class DocumentStatisticsDTO
    {
        public int WordCount { get; set; }
        public int SentenceCount { get; set; }
        public int ParagraphCount { get; set; }
        public int PageCount { get; set; }
    }

    public class SearchRequestDTO
    {
        public string Query { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? DocumentType { get; set; }
        public string? CourtName { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class SearchResponseDTO
    {
        public List<DocumentSearchResultDTO> Results { get; set; } = new();
        public int TotalResults { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}''',

    # Interfaces
    "JuriIQ.Core/Interfaces/IDocumentRepository.cs": '''using System;
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
}''',

    "JuriIQ.Core/Interfaces/IUserRepository.cs": '''using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JuriIQ.Core.Models;

namespace JuriIQ.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<Guid> CreateAsync(User user);
        Task UpdateAsync(User user);
        Task<List<UserDevice>> GetUserDevicesAsync(Guid userId);
        Task AddDeviceAsync(UserDevice device);
        Task<int> GetActiveDeviceCountAsync(Guid userId);
    }
}''',

    "JuriIQ.Core/Interfaces/IAuthService.cs": '''using System.Threading.Tasks;
using JuriIQ.Core.DTOs;

namespace JuriIQ.Core.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request, string ipAddress);
        Task<UserDTO> RegisterAsync(RegisterRequestDTO request);
        string GenerateToken(UserDTO user);
    }
}''',
}

def create_files():
    """Create all files"""
    for file_path, content in FILES.items():
        full_path = os.path.join(BASE_DIR, file_path)
        os.makedirs(os.path.dirname(full_path), exist_ok=True)
        with open(full_path, 'w') as f:
            f.write(content)
        print(f"Created: {file_path}")

if __name__ == "__main__":
    create_files()
    print("\\nAll backend core files created successfully!")
