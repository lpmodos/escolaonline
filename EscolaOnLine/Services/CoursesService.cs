using AutoMapper;
using EscolaOnLine.Data;
using EscolaOnLine.Dtos;
using EscolaOnLine.Exceptions;
using EscolaOnLine.Models;
using Microsoft.EntityFrameworkCore;

namespace EscolaOnLine.Services
{
    public class CoursesService
    {
        private readonly IMapper _mapper;
        private readonly EscolaDbContext _context;

        public CoursesService(EscolaDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResult<int>> CadastrarAsync(CourseCreateDto dto)
        {
            var curso = _mapper.Map<Course>(dto);
            try
            {
                await _context.AddAsync(curso);
                await _context.SaveChangesAsync();

                return ServiceResult<int>.Created(curso.Id);
            }
            catch (DbUpdateException)
            {
                throw new ConflictException("Não foi possível cadastrar curso devido a um conflito de dados.");
            }
        }

        public async Task<ServiceResult<List<CourseReadSimplificadoDto>>> BuscarTodosAsync(
                string? categoria,
                string? titulo,
                string? ordenarPor,
                string? direcao,
                int? pagina)
        {
            if (pagina < 1)
                return ServiceResult<List<CourseReadSimplificadoDto>>.BadRequest("A página deve ser maior que zero.");

            const int numeroItensPorPagina = 20;

            var query = _context.Courses.Where(c => !c.IsDeleted).AsQueryable();

            // Filtro por categoria (opcional)
            if (!string.IsNullOrWhiteSpace(categoria))
            {
                query = query.Where(c => c.Categoria.ToLower() == categoria.ToLower());
            }

            // Busca por título (contém)
            if (!string.IsNullOrWhiteSpace(titulo))
            {
                query = query.Where(c => c.Titulo.ToLower().Contains(titulo.ToLower()));
            }

            // Ordenação
            bool descendente = direcao?.ToLower() == "desc";

            query = (ordenarPor?.ToLower()) switch
            {
                "titulo" => descendente
                    ? query.OrderByDescending(c => c.Titulo)
                    : query.OrderBy(c => c.Titulo),

                "data" => descendente
                    ? query.OrderByDescending(c => c.DataCriacao)
                    : query.OrderBy(c => c.DataCriacao),

                _ => query.OrderByDescending(c => c.DataCriacao) // padrão: mais recentes primeiro
            };

            // Paginação
            var cursos = pagina is null ?
                await query.ToListAsync() :
                await query
                .Skip(((int)pagina - 1) * numeroItensPorPagina)
                .Take(numeroItensPorPagina)
                .ToListAsync();


            var result = _mapper.Map<List<CourseReadSimplificadoDto>>(cursos);
            return ServiceResult<List<CourseReadSimplificadoDto>>.Ok(result);
        }

        public async Task<ServiceResult<CourseReadDto>> BuscarPorIdAsync(int id)
        {
            if (id < 1)
                return ServiceResult<CourseReadDto>.BadRequest("Id incorreto.");

            var curso = await _context.Courses.Where(c => c.Id == id).FirstOrDefaultAsync();

            if (curso is null)
                return ServiceResult<CourseReadDto>.NotFound("Curso não encontrado.");

            var dto = _mapper.Map<CourseReadDto>(curso);
            return ServiceResult<CourseReadDto>.Ok(dto);
        }

        public async Task<ServiceResult> AtualizarAsync(CourseUpdateDto dto, int id)
        {
            if (id < 1)
                return ServiceResult.BadRequest("Id incorreto.");

            var curso = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso is null)
                return ServiceResult.NotFound("Curso não encontrado.");

            _mapper.Map(dto, curso);

            try
            {
                await _context.SaveChangesAsync();
                return ServiceResult.NoContent();
            }
            catch (DbUpdateException)
            {
                throw new ConflictException("Não foi possível atualizar curso devido a um conflito de dados.");
            }
        }

        public async Task<ServiceResult> ApagarAsync(int id, bool apagarDefinitivo = false)
        {
            if (id < 1)
                return ServiceResult.BadRequest("Id incorreto.");

            var curso = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso is null)
                return ServiceResult.NotFound("Curso não encontrado.");

            if (curso.IsDeleted && !apagarDefinitivo)
                return ServiceResult.UnprocessableEntity("Curso já está excluído.");

            try
            {
                if (apagarDefinitivo)
                    _context.Courses.Remove(curso);
                else
                    curso.IsDeleted = true;

                await _context.SaveChangesAsync();
                return ServiceResult.NoContent();
            }
            catch (DbUpdateException)
            {
                throw new ConflictException("Não foi possível excluir curso devido a um conflito de dados.");
            }
        }
    }
}
