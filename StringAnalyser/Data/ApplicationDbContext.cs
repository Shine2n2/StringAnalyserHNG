using Microsoft.EntityFrameworkCore;
using StringAnalyser.Models;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text.Json;

namespace StringAnalyser.Data
{

    public class ApplicationDbContext : DbContext
    {
        public DbSet<AnalyzedString> Stringss { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            modelBuilder.Entity<AnalyzedString>()
                .Property(p => p.Properties)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<StringProperties>(v, (JsonSerializerOptions?)null)!
                );

            base.OnModelCreating(modelBuilder);
        }
    }
}
