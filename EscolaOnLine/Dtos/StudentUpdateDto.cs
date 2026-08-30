using System.ComponentModel.DataAnnotations;

namespace EscolaOnLine.Dtos
{
    public class StudentUpdateDto
    {
        [MaxLength(100, ErrorMessage = "Nome deve ter tamanho maximo de 100 caractere")]
        public string? NomeCompleto { get; set; } 
        public DateTime DataCadastro { get; set; }
    }
}
