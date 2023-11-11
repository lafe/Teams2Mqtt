using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using lafe.Teams2Mqtt.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using lafe.Teams2Mqtt.Model.Teams;

namespace lafe.Teams2Mqtt.Services;

/// <summary>
/// Contains the communication with the Teams client
/// </summary>
/// <remarks>
/// Inspired by the sample from Philipp Bauknecht: https://github.com/GrillPhil/TeamsClientApiSample/tree/main
/// </remarks>
public class TeamsCommunication : IDisposable
{
    protected ILogger<TeamsCommunication> Logger { get; }
    protected AppConfiguration Configuration { get; }

    /// <summary>
    /// The <see cref="ClientWebSocket"/> used to connect with Teams
    /// </summary>
    protected ClientWebSocket? WebSocket { get; set; }

    /// <summary>
    /// The task that listens to Teams messaged sent through the <see cref="WebSocket"/>
    /// </summary>
    protected Task? ListenerBackgroundTask { get; set; }

    /// <summary>
    /// Timer that is used to connect to Teams, when the <see cref="WebSocket"/> is closed
    /// </summary>
    protected Timer? ReconnectTimer { get; set; }

    public delegate void MeetingUpdateMessageReceivedHandler(object sender, MeetingUpdateMessage e);
    /// <summary>
    /// Is executed when a new meeting update message has been received from Teams
    /// </summary>
    public event MeetingUpdateMessageReceivedHandler? MeetingUpdateMessageReceived;

    /// <summary>
    /// Is executed when the connection with Teams has been established
    /// </summary>
    public event EventHandler? ConnectionEstablished;
    /// <summary>
    /// Is executed when the connection with Teams has been closed
    /// </summary>
    public event EventHandler? ConnectionClosed;

    /// <summary>
    /// Contains the latest received state from Teams. If no state has yet been received, it will be <c>null</c>.
    /// </summary>
    public MeetingUpdateMessage? CurrentTeamsState { get; private set; }


    public TeamsCommunication(ILogger<TeamsCommunication> logger, IOptions<AppConfiguration> configuration)
    {
        Logger = logger;
        Configuration = configuration.Value;

        Logger.LogInformation(LogNumbers.TeamsCommunication.TeamsCommunicationTeamsReconnectionInterval, $"Using a reconnection interval of {Configuration.TeamsReconnectInterval}s.");
        ReconnectTimer = new Timer(ReconnectTimerTick, null, TimeSpan.FromSeconds(Configuration.TeamsReconnectInterval), TimeSpan.FromSeconds(Configuration.TeamsReconnectInterval));
    }

    private void ReconnectTimerTick(object? state)
    {
        using var scope = Logger.BeginScope($"{nameof(TeamsCommunication)}:{nameof(ConnectAsync)}");
        try
        {
            // Stop timer
            ReconnectTimer?.Change(Timeout.Infinite, Timeout.Infinite);


            if (WebSocket?.State != WebSocketState.Closed)
            {
                return;
            }

            Logger.LogTrace(LogNumbers.TeamsCommunication.ReconnectTimerTickLogNumber, $"Web Socket is in state {WebSocket?.State}. Requiring reconnect.");

            ListenerBackgroundTask?.Dispose();
#pragma warning disable CS4014
            // Allow "Fire and Forget" async call
            ConnectAsync();
#pragma warning restore CS4014
        }
        catch (Exception ex)
        {
            Logger.LogError(LogNumbers.TeamsCommunication.ReconnectTimerTickException, ex, $"An error occurred while executing the Reconnect Timer handler: {ex}");
        }
        finally
        {
            // Resume timer
            ReconnectTimer?.Change(TimeSpan.FromSeconds(Configuration.TeamsReconnectInterval), TimeSpan.FromSeconds(Configuration.TeamsReconnectInterval));
        }
    }

