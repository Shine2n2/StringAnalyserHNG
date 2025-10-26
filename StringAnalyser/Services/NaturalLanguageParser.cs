using StringAnalyser.Interfaces;
using System.Text.RegularExpressions;

namespace StringAnalyser.Services
{

    public class NaturalLanguageParser : INaturalLanguageParser
    {
      
        public Task<(StringFilter? filters, object interpretedQuery)> ParseAsync(string query, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(query)) return Task.FromResult<(StringFilter?, object)>((null, new { original = query }));

            var q = System.Net.WebUtility.UrlDecode(query).ToLowerInvariant();

            var filter = new StringFilter();

         
            if (Regex.IsMatch(q, @"\bsingle word\b") && q.Contains("palindrom"))
            {
                filter.WordCount = 1;
                filter.IsPalindrome = true;
                return Task.FromResult<(StringFilter?, object)>((
                    filter,
                    new { original = query, parsed_filters = new { word_count = 1, is_palindrome = true } }
                ));
            }

            
            var mLonger = Regex.Match(q, @"longer than (\d+)");
            if (mLonger.Success)
            {
                if (int.TryParse(mLonger.Groups[1].Value, out var n))
                {
                    filter.MinLength = n + 1;
                    return Task.FromResult<(StringFilter?, object)>((
                        filter,
                        new { original = query, parsed_filters = new { min_length = filter.MinLength } }
                    ));
                }
            }

    
            var mContainChar = Regex.Match(q, @"containing the letter (\w)");
            if (mContainChar.Success)
            {
                filter.ContainsCharacter = mContainChar.Groups[1].Value;
                return Task.FromResult<(StringFilter?, object)>((
                    filter,
                    new { original = query, parsed_filters = new { contains_character = filter.ContainsCharacter } }
                ));
            }

            
            if (q.Contains("palindrom") && q.Contains("first vowel"))
            {
                filter.IsPalindrome = true;
                filter.ContainsCharacter = "a"; 
                return Task.FromResult<(StringFilter?, object)>((
                    filter,
                    new { original = query, parsed_filters = new { is_palindrome = true, contains_character = "a" } }
                ));
            }

           
            return Task.FromResult<(StringFilter?, object)>((null, new { original = query }));
        }
    }
}
