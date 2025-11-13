namespace JuriIQ.Core.DTOs;

public class SearchHistoryDto
{
    public int Id { get; set; }
    public string Query { get; set; } = string.Empty;
    public int ResultCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