    protected string BuildWebSocketUrl()
    {
        var deviceName = System.Environment.MachineName ?? string.Empty;
        var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
        var appName = assemblyName?.Name ?? string.Empty;
        var appVersion = assemblyName?.Version?.ToString() ?? string.Empty;

        var url = $"ws://{Configuration.TeamsWebSocketAddress}:{Configuration.TeamsWebSocketPort}?";
        if (!string.IsNullOrWhiteSpace(Configuration.TeamsApiKey))
        {
            url = $"{url}token={Configuration.TeamsApiKey}&";
        }

        return $"{url}protocol-version=2.0.0&manufacturer=lafe&device={deviceName}&app={appName}&app-version={appVersion}";
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        using var scope = Logger.BeginScope($"{nameof(TeamsCommunication)}:{nameof(ConnectAsync)}");
        try
        {
            Logger.LogTrace(LogNumbers.TeamsCommunication.ConnectAsync, $"Connecting to Teams");
            WebSocket?.Dispose();
            WebSocket = new ClientWebSocket(); // We do have to create a new connection every time, because it gets disposed when an error occurs
            Logger.LogTrace(LogNumbers.TeamsCommunication.ConnectAsyncCreatedNewWebSocket, $"Created new Web Socket object for connection with Teams");
            var webSocketUrl = BuildWebSocketUrl();
            Logger.LogDebug(LogNumbers.TeamsCommunication.ConnectAsyncConnecting, $"Connecting to Teams with URL \"{webSocketUrl}\"");
            await WebSocket.ConnectAsync(new Uri(webSocketUrl), cancellationToken);
            Logger.LogTrace(LogNumbers.TeamsCommunication.ConnectAsyncConnected, $"Completed connection to Teams");

            ConnectionEstablished?.Invoke(this, EventArgs.Empty);
            Logger.LogTrace(LogNumbers.TeamsCommunication.ConnectAsyncRaisedConnectionEstablishedEvent, $"Raised ConnectionEstablished event");

            ListenerBackgroundTask = Task.Run(WebSocketListener, cancellationToken);
            Logger.LogTrace(LogNumbers.TeamsCommunication.ConnectAsyncCreatedBackgroundTask, $"Created background task");

            await RequestMeetingStatusAsync(cancellationToken);
        }
        catch (WebSocketException ex)
        {
            if (ex.WebSocketErrorCode == WebSocketError.Faulted)
            {
                Logger.LogWarning(LogNumbers.TeamsCommunication.ConnectAsyncConnectionRefusedException, ex, $"A connection with Teams could not be established, because the connection has been refused (\"{ex.Message}\"). Is Teams running and is the API key correct?");
                Logger.LogDebug(LogNumbers.TeamsCommunication.ConnectAsyncConnectionRefusedDetailsException, ex, $"Details of the refused connection: {ex}");
            }
            else if (ex.WebSocketErrorCode == WebSocketError.NotAWebSocket)
            {
                Logger.LogError(LogNumbers.TeamsCommunication.ConnectAsyncInvalidRequestException, ex, $"A connection with Teams could not be established, because the connection request was invalid: {ex.Message}. Is Teams running and is the API key correct?");
            }
            else
            {
                Logger.LogError(LogNumbers.TeamsCommunication.ConnectAsyncUnknownWebSocketException, ex, $"An unhandled WebSocketException error ({ex.WebSocketErrorCode}) occurred while trying to connect to Teams: {ex}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(LogNumbers.TeamsCommunication.ConnectAsyncException, ex, $"An error occurred while trying to connect to Teams: {ex}");
        }
    }

    /// <summary>
    /// Background task that listens to responses from the Teams Web Socket
    /// </summary>
    private async Task WebSocketListener()
    {
        var buffer = new byte[1024];
        var serializedMessage = string.Empty;
        while (WebSocket?.State == WebSocketState.Open)
        {
            try
            {
                Logger.LogTrace(LogNumbers.TeamsCommunication.WebSocketListenerWaitingUntilNextMessage, $"Waiting until the next message or chunk of a message arrives");
                // Waits until the next message from the Teams client arrives
                var result = await WebSocket.ReceiveAsync(buffer, CancellationToken.None);
                Logger.LogTrace(LogNumbers.TeamsCommunication.WebSocketListenerMessageReceived, $"Retrieved message or chunk of a message");
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Logger.LogInformation(LogNumbers.TeamsCommunication.WebSocketListenerConnectionClosed, $"Teams has closed the connection. Closing the web socket.");
                    if (WebSocket.State == WebSocketState.Open || WebSocket.State == WebSocketState.CloseReceived)
                    {
                        await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                    }

                    ConnectionClosed?.Invoke(this, EventArgs.Empty);
                    Logger.LogTrace(LogNumbers.TeamsCommunication.ConnectAsyncRaisedConnectionClosedEvent, $"Raised ConnectionClosed event");
                }
                else
                {
                    Logger.LogTrace(LogNumbers.TeamsCommunication.WebSocketListenerReceivedTeamsMessage, $"Received message of type {result.MessageType}");
                    var messageChunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Logger.LogTrace(LogNumbers.TeamsCommunication.WebSocketListenerRetrievedMessage, $"Retrieved new message chunk: {messageChunk}");
                    if (result.EndOfMessage)
                    {
                        serializedMessage += messageChunk;
                        Logger.LogTrace(LogNumbers.TeamsCommunication.WebSocketListenerMessageEnd, $"Complete message has been received: {serializedMessage}");

                        var meetingUpdate = JsonSerializer.Deserialize<MeetingUpdateMessage>(serializedMessage);
                        Logger.LogTrace(LogNumbers.TeamsCommunication.WebSocketListenerDeserializedMessage, $"Deserialized the message");
                        if (!string.IsNullOrWhiteSpace(meetingUpdate?.ErrorMessage))
                        {
                            Logger.LogError(LogNumbers.TeamsCommunication.WebSocketListenerMeetingUpdateError, $"An error was received by the Web Socket connection from Teams: {meetingUpdate.ErrorMessage}");
                        }
                        else if (meetingUpdate != null)
                        {
                            Logger.LogTrace(LogNumbers.TeamsCommunication.WebSocketListenerMeetingUpdate, $"Message contains data and is not an error. Raising new Meeting Update received event.");
                            MeetingUpdateMessageReceived?.Invoke(this, meetingUpdate);
                            CurrentTeamsState = meetingUpdate;
                        }
                        else
                        {
                            Logger.LogError(LogNumbers.TeamsCommunication.WebSocketListenerMeetingNullError, $"The received meeting update was null");
                        }

                        serializedMessage = string.Empty;
                    }
                    else
                    {
                        serializedMessage += messageChunk;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(LogNumbers.TeamsCommunication.WebSocketListenerWebSocketError, $"An error occurred while processing web socket messages: {ex}");
            }
        }
    }

    /// <summary>
    /// Sends a request to Teams to retrieve the current Meeting Status
    /// </summary>
    public async Task RequestMeetingStatusAsync(CancellationToken cancellationToken = default)
    {
        var queryMeetingAction = new TeamsAction("query-meeting-state", "query-meeting-state");
        await SendRequestAsync(queryMeetingAction, cancellationToken);
    }

    /// <summary>
    /// Sends a request to Teams to toggle the mute state
    /// </summary>
    public async Task ToggleMuteAsync(CancellationToken cancellationToken = default)
    {
        if (!(CurrentTeamsState?.MeetingUpdate?.MeetingPermissions?.CanToggleMute ?? false))
        {
            Logger.LogWarning(LogNumbers.TeamsCommunication.ToggleMuteAsyncPermissionsInsufficient, $"The current permissions of the Teams client do not allow to mute the call.");
            await RequestMeetingStatusAsync(cancellationToken);
            return;
        }
        var queryMeetingAction = new TeamsAction("toggle-mute", "toggle-mute");
        await SendRequestAsync(queryMeetingAction, cancellationToken);
    }

    /// <summary>
    /// Sends a request to Teams to toggle the video camera state
    /// </summary>
    public async Task ToggleVideoAsync(CancellationToken cancellationToken = default)
    {
        if (!(CurrentTeamsState?.MeetingUpdate?.MeetingPermissions?.CanToggleVideo ?? false))
        {
            Logger.LogWarning(LogNumbers.TeamsCommunication.ToggleVideoAsyncPermissionsInsufficient, $"The current permissions of the Teams client do not allow to disable the video camera.");
            await RequestMeetingStatusAsync(cancellationToken);
            return;
        }
        var queryMeetingAction = new TeamsAction("toggle-video", "toggle-video");
        await SendRequestAsync(queryMeetingAction, cancellationToken);
    }

    /// <summary>
    /// Sends a request to Teams to toggle the "raised hand" state
    /// </summary>
    public async Task ToggleRaisedHandAsync(CancellationToken cancellationToken = default)
    {
        if (!(CurrentTeamsState?.MeetingUpdate?.MeetingPermissions?.CanToggleHand ?? false))
        {
            Logger.LogWarning(LogNumbers.TeamsCommunication.ToggleRaisedHandAsyncPermissionsInsufficient, $"The current permissions of the Teams client do not allow to raise the hand.");
            await RequestMeetingStatusAsync(cancellationToken);
            return;
        }
        var queryMeetingAction = new TeamsAction("raise-hand", "toggle-hand");
        await SendRequestAsync(queryMeetingAction, cancellationToken);
    }

    /// <summary>
    /// Sends a request to Teams to toggle the "background blur" state
    /// </summary>
    public async Task ToggleBackgroundBlurAsync(CancellationToken cancellationToken = default)
    {
        if (!(CurrentTeamsState?.MeetingUpdate?.MeetingPermissions?.CanToggleVideo ?? false))
        {
            Logger.LogWarning(LogNumbers.TeamsCommunication.ToggleRaisedHandAsyncPermissionsInsufficient, $"The current permissions of the Teams client do not allow to toggle background blur.");
            await RequestMeetingStatusAsync(cancellationToken);
            return;
        }
        var queryMeetingAction = new TeamsAction("background-blur", "toggle-background-blur");
        await SendRequestAsync(queryMeetingAction, cancellationToken);
    }

    /// <summary>
    /// Sends a request to Teams to leave the current call
    /// </summary>
    public async Task LeaveCallAsync(CancellationToken cancellationToken = default)
    {
        var queryMeetingAction = new TeamsAction("call", "leave-call");
        await SendRequestAsync(queryMeetingAction, cancellationToken);
    }

    /// <summary>
    /// Sends a request to Teams to react with applause
    /// </summary>
    public async Task ReactWithApplauseAsync(CancellationToken cancellationToken = default)
    {
        var queryMeetingAction = new TeamsAction("call", "react-applause");
        await SendRequestAsync(queryMeetingAction, cancellationToken);
    }

    /// <summary>
    /// Sends a request to Teams to react with a laugh
    /// </summary>
    public async Task ReactWithLaughAsync(CancellationToken cancellationToken = default)
    {
        var queryMeetingAction = new TeamsAction("call", "react-laugh");
        await SendRequestAsync(queryMeetingAction, cancellationToken);
    }

    /// <summary>
    /// Sends a request to Teams to react with a like
    /// </summary>
    public async Task ReactWithLikeAsync(CancellationToken cancellationToken = default)
    {
        var queryMeetingAction = new TeamsAction("call", "react-like");
        await SendRequestAsync(queryMeetingAction, cancellationToken);
    }

    /// <summary>
    /// Sends a request to Teams to react with love
    /// </summary>
    public async Task ReactWithLoveAsync(CancellationToken cancellationToken = default)
    {
        var queryMeetingAction = new TeamsAction("call", "react-love");
        await SendRequestAsync(queryMeetingAction, cancellationToken);
    }

    /// <summary>
    /// Sends a request to Teams to react with wow
    /// </summary>
    public async Task ReactWithWowAsync(CancellationToken cancellationToken = default)
    {
        var queryMeetingAction = new TeamsAction("call", "react-wow");
        await SendRequestAsync(queryMeetingAction, cancellationToken);
    }

    /// <summary>
    /// Sends a given <paramref name="action"/> to Teams
    /// </summary>
    /// <param name="action">The <see cref="TeamsAction"/> that defines the message that should be sent</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can abort the async operation</param>
    protected async Task SendRequestAsync(TeamsAction action, CancellationToken cancellationToken = default)
    {
        using var scope = Logger.BeginScope($"{nameof(TeamsCommunication)}:{nameof(ConnectAsync)}");
        try
        {
            Logger.LogTrace(LogNumbers.TeamsCommunication.SendRequestAsync, $"Sending action \"{action.Action}\" of service \"{action.Service}\" to Teams");

            if (WebSocket == null || WebSocket.State != WebSocketState.Open)
            {
                Logger.LogWarning(LogNumbers.TeamsCommunication.SendRequestAsyncWebSocketConnectionMissing, $"Web Socket connection is missing or not open. Cannot send action");
                return;
            }

            var jsonAction = JsonSerializer.Serialize(action);
            Logger.LogTrace(LogNumbers.TeamsCommunication.SendRequestAsyncSerializedContent, $"Serialized given {nameof(TeamsAction)} to string: {jsonAction}");

            var jsonActionBytes = Encoding.UTF8.GetBytes(jsonAction);
            await WebSocket.SendAsync(jsonActionBytes, WebSocketMessageType.Text, true, cancellationToken);

            Logger.LogTrace(LogNumbers.TeamsCommunication.SendRequestAsyncSuccess, $"Sent action \"{action.Action}\" of service \"{action.Service}\" to Teams");
        }
        catch (Exception ex)
        {
            Logger.LogError(LogNumbers.TeamsCommunication.SendRequestAsyncException, ex, $"An error occurred while sending action \"{action.Action}\" of service \"{action.Service}\" to Teams: {ex}");
        }

    }


    public async Task DisconnectAsync()
    {
        if (WebSocket == null)
        {
            return;
        }
        await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Shutting down application", CancellationToken.None);
    }

    public void Dispose()
    {
        WebSocket?.Dispose();
        ListenerBackgroundTask?.Dispose();
    }
}