using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using lafe.Teams2Mqtt.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using lafe.Teams2Mqtt.Model.Teams;

namespace lafe.Teams2Mqtt.Services;

public class TeamsCommunication : IDisposable
{
    protected ILogger<TeamsCommunication> Logger { get; }
    protected AppConfiguration Configuration { get; }

    protected ClientWebSocket WebSocket { get; }

    protected Task? ListenerBackgroundTask { get; set; }

    public TeamsCommunication(ILogger<TeamsCommunication> logger, IOptions<AppConfiguration> configuration)
    {
        Logger = logger;
        Configuration = configuration.Value;

        WebSocket = new ClientWebSocket();
    }

    protected string BuildWebSocketUrl()
    {
        var deviceName = System.Environment.MachineName ?? string.Empty;
        var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
        var appName = assemblyName?.Name ?? string.Empty;
        var appVersion = assemblyName?.Version?.ToString() ?? string.Empty;

        return $"ws://{Configuration.TeamsWebSocketAddress}:8124?token={Configuration.TeamsApiKey}&protocol-version=1.0.0&manufacturer=lafe&device={deviceName}&app={appName}&app-version={appVersion}";
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        using var scope = Logger.BeginScope($"{nameof(TeamsCommunication)}:{nameof(ConnectAsync)}");
        try
        {
            Logger.LogTrace(LogNumbers.TeamsCommunication.ConnectAsync, $"Connecting to Teams");
            var webSocketUrl = BuildWebSocketUrl();
            Logger.LogDebug(LogNumbers.TeamsCommunication.ConnectAsyncConnecting, $"Connecting to Teams with URL \"{webSocketUrl}\"");
            await WebSocket.ConnectAsync(new Uri(webSocketUrl), cancellationToken);
            Logger.LogTrace(LogNumbers.TeamsCommunication.ConnectAsyncConnected, $"Completed connection to Teams");

            ListenerBackgroundTask = Task.Run(WebSocketListener, cancellationToken);
            Logger.LogTrace(LogNumbers.TeamsCommunication.ConnectAsyncCreatedBackgroundTask, $"Created background task");
        }
        catch (Exception ex)
        {
            Logger.LogError(LogNumbers.TeamsCommunication.ConnectAsyncException, ex, $"An error occurred while trying to connect to Teams: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Background task that listens to responses from the Teams Web Socket
    /// </summary>
    private async Task WebSocketListener()
    {
        // assuming this is enough for messages to fit into a single frame
        var buffer = new byte[5 * 1024];
        while (WebSocket.State == WebSocketState.Open)
        {
            // Waits until the next message from the Teams client arrives
            var result = await WebSocket.ReceiveAsync(buffer, CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
            else
            {
                var serializedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Logger.LogTrace(LogNumbers.TeamsCommunication.WebSocketListenerRetrievedMessage, $"Retrieved message: {serializedMessage}");
                if (result.EndOfMessage)
                {
                    var meetingUpdate = JsonSerializer.Deserialize<MeetingUpdateMessage>(serializedMessage);
                    var meetingState = meetingUpdate?.MeetingUpdate?.MeetingState;
                    if (!string.IsNullOrWhiteSpace(meetingUpdate?.ErrorMessage))
                    {
                        Logger.LogError(LogNumbers.TeamsCommunication.WebSocketListenerMeetingUpdateError, $"An error was received by the Web Socket connection from Teams: {meetingUpdate.ErrorMessage}");
                    }
                    else if (meetingState != null)
                    {
                        Logger.LogInformation(LogNumbers.TeamsCommunication.WebSocketListenerMeetingUpdate, $"Received meeting update:\r\nIsInMeeting: {meetingState.IsInMeeting}\r\nIsCameraOn: {meetingState.IsCameraOn}\r\nIsMuted: {meetingState.IsMuted}\r\nIsHandRaised: {meetingState.IsHandRaised}");
                        
                    }
                    else
                    {
                        Logger.LogError(LogNumbers.TeamsCommunication.WebSocketListenerMeetingStateNullError, $"The received meeting state was empty and could not be processed: {serializedMessage}");
                    }
                }
            }
        }
    }

    public async Task DisconnectAsync()
    {
        await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Shutting down application", CancellationToken.None);
    }

    public void Dispose()
    {
        WebSocket?.Dispose();
        ListenerBackgroundTask?.Dispose();
    }
}