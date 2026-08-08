using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EscolaOnLine.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100, ErrorMessage = "Nome deve ter tamanho maximo de 100 caractere")]
        public string NomeCompleto { get; set; }
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        [ForeignKey("Email")]
        public string Email { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; } = false;
        [Required]
        public string UserId { get; set; }
        public List<Enrollment> Enrollments { get; set; } = new();
    }
}
