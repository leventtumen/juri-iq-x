using JuriIQ.Core.Entities;
using JuriIQ.Core.DTOs;

namespace JuriIQ.Core.Interfaces;

public interface ISearchHistoryRepository
{
    Task<List<SearchHistoryDto>> GetUserRecentSearchesAsync(int userId, int limit = 10);
    Task<int> CreateAsync(SearchHistory searchHistory);
}
