using System.ComponentModel.DataAnnotations;

namespace EscolaOnLine.Dtos
{
    public class CourseCreateDto
    {
        [Required]
        [MaxLength(100, ErrorMessage = "Título deve ter no máximo 100 caracteres")]
        public string Titulo { get; set; } = string.Empty;
        [Required]
        public string Descricao { get; set; } = string.Empty;
        [Required]
        [MaxLength(30, ErrorMessage = "Categoria deve ter no máximo 30 caracteres")]
        public string Categoria { get; set; } = string.Empty;
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Carga horária deve ser maior que zero")]
        public int CargaHoraria { get; set; } //em segundos
    }
}
