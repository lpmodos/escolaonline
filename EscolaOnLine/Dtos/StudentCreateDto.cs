using EscolaOnLine.Models;
using System.ComponentModel.DataAnnotations;

namespace EscolaOnLine.Dtos
{
    public class StudentCreateDto
    {
        [Required(ErrorMessage ="Nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "Nome deve ter tamanho maximo de 100 caractere")]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
