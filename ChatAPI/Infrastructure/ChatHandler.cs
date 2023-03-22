namespace ChatAPI.Infrastructure
{
    public class ChatHandler : WebSocketHandler
    {
        public ChatHandler(ConnectionManager connectionManager) : base(connectionManager)
        {
        }
    }
}
