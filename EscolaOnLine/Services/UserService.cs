using EscolaOnLine.Data;
using EscolaOnLine.Dtos;
using EscolaOnLine.Models;
using Microsoft.AspNetCore.Identity;

namespace EscolaOnLine.Services
{
    public class UserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TokenService _tokenService;
        private readonly EscolaDbContext _context;

        public UserService(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            TokenService tokenService,
            EscolaDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _context = context;
        }

        public async Task<ServiceResult> CadastrarAsync(RegisterDto dto)
        {
            // Validação de Role
            var rolesPermitidas = new[] { "Admin", "Instructor", "Student" };
            if (!rolesPermitidas.Contains(dto.Role))
                return ServiceResult.BadRequest("Role inválida. Use: Admin, Instructor ou Student.");

            // Verifica se e-mail já existe
            var userExistente = await _userManager.FindByEmailAsync(dto.Email);
            if (userExistente != null)
                return ServiceResult.Conflict("E-mail já cadastrado.");

            // Cria o usuário
            var user = new IdentityUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true
            };

            var identityResult = await _userManager.CreateAsync(user, dto.Password);

            if (!identityResult.Succeeded)
            {
                // Transforma os erros do Identity em dicionário (formato ProblemDetails)
                var errors = identityResult.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.Description).ToArray()
                    );

                return ServiceResult.UnprocessableEntity(
                    "Não foi possível criar o usuário.",
                    errors
                );
            }

            // Garante que a Role existe
            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new IdentityRole(dto.Role));

            // Adiciona o usuário na Role
            await _userManager.AddToRoleAsync(user, dto.Role);

            // Se for Student, cria o registro na tabela Students
            if (dto.Role == "Student")
            {
                _context.Students.Add(new Student
                {
                    NomeCompleto = dto.NomeCompleto,
                    UserId = user.Id,
                    DataCadastro = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            // Sucesso
            return ServiceResult<object>.Created(new
            {
                Id = user.Id,
                Email = user.Email,
                Role = dto.Role,
                Message = "Usuário registrado com sucesso"
            });
        }

        public async Task<ServiceResult<AuthResponseDto>> LogarAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            // Por segurança, sempre retornamos a mesma mensagem (evita enumeração de usuários)
            if (user == null)
                return ServiceResult<AuthResponseDto>.Unauthorized("E-mail ou senha inválidos.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                // Lockout
                if (result.IsLockedOut)
                    return ServiceResult<AuthResponseDto>.Unauthorized("Conta temporariamente bloqueada. Tente novamente mais tarde.");

                return ServiceResult<AuthResponseDto>.Unauthorized("E-mail ou senha inválidos.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var tokenResponse = _tokenService.GerarToken(user, roles);

            return ServiceResult<AuthResponseDto>.Ok(tokenResponse);
        }

        public async Task<ServiceResult<AuthResponseDto>> AtualizarTokenAsync(TokenDto dto)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(dto.Token);

            if (principal == null)
                return ServiceResult<AuthResponseDto>.Unauthorized("Token inválido ou expirado.");

            var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return ServiceResult<AuthResponseDto>.Unauthorized("Token inválido.");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return ServiceResult<AuthResponseDto>.Unauthorized("Usuário não encontrado.");

            var roles = await _userManager.GetRolesAsync(user);
            var tokenResponse = _tokenService.GerarToken(user, roles);

            return ServiceResult<AuthResponseDto>.Ok(tokenResponse);
        }

    }
}