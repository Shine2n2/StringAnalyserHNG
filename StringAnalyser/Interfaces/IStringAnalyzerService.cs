using StringAnalyser.Models;

namespace StringAnalyser.Interfaces
{
    public interface IStringAnalyzerService
    {
        Task<(AnalyzedString result, int statusCode)> CreateAsync(object valueObj, CancellationToken ct = default);
        Task<AnalyzedString?> GetByValueAsync(string value, CancellationToken ct = default);
        Task<List<AnalyzedString>> GetAllAsync(StringFilter? filter = null, CancellationToken ct = default);
        Task<bool> DeleteByValueAsync(string value, CancellationToken ct = default);
    }

    public class StringFilter
    {
        public bool? IsPalindrome { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public int? WordCount { get; set; }
        public string? ContainsCharacter { get; set; }
    }
}
