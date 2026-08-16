using EscolaOnLine.Models;

namespace EscolaOnLine.Dtos
{
    public class StudentReadDto
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; } 
        public DateTime DataCadastro { get; set; }
        //public bool IsDeleted { get; set; } = false;
        public string UserId { get; set; } = string.Empty;
        public List<Enrollment> Enrollments { get; set; } = new();
    }
}
