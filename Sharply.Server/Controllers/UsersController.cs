using Microsoft.AspNetCore.Mvc;
using Sharply.Server.Interfaces;
using Sharply.Shared;
using Sharply.Shared.Models;
using System.Security.Claims;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("get-user-data")]
    public async Task<ApiResponse<UserDto>> GetUserDto()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return new ApiResponse<UserDto>
                {
                    Success = false,
                    Error = "User ID not found."
                };

            var userId = Int32.Parse(userIdClaim);

            var dto = await _userService.GetUserDto(userId);

            return new ApiResponse<UserDto>
            {
                Success = true,
                Data = dto
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<UserDto>
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

}

