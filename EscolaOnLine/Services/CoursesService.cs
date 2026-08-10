using AutoMapper;
using EscolaOnLine.Data;
using EscolaOnLine.Dtos;
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

        public async Task<int> CadastrarAsync(CourseCreateDto dto)
        {
            var curso = _mapper.Map<Course>(dto);
            await _context.AddAsync(curso);
            await _context.SaveChangesAsync();

            return curso.Id;
        }

        public async Task<List<CourseReadSimplificadoDto>> BuscarTodosAsync(
            string? categoria,
            string? titulo,
            string? ordenarPor,
            string? direcao,
            int pagina = 1)
        {
            const int numeroItensPorPagina = 20;

            var query = _context.Courses.AsQueryable();

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
            var cursos = await query
                .Skip((pagina - 1) * numeroItensPorPagina)
                .Take(numeroItensPorPagina)
                .ToListAsync();

            return _mapper.Map<List<CourseReadSimplificadoDto>>(cursos);
        }

        public async Task<CourseReadDto> BuscarPorIdAsync(int id)
        {
            var curso = await _context.Courses.Where(c => c.Id == id).FirstOrDefaultAsync();
            return _mapper.Map<CourseReadDto>(curso);
        }

        public async Task<bool> AtualizarAsync(CourseUpdateDto dto, int id)
        {
            var curso = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso is null)
                return false;

            _mapper.Map(dto, curso);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ApagarAsync(int id)
        {
            var curso = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso is null)
                return false;

            _context.Courses.Remove(curso);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
