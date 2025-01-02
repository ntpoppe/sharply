using AutoMapper;
using Sharply.Server.Data;
using Sharply.Server.Interfaces;
using Sharply.Server.Models;
using Sharply.Shared.Models;

namespace Sharply.Server.Services;

/// <summary>
/// Manages operations specific to messages, such as creating, deleting, or editing.
/// </summary>
public class MessageService : IMessageService
{
	private readonly ISharplyContextFactory<SharplyDbContext> _contextFactory;
	private readonly IMapper _mapper;

	public MessageService(ISharplyContextFactory<SharplyDbContext> contextFactory, IMapper mapper)
	{
		_contextFactory = contextFactory;
		_mapper = mapper;
	}

	/// <summary>
	/// Creates a message for a specific channel.
	/// </summary>
	public async Task<MessageDto> CreateMessage(int channelId, int userId, string content)
	{
		using var context = _contextFactory.CreateSharplyContext();

        var channel = await context.Channels.FindAsync(channelId);
        if (channel == null) 
			throw new Exception("Channel not found. (MessageService/CreateMessage)");

        var message = new Message
        {
            Content = content,
            UserId = userId,
            ChannelId = channel.Id,
            Timestamp = DateTime.UtcNow
        };

        context.Messages.Add(message);
        await context.SaveChangesAsync();

		return _mapper.Map<MessageDto>(message);
	}
}
