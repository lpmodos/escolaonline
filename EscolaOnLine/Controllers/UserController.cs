using EscolaOnLine.Data;
using EscolaOnLine.Dtos;
using EscolaOnLine.Models;
using EscolaOnLine.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EscolaOnLine.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TokenService _tokenService;
        private readonly EscolaDbContext _context;

        public UserController(
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

        /// <summary>
        /// Registrar novo usuário
        /// </summary>
        [HttpPost("cadastrar")]
        public async Task<IActionResult> Cadastrar([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(Problem(detail: "Dados inválidos."));

            // Valida a role
            var rolesPermitidas = new[] { "Admin", "Instructor", "Student" };
            if (!rolesPermitidas.Contains(dto.Role))
                return BadRequest(Problem(detail: "Role inválida. Use: Admin, Instructor ou Student."));

            var userExistente = await _userManager.FindByEmailAsync(dto.Email);
            if (userExistente != null)
                return BadRequest(Problem(detail: "E-mail já cadastrado."));

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
                return BadRequest(Problem(detail: erros));
            }

            // Garante que a role existe
            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new IdentityRole(dto.Role));

            await _userManager.AddToRoleAsync(user, dto.Role);

            // Se for Student, cria o registro na tabela Students
            // SERÁ SUBSTITUÍDO PELO POR SERVIÇO DE ESTUDANTES 
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

            return StatusCode(201, new { message = "Usuário registrado com sucesso" });
        }

        /// <summary>
        /// Login
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(Problem(detail: "Dados inválidos."));

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized(Problem(detail: "E-mail ou senha inválidos."));

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
                return Unauthorized(Problem(detail: "E-mail ou senha inválidos."));

            var roles = await _userManager.GetRolesAsync(user);
            var tokenResponse = _tokenService.GerarToken(user, roles);

            return Ok(tokenResponse);
        }

        /// <summary>
        /// Refresh Token
        /// </summary>
        [HttpPost("token/refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token))
                return BadRequest(Problem(detail: "Token é obrigatório."));

            var principal = _tokenService.GetPrincipalFromExpiredToken(dto.Token);
            if (principal == null)
                return Unauthorized(Problem(detail: "Token inválido."));

            var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(Problem(detail: "Token inválido."));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized(Problem(detail: "Usuário não encontrado."));

            var roles = await _userManager.GetRolesAsync(user);
            var tokenResponse = _tokenService.GerarToken(user, roles);

            return Ok(tokenResponse);
        }
    }
}
