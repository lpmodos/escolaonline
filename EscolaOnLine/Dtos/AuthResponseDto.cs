namespace EscolaOnLine.Dtos
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string RefreshToken { get; set; } = "/user/token/refresh";
    }
}
