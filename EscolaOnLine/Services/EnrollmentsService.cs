using AutoMapper;
using EscolaOnLine.Data;
using EscolaOnLine.Dtos;
using EscolaOnLine.Models;
using Microsoft.EntityFrameworkCore;

namespace EscolaOnLine.Services
{
    public class EnrollmentsService
    {
        private readonly IMapper _mapper;
        private readonly EscolaDbContext _context;
        private readonly CoursesService _coursesService;
        private readonly StudentsService _studentsService;

        public EnrollmentsService(IMapper mapper, EscolaDbContext context, CoursesService coursesService, StudentsService studentsService)
        {
            _mapper = mapper;
            _context = context;
            _coursesService = coursesService;
            _studentsService = studentsService;
        }

        public async Task<ServiceResult> CadastrarAsync(EnrollmentCreateDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (dto.CourseId <= 0)
                    return ServiceResult.BadRequest("Curso Id não informado.");

                if (dto.StudentId is null || dto.StudentId <= 0)
                    return ServiceResult.BadRequest("Estudante Id não informado.");

                // Valida Curso
                var cursoResult = await _coursesService.BuscarPorIdAsync(dto.CourseId);
                if (!cursoResult.Success)
                    return ServiceResult.NotFound(cursoResult.Error ?? "Curso não encontrado.");

                if (cursoResult.Dados!.IsDeleted)
                    return ServiceResult.UnprocessableEntity("Curso não está ativo.");

                // Valida Estudante
                var estudanteResult = await _studentsService.BuscarPorIdAsync(dto.StudentId.Value);
                if (!estudanteResult.Success)
                    return ServiceResult.NotFound(estudanteResult.Error ?? "Estudante não encontrado.");

                if (estudanteResult.Dados!.IsDeleted)
                    return ServiceResult.UnprocessableEntity("Estudante não está ativo."); 

                // Verifica se já existe matrícula ATIVA
                var jaMatriculado = await _context.Enrollments
                    .AnyAsync(e => e.CourseId == dto.CourseId
                                && e.StudentId == dto.StudentId
                                && !e.IsDeleted);

                if (jaMatriculado)
                    return ServiceResult.Conflict("Estudante já matriculado no Curso.");


                // Se existir uma matrícula cancelada, será "reativada" (em vez de criar outra)
                var matriculaCancelada = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.CourseId == dto.CourseId
                                           && e.StudentId == dto.StudentId
                                           && e.IsDeleted);

                if (matriculaCancelada is not null)
                {
                    matriculaCancelada.IsDeleted = false;
                    matriculaCancelada.DataMatricula = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return ServiceResult.Created();
                }

                // Cria matrícula
                var enrollment = _mapper.Map<Enrollment>(dto);
                enrollment.DataMatricula = DateTime.UtcNow;

                await _context.AddAsync(enrollment);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.Created();

            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                return ServiceResult.Conflict("Erro ao salvar matrícula. Possível duplicidade.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ServiceResult<List<CourseReadSimplificadoDto>>> BuscarCursosDoEstudanteAsync(int idEstudante)
        {
            if (idEstudante < 1)
                return ServiceResult<List<CourseReadSimplificadoDto>>.BadRequest("Id do estudante inválido.");

            var cursosMatriculadosEstudanteIds = await _context.Enrollments
                .Where(e => e.StudentId == idEstudante && !e.IsDeleted)
                .Select(e => e.CourseId)
                .ToArrayAsync();

            if (cursosMatriculadosEstudanteIds.Length < 1)
                return new ServiceResult<List<CourseReadSimplificadoDto>>();

            var cursos = await _context.Courses
                .Where(c => cursosMatriculadosEstudanteIds
                .Contains(c.Id)).ToListAsync();

            var dto = _mapper.Map<List<CourseReadSimplificadoDto>>(cursos);

            return ServiceResult<List<CourseReadSimplificadoDto>>.Ok(dto);
        }

        public async Task<ServiceResult> ApagarAsync(int idEstudante, int idCurso, bool apagarDefinitivo = false)
        {
            if (idEstudante < 1 || idCurso < 1)
                return ServiceResult.BadRequest("Ids inválidos.");

            var matricula = await _context.Enrollments
                .FirstOrDefaultAsync(m => m.CourseId == idCurso && m.StudentId == idEstudante);

            if (matricula is null)
                return ServiceResult.NotFound("Matrícula não encontrada.");

            if (matricula.IsDeleted && !apagarDefinitivo)
                return ServiceResult.UnprocessableEntity("Matrícula já está cancelada.");

            if (apagarDefinitivo)
                _context.Enrollments.Remove(matricula);
            else
                matricula.IsDeleted = true;

            await _context.SaveChangesAsync();

            return ServiceResult.NoContent();
        }

    }
}
