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
            var rolesPermitidas = new[] { "Admin", "Instructor", "Student" };
            if (!rolesPermitidas.Contains(dto.Role))
                return ServiceResult.Fail("Role inválida. Use: Admin, Instructor ou Student.", 400);

            var userExistente = await _userManager.FindByEmailAsync(dto.Email);
            if (userExistente != null)
                return ServiceResult.Fail("E-mail já cadastrado.", 409);

            var user = new IdentityUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var erros = string.Join(" | ", result.Errors.Select(e => e.Description));
                return ServiceResult.Fail(erros, 400);
            }

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new IdentityRole(dto.Role));

            await _userManager.AddToRoleAsync(user, dto.Role);

            if (dto.Role == "Student")
            {
                var student = new Student
                {
                    NomeCompleto = dto.NomeCompleto,
                    UserId = user.Id,
                    DataCadastro = DateTime.UtcNow
                };
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
            }

            return ServiceResult.Created();
        }

        public async Task<ServiceResult<AuthResponseDto>> LogarAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return ServiceResult<AuthResponseDto>.Fail("E-mail ou senha inválidos.", 401);

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
                return ServiceResult<AuthResponseDto>.Fail("E-mail ou senha inválidos.", 401);

            var roles = await _userManager.GetRolesAsync(user);
            var tokenResponse = _tokenService.GerarToken(user, roles);

            return ServiceResult<AuthResponseDto>.Ok(tokenResponse);
        }

        public async Task<ServiceResult<AuthResponseDto>> AtualizarTokenAsync(TokenDto dto)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(dto.Token);
            if (principal == null)
                return ServiceResult<AuthResponseDto>.Fail("Token inválido.", 401);

            var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return ServiceResult<AuthResponseDto>.Fail("Token inválido.", 401);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ServiceResult<AuthResponseDto>.Fail("Usuário não encontrado.", 401);

            var roles = await _userManager.GetRolesAsync(user);
            var tokenResponse = _tokenService.GerarToken(user, roles);

            return ServiceResult<AuthResponseDto>.Ok(tokenResponse);
        }
    }
}