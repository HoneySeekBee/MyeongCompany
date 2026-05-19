using Microsoft.AspNetCore.Mvc;
using MSMES.Application.Auth;

namespace MSMES.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LoginHandler _login;
    public AuthController(LoginHandler login) => _login = login;

    public sealed record LoginRequest(string UserId, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var result = await _login.HandleAsync(new LoginCommand(req.UserId, req.Password), ct);
        if (result is null) return Unauthorized(new { message = "Invalid credentials" });
        return Ok(result);
    }
}
