using EscolaOnLine.Dtos;
using EscolaOnLine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EscolaOnLine.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Tags("Auth")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registrar usuário (Admin, Instructor ou Student). Requer role Admin.
        /// </summary>
        [HttpPost("cadastrar")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Cadastrar([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _userService.CadastrarAsync(dto);
            return result.ToActionResult();
        }

        /// <summary>
        /// Login. Público. Use o token no botão Authorize do Swagger.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _userService.LogarAsync(dto);
            return result.ToActionResult();
        }

        /// <summary>
        /// Renovar JWT a partir de um token expirado.
        /// </summary>
        [HttpPost("token/refresh")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
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
