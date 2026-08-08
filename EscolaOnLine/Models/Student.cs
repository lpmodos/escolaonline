using System.ComponentModel.DataAnnotations;

namespace EscolaOnLine.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100, ErrorMessage = "Nome deve ter tamanho maximo de 100 caractere")]
        public string NomeCompleto { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;
        [Required]
        public string UserId { get; set; } = string.Empty;
        public List<Enrollment> Enrollments { get; set; } = new();
    }
}
