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
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _coursesService.CadastrarAsync(dto);

            if (!result.Success)
                return result.ToActionResult();

            return CreatedAtAction(nameof(BuscarPorId), new { id = result.Dados }, new { id = result.Dados });
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
            var result = await _coursesService.BuscarTodosAsync(categoria, titulo, ordenarPor, direcao, pagina);
            return result.ToActionResult();
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
            var result = await _coursesService.BuscarPorIdAsync(id);
            return result.ToActionResult();
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
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _coursesService.AtualizarAsync(dto, id);
            return result.ToActionResult();
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
            var result = await _coursesService.ApagarAsync(id);
            return result.ToActionResult();
        }
    }   
}
