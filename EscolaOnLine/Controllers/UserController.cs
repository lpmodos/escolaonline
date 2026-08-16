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
                return BadRequest(Problem(detail: "Dados inválidos."));

            var result = await _userService.CadastrarAsync(dto);

            if (!result.Success)
                return StatusCode(result.StatusCode, Problem(detail: result.Error));
            return StatusCode(201, new { message = "Usuário registrado com sucesso" });
        }

        /// <summary>
        /// Login
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(Problem(detail: "Dados inválidos."));

            var result = await _userService.LogarAsync(dto);

            if (!result.Success)
                return StatusCode(result.StatusCode, Problem(detail: result.Error));

            return Ok(result.Dados);
        }

        /// <summary>
        /// Refresh Token
        /// </summary>
        [HttpPost("token/refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token))
                return BadRequest(Problem(detail: "Token é obrigatório."));

            var result = await _userService.AtualizarTokenAsync(dto);

            if (!result.Success)
                return StatusCode(result.StatusCode, Problem(detail: result.Error));

            return Ok(result.Dados);
        }


    }
}
