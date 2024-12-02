using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;

namespace Sharply.Client.Services;
public class SignalRService
{
    private HubConnection? _connection;

    public async Task ConnectAsync(string? token)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:5001/chatHub", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
            })
            .Build();

        _connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            // Handle incoming messages
        });

        await _connection.StartAsync();
    }

    public async Task SendMessageAsync(string message)
    {
        if (_connection != null)
            await _connection.InvokeAsync("SendMessage", message);
    }
}
