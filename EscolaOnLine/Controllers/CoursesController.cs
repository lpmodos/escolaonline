using EscolaOnLine.Dtos;
using EscolaOnLine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EscolaOnLine.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly CoursesService _coursesService;

        public CoursesController(CoursesService couseService) 
        {
            _coursesService = couseService;
        }

        /// <summary>
        /// Cadastro de curso. Requer credencial Admin/Instructor
        /// </summary>
        /// <response code="201">Curso criado. Header Location aponta para GET /Courses/{id}.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="401">Não autenticado.</response>
        /// <response code="403">Sem permissão.</response>
        /// <response code="409">Conflito ao persistir.</response>
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        [ProducesResponseType(typeof(CreatedIdDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Cadastrar([FromBody] CourseCreateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _coursesService.CadastrarAsync(dto);

            if (!result.Success)
                return result.ToActionResult();

            return CreatedAtAction(
                nameof(BuscarPorId),
                new { id = result.Dados },
                new CreatedIdDto { Id = result.Dados });
        }

        /// <summary>
        /// Listar de cursos / filtrado por categoria com paginação
        /// </summary>
        /// <param name="pagina">Página (padrão 1). Tamanho fixo: 20 itens.</param>
        /// <param name="ordenarPor">titulo ou data. Padrão: data.</param>
        /// <param name="direcao">asc ou desc. Padrão: desc.</param>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<CourseReadSimplificadoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BuscarTodos(
            [FromQuery] string? categoria,
            [FromQuery] string? titulo,
            [FromQuery] string? ordenarPor,
            [FromQuery] string? direcao,
            [FromQuery] int? pagina = 1)
        {
            var result = await _coursesService.BuscarTodosAsync(categoria, titulo, ordenarPor, direcao, pagina);
            return result.ToActionResult();
        }

        /// <summary>
        /// Listar detalhes do curso
        /// </summary>
        /// <param name="id">Id de identificação do Curso.</param>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CourseReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BuscarPorId(int id)
        {
            var result = await _coursesService.BuscarPorIdAsync(id);
            return result.ToActionResult();
        }

        /// <summary>
        /// Atualizar dados do curso. Requer credencial Admin/Instructor
        /// </summary>
        /// <param name="id">Id de identificação do Curso.</param>
        /// <response code="204">Curso atualizado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="401">Não autenticado.</response>
        /// <response code="403">Sem permissão.</response>
        /// <response code="404">Não encontrado.</response>
        /// <response code="409">Conflito ao persistir.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult>Atualizar([FromBody] CourseUpdateDto dto, int id)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _coursesService.AtualizarAsync(dto, id);
            return result.ToActionResult();
        }

        /// <summary>
        /// Apagar curso. Requer credencial Admin
        /// </summary>
        /// <param name="id">Id de identificação do Curso.</param>
        /// <response code="204">Curso apagado com sucesso.</response>
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
        public async Task<IActionResult> Apagar(int id)
        {
            var result = await _coursesService.ApagarAsync(id);
            return result.ToActionResult();
        }
    }   
}
