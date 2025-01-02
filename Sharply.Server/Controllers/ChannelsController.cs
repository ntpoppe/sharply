using Microsoft.AspNetCore.Mvc;
using Sharply.Server.Interfaces;
using Sharply.Shared;
using Sharply.Shared.Models;

[ApiController]
[Route("api/channels")]
public class ChannelsController : ControllerBase
{
    private readonly IChannelService _channelService;

    public ChannelsController(IChannelService channelService)
    {
        _channelService = channelService;
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

    [HttpGet("{channelId}/{userId}/is-user-accessible")]
    public async Task<ApiResponse<bool>> CheckUserChannelAccess(int channelId, int userId)
    {
        try
        {
            var result = await _channelService.CheckUserChannelAccessAsync(channelId, userId);

            return new ApiResponse<bool>()
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>()
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
}

