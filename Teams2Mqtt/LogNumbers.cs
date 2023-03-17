namespace lafe.Teams2Mqtt;

public class LogNumbers
{
    public class Program
    {
        private const int BaseId = 1000;

        public const int InitializationComplete = BaseId + 1; //1001
    }

    public static class Worker
    {
        private const int BaseId = 1100;

        public const int Initializing = BaseId + 1; //1101
        public const int Initialized = BaseId + 2; //1102
        public const int StopAsyncSuccess = BaseId + 5; //1105
        public const int StopAsync = BaseId + 6; //1106
        public const int StopAsyncException = BaseId + 7; //1107
        public const int StopAsyncStoppedMqttService = BaseId + 8; //1108
        public const int StopAsyncRemovedEventHandler = BaseId + 9; //1109
        public const int StopAsyncDisconnectedTeams = BaseId + 10; //1110
        public const int StopAsyncDisposedTeams = BaseId + 11; //1111
        public const int MeetingUpdateMessageReceivedMeetingStateEmpty = BaseId + 12; //1112
        public const int MeetingUpdateMessageReceivedMeetingStateChangeTriggered = BaseId + 13; //1113
        public const int ExecuteAsyncAddedEventReceivers = BaseId + 14; //1114
        public const int OnConnectionEstablishedTeamsOnline = BaseId + 15; //1115
        public const int OnConnectionClosedTeamsOffline = BaseId + 16; //1116
    }

    public static class TeamsCommunication
    {
        private const int BaseId = 1200;

        public const int ConnectAsyncConnecting = BaseId + 1; //1201
        public const int ConnectAsyncConnected = BaseId + 2; //1202
        public const int ConnectAsync = BaseId + 3; //1203
        public const int ConnectAsyncException = BaseId + 4; //1204
        public const int ConnectAsyncCreatedBackgroundTask = BaseId + 5; //1205
        public const int WebSocketListenerRetrievedMessage = BaseId + 6; //1206
        public const int WebSocketListenerMeetingNullError = BaseId + 7; //1207
        public const int WebSocketListenerMeetingUpdateError = BaseId + 8; //1208
        public const int WebSocketListenerMeetingUpdate = BaseId + 9; //1209
        public const int WebSocketListenerWaitingUntilNextMessage = BaseId + 10; //1210
        public const int WebSocketListenerMessageReceived = BaseId + 11; //1211
        public const int WebSocketListenerConnectionClosed = BaseId + 12; //1212
        public const int WebSocketListenerReceivedTeamsMessage = BaseId + 13; //1213
        public const int WebSocketListenerMessageEnd = BaseId + 14; //1214
        public const int WebSocketListenerDeserializedMessage = BaseId + 15; //1215
        public const int WebSocketListenerWebSocketError = BaseId + 16; //1216
        public const int ReconnectTimerTick = BaseId + 17; //1217
        public const int ReconnectTimerTickBackgroundTaskNull = BaseId + 18; //1218
        public const int ReconnectTimerTickWebSocketNull = BaseId + 19; //1219
        public const int ReconnectTimerTickLogNumber = BaseId + 20; //1220
        public const int ReconnectTimerTickBackgroundTaskState = BaseId + 21; //1221
        public const int ReconnectTimerTickException = BaseId + 22; //1222
        public const int WebSocketListenerReconnectTimer = BaseId + 23; //1223
        public const int ConnectAsyncCreatedNewWebSocket = BaseId + 24; //1224
        public const int ConnectAsyncConnectionRefusedException = BaseId + 25; //1225
        public const int ConnectAsyncConnectionRefusedDetailsException = BaseId + 26; //1226
        public const int ConnectAsyncUnknownWebSocketException = BaseId + 27; //1227
        public const int ConnectAsyncInvalidRequestException = BaseId + 28; //1228
        public const int ConnectAsyncRaisedConnectionEstablishedEvent = BaseId + 29; //1229
        public const int ConnectAsyncRaisedConnectionClosedEvent = BaseId + 30; //1230
        public const int TeamsCommunicationTeamsReconnectionInterval = BaseId + 31; //1231
    }
    public static class MqttLogger
    {
        private const int BaseId = 1300;
        public const int PublishVerbose = BaseId + 1; //1301
        public const int PublishInfo = BaseId + 2; //1302
        public const int PublishWarning = BaseId + 3; //1303
        public const int PublishError = BaseId + 4; //1304
        public const int PublishUnknownLevel = BaseId + 5; //1305
    }
    public static class MqttService
    {
        private const int BaseId = 1400;
        public const int StartAsync = BaseId + 1; //1401
        public const int StartAsyncMqttServerNotConfigured = BaseId + 2; //1402
        public const int StartAsyncConnectionConfiguration = BaseId + 3; //1403
        public const int StartAsyncUsingCredentials = BaseId + 4; //1404
        public const int PublishDiscoveryMessageAsync = BaseId + 5; //1405
        public const int PublishDiscoveryMessageAsyncMqttClientNull = BaseId + 6; //1406
        public const int PublishDiscoveryMessageAsyncSuccess = BaseId + 7; //1407
        public const int PublishDiscoveryMessageAsyncException = BaseId + 8; //1408
        public const int PublishDiscoveryMessagesAsyncSensorInformationEmpty = BaseId + 9; //1409
        public const int PublishDiscoveryMessagesAsyncTopicsGenerated = BaseId + 10; //1410
        public const int PublishConfigurationAsyncPayload = BaseId + 11; //1411
        public const int PublishConfigurationAsyncConfigPublished = BaseId + 12; //1412
        public const int SendAvailabilityMessageAsync = BaseId + 13; //1413
        public const int SendAvailabilityMessageAsyncMqttClientEmpty = BaseId + 14; //1414
        public const int SendAvailabilityMessageAsyncSuccess = BaseId + 15; //1415
        public const int SendAvailabilityMessageAsyncException = BaseId + 16; //1416
        public const int RemoveDiscoveryMessageAsyncRemoveDeviceOnShutdownDisabled = BaseId + 17; //1417
        public const int RemoveDiscoveryMessageAsyncMqttClientNull = BaseId + 18; //1418
        public const int RemoveDiscoveryMessageAsyncDiscoveryMessage = BaseId + 19; //1419
        public const int RemoveDiscoveryMessageAsyncRemovedSensorConfig = BaseId + 20; //1420
        public const int RemoveDiscoveryMessageAsyncSuccess = BaseId + 21; //1421
        public const int RemoveDiscoveryMessageAsyncException = BaseId + 22; //1422
        public const int StopAsyncNoClientAvailable = BaseId + 23; //1423
        public const int StopAsyncMqttClientNotStarted = BaseId + 24; //1424
        public const int SendUpdatesAsync = BaseId + 25; //1425
        public const int SendUpdatesAsyncSensorInformationEmpty = BaseId + 26; //1426
        public const int SendUpdatesAsyncPayload = BaseId + 27; //1427
        public const int SendUpdatesAsyncUpdatePublished = BaseId + 28; //1428
        public const int SendUpdatesAsyncException = BaseId + 29; //1429
        public const int SendUpdatesAsyncSensorStateTopic = BaseId + 30; //1430
    }
}