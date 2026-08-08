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
        /// Cadastro de curso
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>IActionResult</returns>

        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Cadastrar([FromBody] CourseCreateDto dto)
        {
            if (dto == null) return BadRequest(Problem(detail: "Dados não informados."));

            var id = await _coursesService.CadastrarAsync(dto);

            return CreatedAtAction(nameof(BuscarPorId), new { id }, new { id });
        }

        /// <summary>
        /// Listar de cursos / filtrado por categoria com paginação
        /// </summary>
        /// <returns>IActionResult</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> BuscarTodos(
            [FromQuery] string? categoria,
            [FromQuery] string? titulo,
            [FromQuery] string? ordenarPor,
            [FromQuery] string? direcao,
            [FromQuery] int pagina = 1)
        {
            if (pagina < 1)
                return BadRequest(Problem(detail: "A página deve ser maior que zero."));

            var cursos = await _coursesService.BuscarTodosAsync(categoria, titulo, ordenarPor, direcao, pagina);
            return Ok(cursos);
        }

        /// <summary>
        /// Listar detalhes do curso
        /// </summary>
        /// <param name="id"></param>
        /// <returns>IActionResult</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> BuscarPorId(int id)
        {
            if (id < 1)
                return BadRequest(Problem(detail: "Id incorreto"));

            var curso = await _coursesService.BuscarPorIdAsync(id);

            if (curso == null)
                return NotFound(Problem(detail: "Curso não encontrado!"));

            return Ok(curso);
        }


        /// <summary>
        /// Atualizar dados do curso
        /// </summary>
        /// <param name="id"></param>
        /// <returns>IActionResult</returns>

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult>Atualizar([FromBody] CourseUpdateDto dto, int id)
        {
            if (id < 1)
                return BadRequest(Problem(detail: "Id incorreto")); 

            var atualizado = await _coursesService.AtualizarAsync(dto, id);

            if (!atualizado)
                return NotFound(Problem(detail: "Curso não atualizado/encontrado."));

            return NoContent();
        }

        /// <summary>
        /// Apagar curso
        /// </summary>
        /// <param name="id"></param>
        /// <returns>IActionResult</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Apagar(int id)
        {
            if (id < 1)
                return BadRequest(Problem(detail: "Id incorreto"));

            bool apagado = await _coursesService.ApagarAsync(id);

            if (!apagado)
                return NotFound(Problem(detail: "Curso não apagado."));

            return NoContent();
        }

    }


    
}
