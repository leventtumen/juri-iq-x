namespace JuriIQ.Core.DTOs;

public class SearchResponse
{
    public int TotalResults { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public double SearchTimeSeconds { get; set; }
    public List<SearchResultDto> Results { get; set; } = new();
}
