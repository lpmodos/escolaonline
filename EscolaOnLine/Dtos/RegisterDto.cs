using System.ComponentModel.DataAnnotations;

namespace EscolaOnLine.Dtos
{
    public class RegisterDto : StudentCreateDto
    {
        [Required]
        public string Role { get; set; } = "Student"; // Admin, Instructor, Student
    }
}
