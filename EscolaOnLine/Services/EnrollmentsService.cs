using AutoMapper;
using EscolaOnLine.Data;
using EscolaOnLine.Dtos;
using EscolaOnLine.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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
                    return ServiceResult.Fail("Curso Id não informado.", 400);

                if (dto.StudentId <= 0)
                    return ServiceResult.Fail("Estudante Id não informado.", 400);

                // Valida Curso
                var curso = await _coursesService.BuscarPorIdAsync(dto.CourseId);
                if (curso is null)
                    return ServiceResult.Fail("Curso não encontrado.", 404);

                if (curso.IsDeleted)
                    return ServiceResult.Fail("Curso não está ativo.", 400);

                // Valida Estudante
                var estudante = await _studentsService.BuscarPorIdAsync(dto.StudentId.Value);
                if (estudante is null)
                    return ServiceResult.Fail("Estudante não encontrado.", 404);

                if (estudante.IsDeleted)
                    return ServiceResult.Fail("Estudante não está ativo.", 400);

                // Verifica se já existe matrícula ATIVA
                var jaMatriculado = await _context.Enrollments
                    .AnyAsync(e => e.CourseId == dto.CourseId
                                && e.StudentId == dto.StudentId
                                && !e.IsDeleted);

                if (jaMatriculado)
                    return ServiceResult.Fail("Estudante já matriculado no Curso.", 409);


                // Opcional: se existir uma matrícula cancelada, você pode "reativar" em vez de criar outra
                var matriculaCancelada = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.CourseId == dto.CourseId
                                           && e.StudentId == dto.StudentId
                                           && e.IsDeleted);

                if (matriculaCancelada is not null)
                {
                    matriculaCancelada.IsDeleted = false;
                    matriculaCancelada.DataMatricula = DateTime.Now;
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return ServiceResult.Created();
                }

                // Cria matrícula
                var enrollment = _mapper.Map<Enrollment>(dto);
                enrollment.DataMatricula = DateTime.Now;

                await _context.AddAsync(enrollment);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.Created();

            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                return ServiceResult.Fail("Erro ao salvar matrícula. Possível duplicidade.", 409);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ServiceResult<List<CourseReadSimplificadoDto>>> BuscarCursosDoEstudanteAsync(int idEstudante)
        {
            var cursosMatriculadosEstudante = await _context.Enrollments.Where(e => e.StudentId == idEstudante && !e.IsDeleted).Select(e => e.CourseId).ToArrayAsync();

            if (cursosMatriculadosEstudante is null)
                return new ServiceResult<List<CourseReadSimplificadoDto>>();

            var cursos = await _context.Courses.Where(c => cursosMatriculadosEstudante.Contains(c.Id)).ToListAsync();

            var dto = _mapper.Map<List<CourseReadSimplificadoDto>>(cursos);

            return ServiceResult<List<CourseReadSimplificadoDto>>.Ok(dto);
        }

        public async Task<ServiceResult> ApagarAsync(int idEstudante, int idCurso, bool apagarDefinitivo = false)
        {
            var matricula = await _context.Enrollments
                .FirstOrDefaultAsync(m => m.CourseId == idCurso && m.StudentId == idEstudante);

            if (matricula is null)
                return ServiceResult.Fail("Matrícula não encontrada.", 404);

            if (matricula.IsDeleted && !apagarDefinitivo)
                return ServiceResult.Fail("Matrícula já está cancelada.", 409);

            if (apagarDefinitivo)
                _context.Enrollments.Remove(matricula);
            else
                matricula.IsDeleted = true;

            await _context.SaveChangesAsync();

            return ServiceResult.Ok(); 
        }

    }
}
