using Microsoft.AspNetCore.Mvc;
using Sharply.Server.Data;
using Sharply.Server.Services;
using Sharply.Shared;
using Sharply.Shared.Models;

[ApiController]
[Route("api/channels")]
public class ChannelsController : ControllerBase
{
    private readonly SharplyDbContext _dbContext;
    private readonly ChannelService _channelService;
    private readonly UserService _userService;

    public ChannelsController(SharplyDbContext dbContext, ChannelService channelService, UserService userService)
    {
        _dbContext = dbContext;
        _channelService = channelService;
        _userService = userService;
    }

    [HttpGet("{channelId}/get-messages")]
    public async Task<ApiResponse<List<MessageDto>>> GetMessagesForChannel(int channelId)
    {
        try
        {
            var messages = await _channelService.GetMessagesForChannelAsync(channelId);

            return new ApiResponse<List<MessageDto>>()
            {
                Success = true,
                Data = messages
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<MessageDto>>
            {
                Success = false,
                Error = ex.Message
            };
        }
    }


}

