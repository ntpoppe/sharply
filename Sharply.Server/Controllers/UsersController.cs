using Microsoft.AspNetCore.Mvc;
using Sharply.Shared;
using Sharply.Shared.Requests;
using System.Security.Claims;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{

    public UsersController() { }

    [HttpGet("get-user-token-data")]
    public ApiResponse<UserTokenDataRequest> GetUserTokenData()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return new ApiResponse<UserTokenDataRequest>
                {
                    Success = false,
                    Error = "User ID not found."
                };

            var usernameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
            if (usernameClaim == null)
                return new ApiResponse<UserTokenDataRequest>
                {
                    Success = false,
                    Error = "Username not found."
                };

            var userId = Int32.Parse(userIdClaim);
            var request = new UserTokenDataRequest() { UserId = userId, Username = usernameClaim };

            return new ApiResponse<UserTokenDataRequest>
            {
                Success = true,
                Data = request
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<UserTokenDataRequest>
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

}

