using AutoMapper;
using EscolaOnLine.Data;
using EscolaOnLine.Dtos;
using EscolaOnLine.Models;
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
            registerDto.Role = "Student"; // Garante que a role seja sempre Student
            return await _userService.CadastrarAsync(registerDto);
        }

        public async Task<ServiceResult<List<StudentReadSimplificadoDto>>> BuscarTodosAsync(
          string? nome,
          string? ordenarPor,
          string? direcao,
          int? pagina)
        {
            if (pagina < 1)
                return ServiceResult<List<StudentReadSimplificadoDto>>.BadRequest("A página deve ser maior que zero.");

            const int numeroItensPorPagina = 20;

            var query = _context.Students.Where(c => !c.IsDeleted).AsQueryable();

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

            // Paginação
            var estudantes = pagina is null ? 
                await query.ToListAsync() : 
                await query
                .Skip(((int)pagina - 1) * numeroItensPorPagina)
                .Take(numeroItensPorPagina)
                .ToListAsync();

            var dto = _mapper.Map<List<StudentReadSimplificadoDto>>(estudantes);
            return ServiceResult<List<StudentReadSimplificadoDto>>.Ok(dto);
        }

        public async Task<ServiceResult<StudentReadDto>> BuscarPorIdAsync(int id)
        {
            if (id < 1)
                return ServiceResult<StudentReadDto>.BadRequest("Id incorreto.");

            var estudante = await _context.Students.Where(c => c.Id == id && !c.IsDeleted).FirstOrDefaultAsync();

            if (estudante is null)
                return ServiceResult<StudentReadDto>.NotFound("Estudante não encontrado.");

            var dto = _mapper.Map<StudentReadDto>(estudante);
            return ServiceResult<StudentReadDto>.Ok(dto);
        }

        public async Task<ServiceResult<StudentReadDto>> BuscarPorUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResult<StudentReadDto>.BadRequest("UserId incorreto.");

            var estudante = await _context.Students.Where(c => c.UserId == userId && !c.IsDeleted).FirstOrDefaultAsync();

            if (estudante is null)
                return ServiceResult<StudentReadDto>.NotFound("Estudante não encontrado.");

            var dto = _mapper.Map<StudentReadDto>(estudante);
            return ServiceResult<StudentReadDto>.Ok(dto);
        }

        public async Task<ServiceResult> AtualizarAsync(StudentUpdateDto dto, int id)
        {
            if (id < 1)
                return ServiceResult.BadRequest("Id incorreto.");

            var estudante = await _context.Students
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (estudante is null)
                return ServiceResult.NotFound("Estudante não encontrado.");

            _mapper.Map(dto, estudante);
            await _context.SaveChangesAsync();

            return ServiceResult.NoContent();
        }

        public async Task<ServiceResult> ApagarAsync(int id, bool apagarDefinitivo = false)
        {
            if (id < 1)
                return ServiceResult.BadRequest("Id incorreto.");

            var estudante = await _context.Students
                .FirstOrDefaultAsync(c => c.Id == id);

            if (estudante is null)
                return ServiceResult.NotFound("Estudante não encontrado."); ;

            if (apagarDefinitivo)
                _context.Students.Remove(estudante);
            else
                estudante.IsDeleted = true;
           
            await _context.SaveChangesAsync();
            return ServiceResult.NoContent();
        }

    }
}
