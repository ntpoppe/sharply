using System.Collections.Generic;
using System.Threading.Tasks;
using Sharply.Client.ViewModels;
using Sharply.Shared.Requests;

namespace Sharply.Client.Interfaces;

public interface IChannelService
{
    Task<List<MessageViewModel>> GetMessagesForChannel(int channelId);
}

