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
        /// Matricular Aluno em Curso
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>IActionResult</returns>
        [HttpPost]
        [Authorize]
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
        /// Apagar estudante
        /// </summary>
        /// <returns>IActionResult</returns>
        [HttpDelete]
        [Authorize]
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
                    return Unauthorized();

                var estudanteResult = await _studentsService.BuscarPorUserIdAsync(userIdLogado);

                if (!estudanteResult.Success)
                    return estudanteResult.ToActionResult();

                // Se o estudante tentou enviar um StudentId diferente do dele → bloqueia
                if (dto.StudentId.HasValue && dto.StudentId.Value > 0 && dto.StudentId != estudanteResult.Dados!.Id)
                    return Forbid();

                dto.StudentId = estudanteResult.Dados.Id;
            }
            else
            {
                if (dto.StudentId is null || dto.StudentId <= 0)
                    return BadRequest(Problem(detail: "StudentId é obrigatório quando o usuário é Admin.", title: "Bad Request"));
            }

            return null; // tudo ok
        }

    }
}
