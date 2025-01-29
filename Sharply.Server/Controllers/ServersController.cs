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

	[HttpPost("create-server")]
	public async Task<ApiResponse<ServerDto>> CreateServer([FromBody] CreateServerRequest request)
	{
		try
		{
			var createdServer = await _serverService.CreateServerAsync(request);
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

	[HttpPost("join-server")]
	public async Task<ApiResponse<ServerDto>> JoinServer([FromBody] JoinServerRequest request)
	{
		try
		{
			var server = await _serverService.GetServerByInviteCodeAsync(request.InviteCode);
			if (server == null)
				return new ApiResponse<ServerDto> { Success = false, Error = "Invalid invite code." };

			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userIdClaim == null)
				return new ApiResponse<ServerDto> { Success = false, Error = "User not authenticated." };

			var userId = int.Parse(userIdClaim);
			var result = await _serverService.AddUserToServerAsync(userId, server.Id);

			return new ApiResponse<ServerDto>
			{
				Success = result,
				Data = server 
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

	[HttpPost("soft-delete-server")]
	public async Task<ApiResponse<bool>> SoftDeleteServer([FromBody] int serverId)
	{
		try
		{
			await _serverService.SoftDeleteServerAsync(serverId);
			return new ApiResponse<bool>
			{
				Success = true,
			};
		}
		catch (Exception ex)
		{
			return new ApiResponse<bool>
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

