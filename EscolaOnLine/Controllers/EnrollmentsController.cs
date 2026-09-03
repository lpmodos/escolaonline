using EscolaOnLine.Dtos;
using EscolaOnLine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EscolaOnLine.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EnrollmentsController : ControllerBase
    {

        private readonly EnrollmentsService _enrollmentsService;
        private readonly StudentsService _studentsService;

        public EnrollmentsController(EnrollmentsService enrollmentsService,
            StudentsService studentsService)
        {
            _enrollmentsService = enrollmentsService;
            _studentsService = studentsService;
        }

        /// <summary>
        /// Matricular aluno em curso. Aluno autentica e matricula a si mesmo; Admin informa studentId.
        /// </summary>
        /// <param name="dto"></param>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(CreatedIdDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Cadastrar([FromBody] EnrollmentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var resultadoPermissao = await ResolverStudentIdAsync(dto);
            if (resultadoPermissao is not null)
                return resultadoPermissao;

            var result = await _enrollmentsService.CadastrarAsync(dto);

            return result.ToActionResult();
        }

        /// <summary>
        /// Cancelar matrícula. Aluno cancela a própria; Admin pode informar studentId. Hard delete só Admin.
        /// </summary>
        /// <param name="courseId">Id do Curso.</param>
        /// <param name="studentId">Id do Estudante.</param>
        /// <param name="apagarDefinitivo">Apagar em definitivo. Requer credencial Admin</param>
        [HttpDelete]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Apagar([FromQuery] int courseId, [FromQuery] int? studentId = null, [FromQuery] bool apagarDefinitivo = false)
        {
            var dto = new EnrollmentCreateDto
            {
                CourseId = courseId,
                StudentId = studentId
            };

            var resultadoPermissao = await ResolverStudentIdAsync(dto);
            if (resultadoPermissao is not null)
                return resultadoPermissao;

            var result = await _enrollmentsService.ApagarAsync(
                dto.StudentId!.Value,
                dto.CourseId,
                apagarDefinitivo);

            return result.ToActionResult();
        }

        /// <summary>
        /// Resolve o StudentId de acordo com o perfil (Admin ou Estudante)
        /// </summary>
        private async Task<IActionResult?> ResolverStudentIdAsync(EnrollmentCreateDto dto)
        {
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin)
            {
                var userIdLogado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(userIdLogado))
                    return Unauthorized(); // 401 — token malformado

                var estudanteResult = await _studentsService.BuscarPorUserIdAsync(userIdLogado);

                if (!estudanteResult.Success)
                    return estudanteResult.ToActionResult();

                // Se o estudante tentou enviar um StudentId diferente do dele → bloqueia
                if (dto.StudentId.HasValue && dto.StudentId.Value > 0 && dto.StudentId != estudanteResult.Dados!.Id)
                    return Forbid(); // 403 — tentou matricular outro aluno

                dto.StudentId = estudanteResult.Dados.Id;
            }
            else
            {
                if (dto.StudentId is null || dto.StudentId <= 0)
                    return BadRequest(Problem(
                        detail: "StudentId é obrigatório quando o usuário é Admin.", 
                        title: "Bad Request"));
            }

            return null; // tudo ok
        }

    }
}
