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
    }
}