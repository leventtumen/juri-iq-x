namespace JuriIQ.Core.Interfaces;

public interface IDocumentProcessor
{
    Task<string> ExtractTextFromFileAsync(string filePath);
    Task<string> GenerateSummaryAsync(string text);
    Task<List<(string keyword, double relevance)>> ExtractKeywordsAsync(string text);
    double CalculateSimilarity(string text1, string text2);
}
