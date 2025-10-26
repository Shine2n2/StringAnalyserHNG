# StringAnalyser


Overview
--------
RESTful API that analyzes strings and persists computed properties to a SQLite database.
Computed properties:
- length — number of characters
- is_palindrome — case-insensitive palindrome flag (current implementation preserves punctuation/whitespace)
- unique_characters — count of distinct characters
- word_count — words separated by whitespace
- sha256_hash — SHA‑256 hex (lowercase) of the input (used as entity id)
- character_frequency_map — mapping character -> occurrence count
- created_at — UTC timestamp

Tech
----
- .NET 8, ASP.NET Core Web API
- Entity Framework Core (SQLite)
- Swagger UI available (configurable)

Configuration
-------------
File: `appsettings.json`
- ConnectionStrings:Sqlite — path to SQLite DB (default: `Data Source=stringanalyzer.db`)
- SwaggerSettings:EnableSwagger — true/false

Quickstart (local)
------------------
1. From the project folder:
   - Restore and run:
     dotnet restore
     dotnet run
   - Or open solution in Visual Studio and use __Build > Build Solution__ then __Debug > Start Debugging__.

2. App URLs are printed to console (typically http://localhost:5000 and https://localhost:5001).
3. If Swagger is enabled, visit {HTTPS-URL}/swagger/index.html.

Database
--------
- The app calls `db.Database.EnsureCreated()` at startup and will create the SQLite file automatically.
- To use EF migrations:
  - Install: `dotnet tool install --global dotnet-ef`
  - Add migration: `dotnet ef migrations add <Name>`
  - Apply: `dotnet ef database update`

API — Endpoints
---------------

1) Create / Analyze
- POST /strings
- Headers: Content-Type: application/json
- Body: { "value": "string to analyze" }
- Success: 201 Created (returns stored AnalyzedString)
- Errors:
  - 400 Bad Request — missing/empty value
  - 422 Unprocessable Entity — value present but not a JSON string
  - 409 Conflict — string already exists
- Example:
  curl -X POST "http://localhost:5000/strings" -H "Content-Type: application/json" -d '{"value":"racecar"}'

2) Retrieve by value
- GET /strings/{string_value}
- Server computes SHA‑256 of path value and looks up by id.
- Responses: 200 OK / 404 Not Found

3) List with filters
- GET /strings
- Query params: is_palindrome (bool), min_length (int), max_length (int), word_count (int), contains_character (string)
- Success: 200 OK with { data, count, filters_applied }

4) Natural-language filtering
- GET /strings/filter-by-natural-language?query={text}

5) Delete
- DELETE /strings/{string_value}
- Computes SHA‑256 of the path value and deletes the record.
- Success: 204 No Content; 404 if not found.

- Repo remote: https://github.com/Shine2n2/StringAnalyserHNG

