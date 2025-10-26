using System.ComponentModel.DataAnnotations;

namespace StringAnalyser.DTOs
{
    public class CreateStringDto
    {
        [Required]
        public object? Value { get; set; }

        
    }
}
