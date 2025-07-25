using Main.Interfaces;
using Main.Requests.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService _authService) : BaseController
{
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] UserLoginRequest request)
    {
        var response = await _authService.UserLoginAsync(request);
        return HandleResponse(response);
    }

    [HttpPost("register")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> RegisterAsync([FromBody] UserRegisterRequest request)
    {
        var response = await _authService.UserRegisterAsync(request);
        return HandleResponse(response);
    }
}
