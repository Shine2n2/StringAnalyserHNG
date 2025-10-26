using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StringAnalyser.DTOs;
using StringAnalyser.Interfaces;

namespace StringAnalyser.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    //public class StringController : ControllerBase
    //{
    //}



    [ApiController]
    [Route("strings")]
    public class StringsController : ControllerBase
    {
        private readonly IStringAnalyzerService _service;
        private readonly INaturalLanguageParser _parser;

        public StringsController(IStringAnalyzerService service, INaturalLanguageParser parser)
        {
            _service = service;
            _parser = parser;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStringDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Value?.ToString()))
                return BadRequest(new { error = "'value' must be a string." });

            var (result, statusCode) = await _service.CreateAsync(dto.Value, ct);

            if (result == null)
                return StatusCode(statusCode, new { error = "Failed to create string." });

            var response = new StringResponseDto
            {
                Id = result.Id,
                Value = result.Value,
                Properties = result.Properties,
                CreatedAt = result.CreatedAt
            };

            return StatusCode(statusCode, response);
        }

        [HttpGet("{*string_value}")]
        public async Task<IActionResult> GetByValue([FromRoute] string string_value, CancellationToken ct)
        {
            if (string_value == null) return BadRequest();
            var found = await _service.GetByValueAsync(string_value, ct);
            if (found == null) return NotFound();
            var response = new StringResponseDto
            {
                Id = found.Id,
                Value = found.Value,
                Properties = found.Properties,
                CreatedAt = found.CreatedAt
            };
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? is_palindrome, [FromQuery] int? min_length, [FromQuery] int? max_length, [FromQuery] int? word_count, [FromQuery] string? contains_character, CancellationToken ct = default)
        {
            // validate query types are already bound by model binding. We just compose filter
            var filter = new StringFilter
            {
                IsPalindrome = is_palindrome,
                MinLength = min_length,
                MaxLength = max_length,
                WordCount = word_count,
                ContainsCharacter = contains_character
            };

            var data = await _service.GetAllAsync(filter, ct);
            var response = new
            {
                data = data.Select(d => new
                {
                    id = d.Id,
                    value = d.Value,
                    properties = d.Properties,
                    created_at = d.CreatedAt
                }),
                count = data.Count,
                filters_applied = new
                {
                    is_palindrome,
                    min_length,
                    max_length,
                    word_count,
                    contains_character
                }
            };

            return Ok(response);
        }

        [HttpGet("filter-by-natural-language")]
        public async Task<IActionResult> FilterByNaturalLanguage([FromQuery] string query, CancellationToken ct)
        {
            var (filters, interpreted) = await _parser.ParseAsync(query, ct);
            if (filters == null)
            {
                return BadRequest(new { error = "Unable to parse natural language query", interpreted_query = interpreted });
            }

            // additional validation for conflicting filters could happen here
            var data = await _service.GetAllAsync(filters, ct);
            var response = new
            {
                data = data.Select(d => new
                {
                    id = d.Id,
                    value = d.Value,
                    properties = d.Properties,
                    created_at = d.CreatedAt
                }),
                count = data.Count,
                interpreted_query = interpreted
            };

            return Ok(response);
        }

        [HttpDelete("{*string_value}")]
        public async Task<IActionResult> Delete([FromRoute] string string_value, CancellationToken ct)
        {
            if (string_value == null) return BadRequest();
            var success = await _service.DeleteByValueAsync(string_value, ct);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
