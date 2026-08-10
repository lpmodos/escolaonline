using System.ComponentModel.DataAnnotations;

namespace EscolaOnLine.Dtos
{
    public class CourseReadSimplificadoDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Categoria { get; set; }
    }
}
