using Microsoft.AspNetCore.Mvc;
using Sharply.Server.Services;
using Sharply.Shared;
using Sharply.Shared.Models;
using System.Security.Claims;

[ApiController]
[Route("api/servers")]
public class ServersController : ControllerBase
{
    private readonly UserService _userService;

    public ServersController(UserService userService)
        => _userService = userService;

    [HttpPost("{serverId}/add-user")]
    public async Task<IActionResult> AddUserToServer(int serverId)
    {
        try
        {
            //await _userService.AddUserToServerAsync(request.UserId, serverId);
            //return Ok(new { message = "User added to server successfully" });
            return BadRequest();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("get-user-servers")]
    public async Task<ApiResponse<List<ServerDto>>> GetUserServers()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return new ApiResponse<List<ServerDto>>
                {
                    Success = false,
                    Error = "User ID not found."
                };

            var userId = Int32.Parse(userIdClaim);
            var userServers = await _userService.GetServersForUserAsync(userId);

            return new ApiResponse<List<ServerDto>>
            {
                Success = true,
                Data = userServers
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<ServerDto>>
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
}

