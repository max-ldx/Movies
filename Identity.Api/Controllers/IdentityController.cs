using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Controllers;

[ApiController]
public class IdentityController : ControllerBase
{
    private const string TokenSecret = "ForTheLoveOfGodStoreAndLoadThisSecurely";
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(8);

    [HttpPost("token")]
    public IActionResult GenerateToken(
        [FromBody] TokenGenerationRequest request)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(TokenSecret);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, request.Email),
            new(JwtRegisteredClaimNames.Email, request.Email),
            new("userid", request.UserId.ToString())
        };

        claims.AddRange(from claimPair in request.CustomClaims
            let jsonElement = (JsonElement)claimPair.Value
            let valueType = jsonElement.ValueKind switch
            {
                JsonValueKind.True or JsonValueKind.False => ClaimValueTypes.Boolean,
                JsonValueKind.Number => ClaimValueTypes.Double,
                _ => ClaimValueTypes.String
            }
            select new Claim(claimPair.Key, claimPair.Value.ToString()!, valueType));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(TokenLifetime),
            Issuer = "https://id.maximilienledoux.com",
            Audience = "https://movies.maximilienledoux.com",
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        var jwt = tokenHandler.WriteToken(token);
        return Ok(jwt);
    }
}