using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace PL.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    
    protected int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null || !int.TryParse(claim.Value, out var id))
            throw new InvalidOperationException(
                "The authenticated principal does not carry a valid integer NameIdentifier claim.");
        return id;
    }
}
