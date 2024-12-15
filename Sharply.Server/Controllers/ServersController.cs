using Microsoft.AspNetCore.Mvc;
using Sharply.Server.Services;
using Sharply.Shared.Requests;

[ApiController]
[Route("api/servers")]
public class ServersController : ControllerBase
{
    private readonly UserService _userService;

    public ServersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("{serverId}/users")]
    public async Task<IActionResult> AddUserToServer(int serverId, [FromBody] AddUserRequest request)
    {
        try
        {
            await _userService.AddUserToServerAsync(request.UserId, serverId);
            return Ok(new { message = "User added to server successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

