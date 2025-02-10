using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sharply.Client.ViewModels;

public partial class ChatWindowViewModel : ViewModelBase
{
    private readonly ChannelListViewModel _channelList;
    private readonly ApplicationServices _services;
    public ChatWindowViewModel(ApplicationServices services, ChannelListViewModel channelList)
    {
        _services = services;
        _channelList = channelList;
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
            var currentUser = _services.CurrentUserService.CurrentUser;
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
        => ChannelDisplayName = _channelList.SelectedChannel != null
            ? $"GET PARENT SERVER / {_channelList.SelectedChannel.Name}"
            : "No channel selected";

    #endregion
}
