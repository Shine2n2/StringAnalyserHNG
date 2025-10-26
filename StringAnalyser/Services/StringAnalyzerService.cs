using Microsoft.EntityFrameworkCore;
using StringAnalyser.Data;
using StringAnalyser.Interfaces;
using StringAnalyser.Models;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace StringAnalyser.Services
{

    public class StringAnalyzerService : IStringAnalyzerService
    {
        private readonly ApplicationDbContext _db;

        public StringAnalyzerService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<(AnalyzedString result, int statusCode)> CreateAsync(object valueObj, CancellationToken ct = default)
        {
            // Validate type
            if (valueObj == null)
                return (null!, 400); // missing

            if (valueObj is not string value)
            {
                // if it's not a string in the JSON payload, return 422
                return (null!, 422);
            }

            // compute SHA-256 hash (hex lowercase)
            var hash = ComputeSha256Hex(value);

            // check conflict by id (hash) or by exact value (to be safe)
            var existing = await _db.Strings.FindAsync(new object[] { hash }, ct);
            if (existing != null)
            {
                return (existing, 409);
            }

            // compute properties
            var properties = Analyze(value, hash);

            var entity = new AnalyzedString
            {
                Id = hash,
                Value = value,
                CreatedAt = DateTime.UtcNow,
                Properties = properties,
                IsPalindrome = properties.IsPalindrome,
                Length = properties.Length,
                WordCount = properties.WordCount,
                UniqueCharacters = properties.UniqueCharacters
            };

            _db.Strings.Add(entity);
            await _db.SaveChangesAsync(ct);

            return (entity, 201);
        }

        public async Task<AnalyzedString?> GetByValueAsync(string value, CancellationToken ct = default)
        {
            var id = ComputeSha256Hex(value);
            return await _db.Strings.FindAsync(new object[] { id }, ct);
        }

        public async Task<List<AnalyzedString>> GetAllAsync(StringFilter? filter = null, CancellationToken ct = default)
        {
            IQueryable<AnalyzedString> q = _db.Strings.AsQueryable();

            if (filter != null)
            {
                if (filter.IsPalindrome is not null)
                    q = q.Where(s => s.IsPalindrome == filter.IsPalindrome.Value);
                if (filter.MinLength is not null)
                    q = q.Where(s => s.Length >= filter.MinLength.Value);
                if (filter.MaxLength is not null)
                    q = q.Where(s => s.Length <= filter.MaxLength.Value);
                if (filter.WordCount is not null)
                    q = q.Where(s => s.WordCount == filter.WordCount.Value);
                if (!string.IsNullOrEmpty(filter.ContainsCharacter))
                {
                    var ch = filter.ContainsCharacter;
                    q = q.Where(s => s.Value.Contains(ch));
                }
            }

            return await q.OrderByDescending(s => s.CreatedAt).ToListAsync(ct);
        }

        public async Task<bool> DeleteByValueAsync(string value, CancellationToken ct = default)
        {
            var id = ComputeSha256Hex(value);
            var found = await _db.Strings.FindAsync(new object[] { id }, ct);
            if (found == null) return false;
            _db.Strings.Remove(found);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        #region helpers

        private static string ComputeSha256Hex(string input)
        {
            using var sha = SHA256.Create();
            var b = Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(b);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var bt in hash) sb.Append(bt.ToString("x2"));
            return sb.ToString();
        }

        private static StringProperties Analyze(string value, string hash)
        {
            var props = new StringProperties
            {
                Sha256Hash = hash
            };

            props.Length = value.Length;
            props.IsPalindrome = IsPalindrome(value);
            props.WordCount = CountWords(value);
            props.CharacterFrequencyMap = ComputeCharacterFrequency(value);
            props.UniqueCharacters = props.CharacterFrequencyMap.Count;

            return props;
        }

        private static bool IsPalindrome(string s)
        {
            // spec: case-insensitive comparison. We keep all characters (including spaces/punctuation).
            if (s == null) return false;
            var lower = s.ToLowerInvariant();
            int i = 0, j = lower.Length - 1;
            while (i < j)
            {
                if (lower[i] != lower[j]) return false;
                i++; j--;
            }
            return true;
        }

        private static int CountWords(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            // split by whitespace
            var parts = Regex.Split(s.Trim(), @"\s+");
            return parts.Length;
        }

        private static Dictionary<string, int> ComputeCharacterFrequency(string s)
        {
            var dict = new Dictionary<string, int>();
            foreach (var ch in s)
            {
                var key = ch.ToString(); // use exact character as key
                if (dict.ContainsKey(key)) dict[key]++;
                else dict[key] = 1;
            }
            return dict;
        }

        #endregion
    }
}
