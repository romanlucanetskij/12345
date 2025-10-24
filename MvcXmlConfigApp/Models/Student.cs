using System.ComponentModel.DataAnnotations;

namespace MvcXmlConfigApp.Models;

public class Student
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Group { get; set; } = string.Empty;

    [Range(0, 5)]
    public double Gpa { get; set; }
}
