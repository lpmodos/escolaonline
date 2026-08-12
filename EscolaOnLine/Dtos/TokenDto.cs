using System.ComponentModel.DataAnnotations;

namespace EscolaOnLine.Dtos
{
    public class TokenDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }
}
