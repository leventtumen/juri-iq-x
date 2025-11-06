using System.ComponentModel.DataAnnotations;

namespace JurisIQ.Backend.Models.DTOs
{
    // Authentication DTOs
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string DeviceId { get; set; } = string.Empty;

        public string DeviceName { get; set; } = string.Empty;

        public string DeviceType { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public SubscriptionType SubscriptionType { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<UserDeviceDto> Devices { get; set; } = new();
    }

    public class UserDeviceDto
    {
        public int Id { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public DateTime FirstLoginAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
    }

    // Document DTOs
    public class DocumentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string FileType { get; set; } = string.Empty;
        public string? CourtName { get; set; }
        public string? CaseNumber { get; set; }
        public DateTime? DecisionDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Summary { get; set; }
        public string? Keywords { get; set; }
        public bool IsBookmarked { get; set; }
        public int ViewCount { get; set; }
    }

    public class DocumentDetailDto : DocumentDto
    {
        public string FullText { get; set; } = string.Empty;
        public List<DocumentKeywordDto> DocumentKeywords { get; set; } = new();
        public List<DocumentDto> RelatedDocuments { get; set; } = new();
    }

    public class DocumentKeywordDto
    {
        public string Keyword { get; set; } = string.Empty;
        public double RelevanceScore { get; set; }
        public int Frequency { get; set; }
    }

    public class BookmarkRequest
    {
        public string? Notes { get; set; }
    }

    public class BookmarkDto
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Search DTOs
    public class SearchRequest
    {
        [Required]
        public string Query { get; set; } = string.Empty;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public string? DocumentType { get; set; }

        public string? CourtName { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }
    }

    public class SearchResultDto
    {
        public List<DocumentDto> Documents { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class SearchHistoryDto
    {
        public int Id { get; set; }
        public string Query { get; set; } = string.Empty;
        public string? Filter { get; set; }
        public int ResultCount { get; set; }
        public DateTime SearchedAt { get; set; }
    }

    // Admin DTOs
    public class AdminUserListDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public SubscriptionType SubscriptionType { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<UserDeviceDto> Devices { get; set; } = new();
        public int DeviceCount => Devices.Count(d => d.IsActive);
    }

    // API Response wrapper
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ApiResponse<T> SuccessResult(T data, string message = "Operation successful")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}