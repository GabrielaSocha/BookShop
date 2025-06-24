using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BookShoptry.Data;
using BookShoptry.Models;
using BookShoptry.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace BookShoptry.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorizeController : ControllerBase
{
    private readonly StoreContext _context;
    private readonly IConfiguration _config;

    public AuthorizeController(StoreContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _context.Customers.AnyAsync(u => u.Username == dto.Username))
            return BadRequest("Username already taken");

        var roleToSet = "User";

        if (!string.IsNullOrWhiteSpace(dto.Role) && NormalizeRole(dto.Role) == "Admin")
        {
            var anyAdminExists = await _context.Customers.AnyAsync(u => u.Role == "Admin");
            if (anyAdminExists)
                return StatusCode(403, "Admin already exists. Only an existing admin can assign this role.");

            roleToSet = "Admin";
        }

        var user = new Customer
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = roleToSet
        };

        _context.Customers.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { user.Id, user.Username, user.Email, user.Role });
    }



    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _context.Customers.SingleOrDefaultAsync(x => x.Username == dto.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        var token = GenerateJwtToken(user);

        return Content($"Bearer {token}", "text/plain");
    }

    [Authorize]
    [HttpPut("update-account")]
    public async Task<IActionResult> UpdateAccount(UpdateAccountDto dto)
    {
        var tokenUsername = User.Identity?.Name;
        var isAdmin = User.IsInRole("Admin");

        if (string.IsNullOrEmpty(tokenUsername))
            return Unauthorized("Invalid token");

        Customer? userToUpdate;

        if (isAdmin)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
                return BadRequest("Username is required for admin update.");

            userToUpdate = await _context.Customers
                .SingleOrDefaultAsync(x => x.Username.ToLower() == dto.Username.Trim().ToLower());

            if (userToUpdate == null)
                return NotFound("User not found.");
        }
        else
        {
            // Zwykły użytkownik — ignorujemy dto.Username i aktualizujemy tylko jego konto
            if (!string.IsNullOrEmpty(dto.Username) && dto.Username.ToLower() != tokenUsername.ToLower())
                return Unauthorized("You are only allowed to update your own account.");

            userToUpdate = await _context.Customers
                .SingleOrDefaultAsync(x => x.Username.ToLower() == tokenUsername.ToLower());

            if (userToUpdate == null)
                return NotFound("User not found.");
        }

        // Walidacja i aktualizacja danych
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            if (await _context.Customers.AnyAsync(x => x.Email == dto.Email && x.Id != userToUpdate.Id))
                return BadRequest("Email is already taken.");

            userToUpdate.Email = dto.Email;
        }

        if (!string.IsNullOrWhiteSpace(dto.NewUsername))
        {
            if (await _context.Customers.AnyAsync(x => x.Username == dto.NewUsername && x.Id != userToUpdate.Id))
                return BadRequest("Username is already taken.");

            userToUpdate.Username = dto.NewUsername;
        }

        await _context.SaveChangesAsync();
        return Ok("Account updated successfully");
    }


    [Authorize]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        var tokenUsername = User.Identity?.Name;
        var isAdmin = User.IsInRole("Admin");

        if (string.IsNullOrEmpty(tokenUsername))
            return Unauthorized("Invalid token");

        // Znajdź użytkownika po e-mailu
        var user = await _context.Customers.SingleOrDefaultAsync(x => x.Email == dto.Email);
        if (user == null)
            return NotFound("User not found.");

        // Jeśli użytkownik nie jest adminem, sprawdzamy, czy e-mail należy do niego
        if (!isAdmin && !string.Equals(user.Username, tokenUsername, StringComparison.OrdinalIgnoreCase))
            return Unauthorized("You are only allowed to reset your own password.");

        // Sprawdzenie, czy nowe hasło jest takie samo jak stare
        if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
            return BadRequest("New password cannot be the same as the current password.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();

        return Ok("Password reset successful.");
    }


    [Authorize(Roles = "Admin")]
    [HttpPut("change-role")]
    public async Task<IActionResult> ChangeUserRole(ChangeUserRoleDto dto)
    {
        var user = await _context.Customers.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null)
            return NotFound("User not found");

        var normalizedRole = NormalizeRole(dto.NewRole);
        user.Role = normalizedRole;

        await _context.SaveChangesAsync();

        return Ok(new { user.Id, user.Username, user.Role });
    }

    private string NormalizeRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return "User";

        role = role.Trim().ToLower();

        return role switch
        {
            "admin" => "Admin",
            "user" => "User",
            _ => "User" // domyślnie User, jeśli ktoś poda np. "administrator"
        };
    }



    private string GenerateJwtToken(Customer user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim("id", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username), // <- TO DODAJEMY
            new Claim(ClaimTypes.Role, user.Role ?? "User")
        };


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
