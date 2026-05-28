using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using VanSalesAPI.Data;
using VanSalesAPI.DTOs;
using VanSalesAPI.Models;

namespace VanSalesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 🧾 REGISTER
        // =====================================================
        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto dto)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (existingUser != null)
                return BadRequest(new ApiResponse<string>(
                    false,
                    "Username already exists",
                    null
                ));

            var user = new AppUser
            {
                Username = dto.Username,
                PasswordHash = dto.Password, // لاحقاً BCrypt
                Role = dto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>(
                true,
                "User registered successfully",
                new
                {
                    userId = user.Id
                }
            ));
        }

        // =====================================================
        // 🔐 LOGIN
        // =====================================================
        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null)
                return BadRequest(new ApiResponse<string>(
                    false,
                    "Invalid username",
                    null
                ));

            if (user.PasswordHash != dto.Password)
                return BadRequest(new ApiResponse<string>(
                    false,
                    "Invalid password",
                    null
                ));

            var key = Encoding.UTF8.GetBytes(
                "THIS_IS_A_SUPER_SECURE_VAN_SALES_API_SECRET_KEY_2026_ABC123!"
            );

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                Issuer = "VanSalesAPI",
                Audience = "VanSalesAPI",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new ApiResponse<object>(
                true,
                "Login successful",
                new
                {
                    token = tokenHandler.WriteToken(token),
                    role = user.Role,
                    userId = user.Id
                }
            ));
        }
    }
}