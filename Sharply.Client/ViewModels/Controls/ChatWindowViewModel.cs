using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Services;

namespace Sharply.Client.ViewModels;

public partial class ChatWindowViewModel : ViewModelBase
{
    private readonly ApplicationServices _services;
    private readonly ChannelListViewModel _channelList;
    private readonly ServerListViewModel _serverList;

    public ChatWindowViewModel(ApplicationServices services, ChannelListViewModel channelList, ServerListViewModel serverList)
    {
        _services = services;
        _channelList = channelList;
        _serverList = serverList;
        SendMessageCommand = new AsyncRelayCommand(SendMessage);
    }

    #region Commands
    public IRelayCommand SendMessageCommand { get; }
    #endregion

    #region Properties
    [ObservableProperty]
    private string? _newMessage = string.Empty;

    [ObservableProperty]
    private string _channelDisplayName = "Unknown";

    public ObservableCollection<MessageViewModel> DisplayedMessages => _channelList.Messages;
    #endregion

    #region Methods
    private async Task SendMessage()
    {
        try
        {
            var currentUser = _services.UserService.CurrentUser;
            if (currentUser == null || _channelList.SelectedChannel == null || _channelList.SelectedChannel.Id == null || string.IsNullOrWhiteSpace(NewMessage))
                return;

            await _services.SignalRService.SendMessageAsync(_channelList.SelectedChannel.Id.Value, currentUser.Id, NewMessage);
            NewMessage = string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    public void UpdateChannelDisplay()
    {
        if (_channelList.SelectedChannel == null)
        {
            ChannelDisplayName = "No channel selected.";
            return;
        }

        var serverId = _channelList.SelectedChannel.ServerId;
        if (serverId == null) throw new InvalidOperationException("serverId was null in UpdateChannelDisplay");

        var serverName = _serverList.GetLoadedServerName(serverId.Value);
        ChannelDisplayName = $"{serverName} / {_channelList.SelectedChannel.Name}";
    }

    #endregion
}
