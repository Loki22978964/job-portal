using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace PL.Services;

public sealed class JwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings.Value;
    }

    public string GenerateToken(UserDto user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.Email,           user.Email),
            new Claim(ClaimTypes.Role,            user.RoleName),
            new Claim(ClaimTypes.GivenName,       user.FirstName),
            new Claim(ClaimTypes.Surname,         user.LastName),
        ];

        var token = new JwtSecurityToken(
            issuer:            _settings.Issuer,
            audience:          _settings.Audience,
            claims:            claims,
            expires:           DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
