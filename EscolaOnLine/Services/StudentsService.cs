using AutoMapper;
using EscolaOnLine.Data;
using EscolaOnLine.Dtos;
using Microsoft.EntityFrameworkCore;

namespace EscolaOnLine.Services
{
    public class StudentsService
    {
        private readonly IMapper _mapper;
        private readonly EscolaDbContext _context;
        private readonly UserService _userService;

        public StudentsService(EscolaDbContext context,
            IMapper mapper,
            UserService userService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<ServiceResult> CadastrarAsync(StudentCreateDto dto)
        {
            var registerDto = _mapper.Map<RegisterDto>(dto);
            return await _userService.CadastrarAsync(registerDto);
        }

        public async Task<List<StudentReadSimplificadoDto>> BuscarTodosAsync(
          string? nome,
          string? ordenarPor,
          string? direcao,
          int? pagina)
        {
            const int numeroItensPorPagina = 20;

            var query = _context.Students.AsQueryable();

            // Busca por título (contém)
            if (!string.IsNullOrWhiteSpace(nome))
            {
                query = query.Where(c => c.NomeCompleto.ToLower().Contains(nome.ToLower()));
            }

            // Ordenação
            bool descendente = direcao?.ToLower() == "desc";

            query = (ordenarPor?.ToLower()) switch
            {
                "nome" => descendente
                    ? query.OrderByDescending(c => c.NomeCompleto)
                    : query.OrderBy(c => c.NomeCompleto),

                "id" => descendente
                    ? query.OrderByDescending(c => c.Id)
                    : query.OrderBy(c => c.Id),

                _ => query.OrderByDescending(c => c.DataCadastro) // padrão: mais recentes primeiro
            };

            if (pagina is null)
                return _mapper.Map<List<StudentReadSimplificadoDto>>(query);

            // Paginação
            var estudantes = await query
                .Skip(((int)pagina - 1) * numeroItensPorPagina)
                .Take(numeroItensPorPagina)
                .ToListAsync();

            return _mapper.Map<List<StudentReadSimplificadoDto>>(estudantes);
        }

        public async Task<StudentReadDto> BuscarPorIdAsync(int id)
        {
            var estudante = await _context.Students.Where(c => c.Id == id).FirstOrDefaultAsync();
            return _mapper.Map<StudentReadDto>(estudante);
        }

        public async Task<StudentReadDto> BuscarPorUserIdAsync(string userId)
        {
            var estudante = await _context.Students.Where(c => c.UserId == userId).FirstOrDefaultAsync();
            return _mapper.Map<StudentReadDto>(estudante);
        }

        public async Task<bool> AtualizarAsync(StudentUpdateDto dto, int id)
        {
            var estudante = await _context.Students
                .FirstOrDefaultAsync(c => c.Id == id);

            if (estudante is null)
                return false;

            _mapper.Map(dto, estudante);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ApagarAsync(int id, bool apagarDefinitivo = false)
        {
            var estudante = await _context.Students
                .FirstOrDefaultAsync(c => c.Id == id);

            if (estudante is null)
                return false;

            if (apagarDefinitivo)
                _context.Students.Remove(estudante);
            else
                estudante.IsDeleted = true;
           
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
