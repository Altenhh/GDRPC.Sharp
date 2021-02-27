    namespace Tsubasa.Online
{
    public enum PacketIds
    {
        Ping = 1, // CLIENT
        Pong = 2, // SERVER
        Notification = 3, // SERVER
        Disconnection = 4, // SERVER
        Login = 5, // CLIENT
        SwitchAction = 6, // CLIENT
    }
}