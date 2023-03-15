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
        public const int CalculatedUpdateInterval = BaseId + 3; //1103
        public const int ExecuteAsyncLoopError = BaseId + 4; //1104
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
        public const int WebSocketListenerMeetingStateNullError = BaseId + 7; //1207
        public const int WebSocketListenerMeetingUpdateError = BaseId + 8; //1208
        public const int WebSocketListenerMeetingUpdate = BaseId + 9; //1209
    }
}