using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using JuriIQ.Core.Interfaces;

namespace JuriIQ.AI.Services;

public class DocumentProcessor : IDocumentProcessor
{
    public async Task<string> ExtractTextFromFileAsync(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => await ExtractFromPdfAsync(filePath),
            ".docx" => await ExtractFromDocxAsync(filePath),
            ".doc" => await ExtractFromDocAsync(filePath),
            ".txt" => await ExtractFromTxtAsync(filePath),
            _ => throw new NotSupportedException($"File extension {extension} is not supported.")
        };
    }

    public async Task<string> GenerateSummaryAsync(string text)
    {
        // Simple extractive summarization - take first N sentences
        // In production, you would use a more sophisticated NLP model

        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // Clean text
        text = CleanText(text);

        // Split into sentences
        var sentences = Regex.Split(text, @"(?<=[\.!?])\s+");

        // Take first 5 sentences or up to 500 characters
        var summary = new StringBuilder();
        var totalLength = 0;
        var sentenceCount = 0;

        foreach (var sentence in sentences)
        {
            if (sentenceCount >= 5 || totalLength + sentence.Length > 500)
            {
                break;
            }

            summary.Append(sentence).Append(" ");
            totalLength += sentence.Length;
            sentenceCount++;
        }

        return await Task.FromResult(summary.ToString().Trim());
    }

    public async Task<List<(string keyword, double relevance)>> ExtractKeywordsAsync(string text)
    {
        // Simple TF-IDF-like keyword extraction
        // In production, you would use a more sophisticated NLP model

        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<(string, double)>();
        }

        text = CleanText(text);

        // Tokenize and count words
        var words = text.ToLowerInvariant()
            .Split(new[] { ' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?' },
                StringSplitOptions.RemoveEmptyEntries);

        // Turkish stop words
        var stopWords = new HashSet<string>
        {
            "bir", "ve", "veya", "ile", "bu", "şu", "o", "de", "da", "için", "olan",
            "var", "yok", "mi", "mu", "mı", "mü", "gibi", "kadar", "daha", "en",
            "çok", "az", "çünkü", "ancak", "fakat", "ama", "yahut", "ise", "ne",
            "ki", "iken", "dahi", "bile", "hem", "ya", "ait", "ilk", "son"
        };

        // Count word frequencies
        var wordFrequency = new Dictionary<string, int>();

        foreach (var word in words)
        {
            if (word.Length >= 4 && !stopWords.Contains(word))
            {
                wordFrequency[word] = wordFrequency.GetValueOrDefault(word, 0) + 1;
            }
        }

        // Calculate relevance (normalized frequency)
        var maxFrequency = wordFrequency.Values.DefaultIfEmpty(0).Max();

        var keywords = wordFrequency
            .Select(kvp => (kvp.Key, (double)kvp.Value / maxFrequency))
            .OrderByDescending(x => x.Item2)
            .Take(20)
            .ToList();

        return await Task.FromResult(keywords);
    }

    public double CalculateSimilarity(string text1, string text2)
    {
        // Cosine similarity using word vectors
        // This is a simplified version - in production, use more sophisticated embeddings

        if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2))
        {
            return 0.0;
        }

        text1 = CleanText(text1).ToLowerInvariant();
        text2 = CleanText(text2).ToLowerInvariant();

        var words1 = text1.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var words2 = text2.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // Create word frequency vectors
        var allWords = words1.Union(words2).Distinct().ToList();

        var vector1 = allWords.Select(word => words1.Count(w => w == word)).ToArray();
        var vector2 = allWords.Select(word => words2.Count(w => w == word)).ToArray();

        // Calculate cosine similarity
        var dotProduct = vector1.Zip(vector2, (a, b) => a * b).Sum();
        var magnitude1 = Math.Sqrt(vector1.Sum(x => x * x));
        var magnitude2 = Math.Sqrt(vector2.Sum(x => x * x));

        if (magnitude1 == 0 || magnitude2 == 0)
        {
            return 0.0;
        }

        return dotProduct / (magnitude1 * magnitude2);
    }

    private async Task<string> ExtractFromPdfAsync(string filePath)
    {
        var text = new StringBuilder();

        using (var document = PdfDocument.Open(filePath))
        {
            foreach (var page in document.GetPages())
            {
                text.AppendLine(page.Text);
            }
        }

        return await Task.FromResult(text.ToString());
    }

    private async Task<string> ExtractFromDocxAsync(string filePath)
    {
        var text = new StringBuilder();

        using (var document = WordprocessingDocument.Open(filePath, false))
        {
            var body = document.MainDocumentPart?.Document?.Body;

            if (body != null)
            {
                foreach (var paragraph in body.Descendants<Paragraph>())
                {
                    text.AppendLine(paragraph.InnerText);
                }
            }
        }

        return await Task.FromResult(text.ToString());
    }

    private async Task<string> ExtractFromDocAsync(string filePath)
    {
        // For .doc files (older Word format), we'll need to use a different approach
        // This is a simplified version - you might need to use Aspose.Words or another library
        // For now, we'll throw an exception and recommend converting to .docx

        throw new NotSupportedException(
            "Legacy .doc format is not fully supported. " +
            "Please convert the document to .docx format or use a library like Aspose.Words.");
    }

    private async Task<string> ExtractFromTxtAsync(string filePath)
    {
        return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
    }

    private string CleanText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // Remove excessive whitespace
        text = Regex.Replace(text, @"\s+", " ");

        // Remove special characters but keep Turkish characters
        text = Regex.Replace(text, @"[^\w\s\u00C0-\u017F\u0130\u0131\u011E\u011F\u015E\u015F\.,;:!?-]", "");

        return text.Trim();
    }
}
