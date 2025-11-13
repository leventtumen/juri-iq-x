using Dapper;
using JuriIQ.Core.Entities;
using JuriIQ.Core.Interfaces;
using JuriIQ.Core.DTOs;
using JuriIQ.Infrastructure.Data;

namespace JuriIQ.Infrastructure.Repositories;

public class SearchHistoryRepository : ISearchHistoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SearchHistoryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<SearchHistoryDto>> GetUserRecentSearchesAsync(int userId, int limit = 10)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id AS Id, query AS Query, result_count AS ResultCount, created_at AS CreatedAt
            FROM search_history
            WHERE user_id = @UserId
            ORDER BY created_at DESC
            LIMIT @Limit";

        var searches = await connection.QueryAsync<SearchHistoryDto>(sql, new { UserId = userId, Limit = limit });
        return searches.ToList();
    }

    public async Task<int> CreateAsync(SearchHistory searchHistory)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO search_history (user_id, query, result_count, created_at)
            VALUES (@UserId, @Query, @ResultCount, @CreatedAt)
            RETURNING id";

        return await connection.ExecuteScalarAsync<int>(sql, searchHistory);
    }
}
