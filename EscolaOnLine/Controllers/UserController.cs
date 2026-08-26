using EscolaOnLine.Dtos;
using EscolaOnLine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EscolaOnLine.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registrar novo usuário
        /// </summary>
        [HttpPost("cadastrar")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cadastrar([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _userService.CadastrarAsync(dto);
            return result.ToActionResult();
        }

        /// <summary>
        /// Login
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _userService.LogarAsync(dto);
            return result.ToActionResult();
        }           

        /// <summary>
        /// Refresh Token
        /// </summary>
        [HttpPost("token/refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            // Validação extra (caso o DTO não tenha [Required])
            if (string.IsNullOrWhiteSpace(dto.Token))
                return BadRequest(Problem(detail: "Token é obrigatório.", title: "Bad Request"));

            var result = await _userService.AtualizarTokenAsync(dto);
            return result.ToActionResult();
        }

    }
}
