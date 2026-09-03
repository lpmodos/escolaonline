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
        /// Cadastro de Estudante.
        /// </summary>
        /// <response code="201">Estudante criado.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="409">Conflito ao persistir.</response>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Cadastrar([FromBody] StudentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _studentsService.CadastrarAsync(dto);
            return result.ToActionResult();
        }

        /// <summary>
        /// Listar Estudantes / filtrado por categoria com paginação. Requer credencial Admin
        /// </summary>
        /// <param name="pagina">Página (padrão 1). Tamanho fixo: 20 itens.</param>
        /// <param name="ordenarPor">Nome ou id. Padrão: data.</param>
        /// <param name="direcao">asc ou desc. Padrão: desc.</param>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(List<StudentReadSimplificadoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> BuscarTodos([FromQuery] string? nome, [FromQuery] string? ordenarPor, [FromQuery] string? direcao, [FromQuery] int? pagina = 1)
        {
            var result = await _studentsService.BuscarTodosAsync(nome, ordenarPor, direcao, pagina);
            return result.ToActionResult();
        }

        /// <summary>
        /// Listar detalhes do estudante. Admin ou o próprio estudante
        /// </summary>
        /// <param name="id"></param>
        [HttpGet("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(StudentReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(StudentReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> BuscarEstudanteAtual()
        {
            var userIdLogado = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdLogado))
                return Unauthorized();

            var result = await _studentsService.BuscarPorUserIdAsync(userIdLogado);
            return result.ToActionResult();
        }

        /// <summary>
        /// Atualizar dados do estudante. Admin ou o próprio
        /// </summary>
        /// <param name="id">Id de identificação do Estudante.</param>
        /// <response code="204">Estudante atualizado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="401">Não autenticado.</response>
        /// <response code="403">Sem permissão.</response>
        /// <response code="404">Não encontrado.</response>
        /// <response code="409">Conflito ao persistir.</response>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
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
        /// Apagar estudante. Requer credencial Admin
        /// </summary>
        /// <param name="id"></param>
        /// <response code="204">Estudante apagado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="401">Não autenticado.</response>
        /// <response code="403">Sem permissão.</response>
        /// <response code="404">Não encontrado.</response>
        /// <response code="409">Conflito ao persistir.</response>
        /// <response code="422">Recurso não existente ou já desativado.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Apagar(int id, [FromQuery] bool? apagarDefinitivo = false)
        {
            var result = await _studentsService.ApagarAsync(id, apagarDefinitivo ?? false);
            return result.ToActionResult();
        }

        /// <summary>
        /// Lista cursos de um estudante
        /// </summary>
        /// <param name="id"></param>
        [HttpGet("{id}/enrollments")]
        [Authorize]
        [ProducesResponseType(typeof(List<CourseReadSimplificadoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
