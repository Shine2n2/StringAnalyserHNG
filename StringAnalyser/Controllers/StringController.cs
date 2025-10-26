using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StringAnalyser.DTOs;
using StringAnalyser.Interfaces;

namespace StringAnalyser.Controllers
{
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
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { error = "Query is required.", interpreted_query = new { original = query } });

            var (filters, interpreted) = await _parser.ParseAsync(query, ct);

            if (filters == null)
            {
                var found = await _service.GetByValueAsync(query, ct);
                if (found != null)
                {
                    var responseSingle = new
                    {
                        data = new[]
                        {
                            new
                            {
                                id = found.Id,
                                value = found.Value,
                                properties = found.Properties,
                                created_at = found.CreatedAt
                            }
                        },
                        count = 1,
                        interpreted_query = new { original = query, note = "treated as exact-value lookup" }
                    };
                    return Ok(responseSingle);
                }

                
                return BadRequest(new { error = "Unable to parse natural language query", interpreted_query = interpreted });
            }

           
            if (filters.MinLength.HasValue && filters.MaxLength.HasValue && filters.MinLength.Value > filters.MaxLength.Value)
            {
                return UnprocessableEntity(new
                {
                    error = "Parsed filters are conflicting (min_length > max_length).",
                    interpreted_query = interpreted
                });
            }

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
