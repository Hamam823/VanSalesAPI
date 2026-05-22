using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
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
        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto dto)
        {
            // 1️⃣ التأكد إذا المستخدم موجود
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (existingUser != null)
                return BadRequest("Username already exists");

            // 2️⃣ إنشاء المستخدم
            var user = new AppUser
            {
                Username = dto.Username,
                PasswordHash = dto.Password, // لاحقًا نحولها BCrypt
                Role = dto.Role,
                SalesmanId = dto.SalesmanId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User registered successfully",
                userId = user.Id
            });
        }
        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null)
                return BadRequest("Invalid username");

            if (user.PasswordHash != dto.Password)
                return BadRequest("Invalid password");

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("THIS_IS_A_SUPER_SECURE_VAN_SALES_API_SECRET_KEY_2026_ABC123!");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                new System.Security.Claims.Claim("id", user.Id.ToString()),
                new System.Security.Claims.Claim("role", user.Role)
            }),
                Expires = DateTime.UtcNow.AddDays(1),
                Issuer = "VanSalesAPI",
                Audience = "VanSalesAPI",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                role = user.Role
            });
        }
    }
}
