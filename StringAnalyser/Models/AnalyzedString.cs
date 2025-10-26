using System.ComponentModel.DataAnnotations;

namespace StringAnalyser.Models
{
    public class AnalyzedString
    {
        
        [Key]
        [MaxLength(64)]
        public string Id { get; set; } = null!;

        [Required]
        public string Value { get; set; } = null!;

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public StringProperties Properties { get; set; } = null!;

      
        public bool IsPalindrome { get; set; }
        public int Length { get; set; }
        public int WordCount { get; set; }
        public int UniqueCharacters { get; set; }
    }
}
