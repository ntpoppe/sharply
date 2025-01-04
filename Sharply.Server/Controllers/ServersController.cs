using Microsoft.AspNetCore.Mvc;
using Sharply.Server.Interfaces;
using Sharply.Shared;
using Sharply.Shared.Requests;
using Sharply.Shared.Models;
using System.Security.Claims;

[ApiController]
[Route("api/servers")]
public class ServersController : ControllerBase
{
    private readonly IUserService _userService;
	private readonly IServerService _serverService;

    public ServersController(IUserService userService, IServerService serverService)
	{
		_userService = userService;
		_serverService = serverService;
	}

    [HttpPost("{serverId}/add-user")]
    public async Task<IActionResult> AddUserToServer(int serverId)
    {
        try
        {
            //await _userService.AddUserToServerAsync(request.UserId, serverId);
            //return Ok(new { message = "User added to server successfully" });
			await Task.Delay(2);
            return BadRequest();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

	[HttpPost("create-server")]
	public async Task<ApiResponse<ServerDto>> CreateServer([FromBody] CreateServerRequest request)
	{
		try
		{
			var createdServer = await _serverService.CreateServer(request);
			return new ApiResponse<ServerDto>
			{
				Success = true,
				Data = createdServer
			};
		}
		catch (Exception ex)
		{
		    return new ApiResponse<ServerDto>
            {
                Success = false,
                Error = ex.Message
            };

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

