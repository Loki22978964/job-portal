using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PL.Services;

namespace PL.Controllers;

[AllowAnonymous]
[Route("api/auth")]
public sealed class AuthController : ApiControllerBase
{
    private readonly IUserService    _userService;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(IUserService userService, JwtTokenService jwtTokenService)
    {
        ArgumentNullException.ThrowIfNull(userService);
        ArgumentNullException.ThrowIfNull(jwtTokenService);
        _userService     = userService;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var dto    = UserMapper.ToRegisterDto(request);
        var result = await _userService.RegisterAsync(dto, cancellationToken);
        return Ok(UserMapper.ToViewModel(result.Data!));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var dto   = UserMapper.ToLoginDto(request);
            var user  = await _userService.AuthenticateAsync(dto, cancellationToken);
            var token = _jwtTokenService.GenerateToken(user);
            return Ok(new AuthResponse(token, UserMapper.ToViewModel(user)));
        }
        catch (AuthorizationException ex)
        {
            
            return Unauthorized(new ErrorResponse(ex.Message));
        }
    }
}
