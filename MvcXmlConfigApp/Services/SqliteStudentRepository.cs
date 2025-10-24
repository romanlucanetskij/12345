using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;
using MvcXmlConfigApp.Models;

namespace MvcXmlConfigApp.Services;

public class SqliteStudentRepository : IStudentRepository
{
    private readonly string _connectionString;

    public SqliteStudentRepository(IHostEnvironment environment)
    {
        var dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDirectory);
        var databasePath = Path.Combine(dataDirectory, "students.db");
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Students (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                GroupName TEXT NOT NULL,
                Gpa REAL NOT NULL
            );";

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IEnumerable<Student>> GetStudentsAsync(string? group, double? minGpa, CancellationToken cancellationToken = default)
    {
        var students = new List<Student>();

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
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

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            students.Add(new Student
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Group = reader.GetString(2),
                Gpa = reader.GetDouble(3)
            });
        }

        return students;
    }

    public async Task<Student?> GetStudentByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, GroupName, Gpa FROM Students WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return new Student
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Group = reader.GetString(2),
                Gpa = reader.GetDouble(3)
            };
        }

        return null;
    }

    public async Task<Student> CreateStudentAsync(Student student, CancellationToken cancellationToken = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Students (Name, GroupName, Gpa)
            VALUES ($name, $group, $gpa);
            SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("$name", student.Name);
        command.Parameters.AddWithValue("$group", student.Group);
        command.Parameters.AddWithValue("$gpa", student.Gpa);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        if (result is long id)
        {
            student.Id = (int)id;
        }

        return student;
    }

    public async Task<bool> UpdateStudentAsync(Student student, CancellationToken cancellationToken = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
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

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        return affected > 0;
    }

    public async Task<bool> DeleteStudentAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Students WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        return affected > 0;
    }

    private SqliteConnection CreateConnection() => new(_connectionString);
}
