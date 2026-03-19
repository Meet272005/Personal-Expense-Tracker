using Microsoft.AspNetCore.Mvc;
using Expense_managment.Models;
using Expense_managment.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Expense_managment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthController(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public class LoginModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userRepository.AuthenticateAsync(model.Email, model.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                Token = token,
                User = new { user.UserId, user.Name, user.Email, user.ProfileImage }
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (await _userRepository.UserExistsAsync(user.Email))
            {
                return BadRequest(new { message = "Email already in use" });
            }

            var createdUser = await _userRepository.RegisterAsync(user);

            var token = GenerateJwtToken(createdUser);

            return Ok(new
            {
                Token = token,
                User = new { createdUser.UserId, createdUser.Name, createdUser.Email, createdUser.ProfileImage }
            });
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] User updatedUser)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            if (userId != updatedUser.UserId)
            {
                 return Forbid();
            }

            var existingUser = await _userRepository.GetUserByIdAsync(userId);
            if (existingUser == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Only update allowed fields
            existingUser.Name = updatedUser.Name;
            existingUser.Email = updatedUser.Email;
            existingUser.ProfileImage = updatedUser.ProfileImage;
            
            if (!string.IsNullOrEmpty(updatedUser.Password))
            {
                existingUser.Password = updatedUser.Password;
            }

            var success = await _userRepository.UpdateUserAsync(existingUser);

            if (!success)
            {
                return BadRequest(new { message = "Failed to update profile." });
            }

            // Generate a fresh token with updated info
            var token = GenerateJwtToken(existingUser);

            return Ok(new
            {
                Token = token,
                User = new { existingUser.UserId, existingUser.Name, existingUser.Email, existingUser.ProfileImage }
            });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "ThisIsASecretKeyForJwtAuthenticationThatNeedsToBeLongEnough"));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
