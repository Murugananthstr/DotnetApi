using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetApi.Controllers;

[ApiController]
[Route("api/oauth-test")]
[Authorize]
public class OAuthTestController : ControllerBase
{
    [HttpGet("me", Name = "GetOAuthTestUser")]
    public IActionResult GetAuthenticatedUser()
    {
        return Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            Name = User.Identity?.Name,
            Claims = User.Claims.Select(claim => new
            {
                claim.Type,
                claim.Value
            })
        });
    }
}