using System.ComponentModel.DataAnnotations;

namespace EscolaOnLine.Dtos
{
    public class CourseUpdateDto 
    {
        [MinLength(3, ErrorMessage = "Título deve ter no mínimo 3 caracteres.")]
        [MaxLength(100, ErrorMessage = "Título deve ter no máximo 100 caracteres.")]
        public string? Titulo { get; set; } 
        public string? Descricao { get; set; }
        [MaxLength(30, ErrorMessage = "Categoria deve ter no máximo 30 caracteres.")]
        public string? Categoria { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Carga horária deve ser maior que zero.")]
        public int? CargaHoraria { get; set; }

    }
}
