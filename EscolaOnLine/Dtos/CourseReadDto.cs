namespace EscolaOnLine.Dtos
{
    public class CourseReadDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public string Categoria { get; set; }
        public int CargaHoraria { get; set; }
        public DateTime DataCriacao { get; set; }
        public bool IsDeleted { get; set; }
    }
}
