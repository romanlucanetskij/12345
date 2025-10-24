using MvcXmlConfigApp.Models;

namespace MvcXmlConfigApp.Services;

public interface IStudentRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Student>> GetStudentsAsync(string? group, double? minGpa, CancellationToken cancellationToken = default);
    Task<Student?> GetStudentByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Student> CreateStudentAsync(Student student, CancellationToken cancellationToken = default);
    Task<bool> UpdateStudentAsync(Student student, CancellationToken cancellationToken = default);
    Task<bool> DeleteStudentAsync(int id, CancellationToken cancellationToken = default);
}
