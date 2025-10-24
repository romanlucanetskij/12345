using Microsoft.AspNetCore.Mvc;
using MvcXmlConfigApp.Models;
using MvcXmlConfigApp.Services;

namespace MvcXmlConfigApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentRepository _repository;

    public StudentsController(IStudentRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Student>>> GetStudents([FromQuery] string? group, [FromQuery] double? minGpa)
    {
        var students = await _repository.GetStudentsAsync(group, minGpa);
        return Ok(students);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Student>> GetStudent(int id)
    {
        var student = await _repository.GetStudentByIdAsync(id);
        if (student is null)
        {
            return NotFound();
        }

        return Ok(student);
    }

    [HttpPost]
    public async Task<ActionResult<Student>> CreateStudent([FromBody] Student student)
    {
        var created = await _repository.CreateStudentAsync(student);
        return CreatedAtAction(nameof(GetStudent), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateStudent(int id, [FromBody] Student student)
    {
        if (id != student.Id)
        {
            return BadRequest("Student ID in the path does not match the payload.");
        }

        var updated = await _repository.UpdateStudentAsync(student);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        var deleted = await _repository.DeleteStudentAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
