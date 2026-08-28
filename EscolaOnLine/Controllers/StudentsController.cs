using EscolaOnLine.Dtos;
using EscolaOnLine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EscolaOnLine.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly StudentsService _studentsService;
        private readonly EnrollmentsService _enrollementsService;
        public StudentsController(StudentsService studentsService, EnrollmentsService enrollementsService)
        {
            _studentsService = studentsService;
            _enrollementsService = enrollementsService;
        }

        /// <summary>
        /// Cadastro de Estudante
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Cadastrar([FromBody] StudentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _studentsService.CadastrarAsync(dto);
            return result.ToActionResult();
        }

        /// <summary>
        /// Listar todos Estudantes
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>IActionResult</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BuscarTodos([FromQuery] string? nome, [FromQuery] string? ordernarPor, [FromQuery] string? direcao, [FromQuery] int? pagina)
        {
            var result = await _studentsService.BuscarTodosAsync(nome, ordernarPor, direcao, pagina);
            return result.ToActionResult();
        }

        /// <summary>
        /// Listar detalhes do estudante
        /// </summary>
        /// <param name="id"></param>
        /// <returns>IActionResult</returns>
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> BuscarPorId(int id)
        {
            var result = await _studentsService.BuscarPorIdAsync(id);

            if (!result.Success)
                return result.ToActionResult();

            // Verifica se é Admin
            var isAdmin = User.IsInRole("Admin");

            // Verifica se é o próprio usuário
            var userIdLogado = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isProprioUsuario = result.Dados!.UserId == userIdLogado;

            if (!isAdmin && !isProprioUsuario)
                return Forbid(); // 403

            return result.ToActionResult();
        }

        /// <summary>
        /// Obter dados do pefil do estudante logado
        /// </summary>
        /// <param name="id"></param>
        /// <returns>IActionResult</returns>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> BuscarEstudanteAtual()
        {
            var userIdLogado = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdLogado))
                return Unauthorized();

            var result = await _studentsService.BuscarPorUserIdAsync(userIdLogado);
            return result.ToActionResult();
        }

        /// <summary>
        /// Atualizar dados do estudante
        /// </summary>
        /// <param name="id"></param>
        /// <returns>IActionResult</returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Atualizar([FromBody] StudentUpdateDto dto, int id)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var estudante = await _studentsService.BuscarPorIdAsync(id);

            if (!estudante.Success)
                return estudante.ToActionResult();

            // Verifica se é Admin
            var isAdmin = User.IsInRole("Admin");

            // Verifica se é o próprio usuário
            var userIdLogado = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isProprioUsuario = estudante.Dados!.UserId == userIdLogado;

            if (!isAdmin && !isProprioUsuario)
                return Forbid(); // 403

            var result = await _studentsService.AtualizarAsync(dto, id);
            return result.ToActionResult();
        }

        /// <summary>
        /// Apagar estudante
        /// </summary>
        /// <param name="id"></param>
        /// <returns>IActionResult</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Apagar(int id, [FromQuery] bool? apagarDefinitivo = false)
        {
            var result = await _studentsService.ApagarAsync(id, apagarDefinitivo ?? false);
            return result.ToActionResult();
        }

        /// <summary>
        /// Lista cursos de um estudante
        /// </summary>
        /// <param name="id"></param>
        /// <returns>IActionResult</returns>
        [HttpGet("{id}/enrollments")]
        [Authorize]
        public async Task<IActionResult> BuscarCursos(int id)
        {
            var estudante = await _studentsService.BuscarPorIdAsync(id);

            if (!estudante.Success)
                return estudante.ToActionResult();

            // Verifica se é Admin
            var isAdmin = User.IsInRole("Admin");

            // Verifica se é o próprio usuário
            var userIdLogado = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isProprioUsuario = estudante.Dados!.UserId == userIdLogado;

            if (!isAdmin && !isProprioUsuario)
                return Forbid(); // 403

            var result = await _enrollementsService.BuscarCursosDoEstudanteAsync(id);
            return result.ToActionResult();
        }

    }
}
