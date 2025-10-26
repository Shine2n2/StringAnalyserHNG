namespace StringAnalyser.Interfaces
{
    public interface INaturalLanguageParser
    {
        
        Task<(StringFilter? filters, object interpretedQuery)> ParseAsync(string query, CancellationToken ct = default);
    }
}
