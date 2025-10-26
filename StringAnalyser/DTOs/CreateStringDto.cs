using System.ComponentModel.DataAnnotations;

namespace StringAnalyser.DTOs
{
    public class CreateStringDto
    {
        [Required]
        public string? Value { get; set; }
    }
}
