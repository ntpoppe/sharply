using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Sharply.Client.Interfaces;

namespace Sharply.Client.ViewModels;

public partial class ChannelListViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private readonly ISignalRService _signalRService;
    private readonly ITokenStorageService _tokenStorageService;
    private readonly IChannelService _channelService;

    public ChannelListViewModel(IApiService apiService, ISignalRService signalRService, ITokenStorageService tokenStorageService, IChannelService channelService)
    {
        _apiService = apiService;
        _signalRService = signalRService;
        _tokenStorageService = tokenStorageService;
        _channelService = channelService;
    }

    [ObservableProperty]
    public ObservableCollection<ChannelViewModel> _channels = new();

    [ObservableProperty]
    public ChannelViewModel? _selectedChannel;

    [ObservableProperty]
    public ObservableCollection<MessageViewModel> _messages = new();

    public void LoadChannelsAsync(ServerViewModel server)
    {
        Channels = new ObservableCollection<ChannelViewModel>(server.Channels);
        SelectedChannel = server.Channels.First();
    }

    partial void OnSelectedChannelChanged(ChannelViewModel? oldValue, ChannelViewModel? newValue)
    {
        _ = OnSelectedChannelChangedAsync(oldValue, newValue);
    }

    private async Task OnSelectedChannelChangedAsync(ChannelViewModel? oldValue, ChannelViewModel? newValue)
    {
        try
        {
            if (oldValue?.Id != null)
                await _signalRService.LeaveChannelAsync(oldValue.Id.Value);

            if (newValue?.Id != null)
            {
                await _signalRService.JoinChannelAsync(newValue.Id.Value);
                Messages.Clear();

                var fetchedMessages = await _channelService.GetMessagesForChannel(newValue.Id.Value);
                if (fetchedMessages != null)
                {
                    var combinedMessages = new HashSet<MessageViewModel>(newValue.Messages);

                    foreach (var fetchedMessage in fetchedMessages)
                    {
                        if (!combinedMessages.Any(m => m.Id == fetchedMessage.Id))
                            combinedMessages.Add(fetchedMessage);
                    }

                    foreach (var message in combinedMessages.OrderBy(m => m.Timestamp))
                    {
                        Messages.Add(message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("something happened in selectedchannelchanged", ex);
        }
    }

    public void OnMessageReceived(string user, string message, DateTime timestamp)
    {
        if (SelectedChannel == null || Messages == null)
            return;

        var newMessage = new MessageViewModel()
        {
            Username = user,
            Content = message,
            Timestamp = timestamp
        };

        Messages.Add(newMessage);
        SelectedChannel.Messages.Add(newMessage);
    }
}

