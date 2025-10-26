using StringAnalyser.Models;

namespace StringAnalyser.DTOs
{
    public class StringResponseDto
    {
        public string Id { get; set; } = null!;
        public string Value { get; set; } = null!;
        public StringProperties Properties { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
