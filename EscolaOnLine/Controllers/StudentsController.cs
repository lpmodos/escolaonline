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
        public StudentsController(StudentsService studentsService)
        {
            _studentsService = studentsService;
        }

        /// <summary>
        /// Cadastro de Estudante
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] StudentCreateDto dto)
        {
            if (dto == null) return BadRequest(Problem(detail: "Dados não informados."));
            
            var result = await _studentsService.CadastrarAsync(dto);

            if (!result.Success)
                return StatusCode(result.StatusCode, Problem(detail: result.Error));
            return StatusCode(201, new { message = "Usuário registrado com sucesso" });
        }

        /// <summary>
        /// Listar todos Estudantes
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>IActionResult</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BuscarTodos([FromQuery]string? nome, [FromQuery]string? ordernarPor, [FromQuery]string? direcao, [FromQuery] int? pagina)
        {
            if (pagina < 1)
                return BadRequest(Problem(detail: "A página deve ser maior que zero."));
            var estudantes = _studentsService.BuscarTodosAsync(nome, ordernarPor, direcao, pagina);
            return Ok(estudantes);
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
            if (id < 1)
                return BadRequest(Problem(detail: "Id incorreto"));

            var estudante = await _studentsService.BuscarPorIdAsync(id);

            if (estudante == null)
                return NotFound(Problem(detail: "Estudante não encontrado!"));

            // Verifica se é Admin
            var isAdmin = User.IsInRole("Admin");

            // Verifica se é o próprio usuário
            var userIdLogado = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isProprioUsuario = estudante.UserId == userIdLogado;

            if (!isAdmin && !isProprioUsuario)
                return Forbid(); // 403

            return Ok(estudante);
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

            var estudante = await _studentsService.BuscarPorUserIdAsync(userIdLogado);

            if (estudante == null)
                return NotFound(Problem(detail: "Nenhuma estudante logado!"));

            return Ok(estudante);
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
            if (id < 1)
                return BadRequest(Problem(detail: "Id incorreto"));

            var estudante = await _studentsService.BuscarPorIdAsync(id);

            if (estudante == null)
                return NotFound(Problem(detail: "Estudante não encontrado!"));

            // Verifica se é Admin
            var isAdmin = User.IsInRole("Admin");

            // Verifica se é o próprio usuário
            var userIdLogado = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isProprioUsuario = estudante.UserId == userIdLogado;

            if (!isAdmin && !isProprioUsuario)
                return Forbid(); // 403

            var atualizado = await _studentsService.AtualizarAsync(dto, id);

            if (!atualizado)
                return NotFound(Problem(detail: "Estudante não atualizado/encontrado."));

            return NoContent();
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
            if (id < 1)
                return BadRequest(Problem(detail: "Id incorreto"));

            bool apagado = await _studentsService.ApagarAsync(id, (bool)apagarDefinitivo);

            if (!apagado)
                return NotFound(Problem(detail: "Estudante não apagado/desativado."));

            return NoContent();
        }
    }
}
