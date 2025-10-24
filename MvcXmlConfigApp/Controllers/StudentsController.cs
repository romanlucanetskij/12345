using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;
using MvcXmlConfigApp.Models;

namespace MvcXmlConfigApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly string _connectionString;
    private static readonly SemaphoreSlim InitializationSemaphore = new(1, 1);
    private static bool _databaseInitialized;

    public StudentsController(IHostEnvironment environment)
    {
        var dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDirectory);
        var databasePath = Path.Combine(dataDirectory, "students.db");
        _connectionString = $"Data Source={databasePath}";
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Student>>> GetStudents([FromQuery] string? group, [FromQuery] double? minGpa)
    {
        await EnsureDatabaseCreatedAsync();

        var students = new List<Student>();

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        var filters = new List<string>();

        if (!string.IsNullOrWhiteSpace(group))
        {
            filters.Add("GroupName = $group");
            command.Parameters.AddWithValue("$group", group.Trim());
        }

        if (minGpa.HasValue)
        {
            filters.Add("Gpa >= $minGpa");
            command.Parameters.AddWithValue("$minGpa", minGpa.Value);
        }

        var whereClause = filters.Count > 0 ? $"WHERE {string.Join(" AND ", filters)}" : string.Empty;
        command.CommandText = $"SELECT Id, Name, GroupName, Gpa FROM Students {whereClause} ORDER BY Id";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            students.Add(new Student
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Group = reader.GetString(2),
                Gpa = reader.GetDouble(3)
            });
        }

        return Ok(students);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Student>> GetStudent(int id)
    {
        await EnsureDatabaseCreatedAsync();

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, GroupName, Gpa FROM Students WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var student = new Student
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Group = reader.GetString(2),
                Gpa = reader.GetDouble(3)
            };

            return Ok(student);
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<Student>> CreateStudent([FromBody] Student student)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        await EnsureDatabaseCreatedAsync();

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Students (Name, GroupName, Gpa)
            VALUES ($name, $group, $gpa);
            SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("$name", student.Name);
        command.Parameters.AddWithValue("$group", student.Group);
        command.Parameters.AddWithValue("$gpa", student.Gpa);

        var result = await command.ExecuteScalarAsync();
        if (result is long id)
        {
            student.Id = (int)id;
        }

        return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateStudent(int id, [FromBody] Student student)
    {
        if (id != student.Id)
        {
            return BadRequest("Student ID in the path does not match the payload.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        await EnsureDatabaseCreatedAsync();

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Students
            SET Name = $name,
                GroupName = $group,
                Gpa = $gpa
            WHERE Id = $id";

        command.Parameters.AddWithValue("$name", student.Name);
        command.Parameters.AddWithValue("$group", student.Group);
        command.Parameters.AddWithValue("$gpa", student.Gpa);
        command.Parameters.AddWithValue("$id", student.Id);

        var affected = await command.ExecuteNonQueryAsync();
        if (affected == 0)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        await EnsureDatabaseCreatedAsync();

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Students WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);

        var affected = await command.ExecuteNonQueryAsync();
        if (affected == 0)
        {
            return NotFound();
        }

        return NoContent();
    }

    private async Task EnsureDatabaseCreatedAsync()
    {
        if (_databaseInitialized)
        {
            return;
        }

        await InitializationSemaphore.WaitAsync();
        try
        {
            if (_databaseInitialized)
            {
                return;
            }

            await using var connection = CreateConnection();
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Students (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    GroupName TEXT NOT NULL,
                    Gpa REAL NOT NULL
                );";

            await command.ExecuteNonQueryAsync();
            _databaseInitialized = true;
        }
        finally
        {
            InitializationSemaphore.Release();
        }
    }

    private SqliteConnection CreateConnection() => new(_connectionString);
}
