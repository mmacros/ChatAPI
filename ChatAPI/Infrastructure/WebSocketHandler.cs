using ChatAPI.Models;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;


namespace ChatAPI.Infrastructure
{
    public abstract class WebSocketHandler
    {
        protected ConnectionManager ConnectionManager { get; set; }

        public WebSocketHandler(ConnectionManager connectionManager)
        {
            ConnectionManager = connectionManager;
        }

        public virtual async Task OnConnected(WebSocket socket, string username)
        {
            string connectionError = ValidateUsername(username);

            if (!string.IsNullOrEmpty(connectionError))
            {
                await ConnectionManager.RemoveSocket(socket, connectionError);
            }
            else
            {
                ConnectionManager.AddSocket(socket);
                ConnectionManager.AddUser(socket, username);

                ServerMessage connectMessage = new ServerMessage(username, false, GetAllUsers());
                await BroadcastMessage(JsonSerializer.Serialize(connectMessage));
            }
        }

        public string ValidateUsername(string username)
        {
            if (String.IsNullOrEmpty(username))
            {
                return $"El usuario no puede ser vacío";
            }

            if (ConnectionManager.UsernameAlreadyExists(username))
            {
                return $"El usuario {username} ya existe.";
            }

            return null;
        }

        public virtual async Task<string> OnDisconnected(WebSocket socket)
        {
            string socketId = ConnectionManager.GetId(socket);
            await ConnectionManager.RemoveSocket(socket);

            string username = ConnectionManager.GetUsernameBySocketId(socketId);
            ConnectionManager.RemoveUser(username);

            return username;
        }

        public async Task SendMessageAsync(WebSocket socket, string message)
        {
            if (socket.State != WebSocketState.Open)
                return;

            await socket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(message),
                                                                  offset: 0,
                                                                  count: message.Length),
                                   messageType: WebSocketMessageType.Text,
                                   endOfMessage: true,
                                   cancellationToken: CancellationToken.None);
        }

        public async Task SendMessageAsync(string socketId, string message)
        {
            await SendMessageAsync(ConnectionManager.GetSocketById(socketId), message);
        }

        public async Task SendMessageToAllAsync(string message)
        {
            foreach (var pair in ConnectionManager.GetAllSockets())
            {
                if (pair.Value.State == WebSocketState.Open)
                    await SendMessageAsync(pair.Value, message);
            }
        }

        public async Task SendBinaryToAsync(string user, byte[] message)
        {
            var websocket = ConnectionManager.GetSocketByUsername(user);
            if (websocket != null && websocket.State == WebSocketState.Open)
                await websocket.SendAsync(buffer: new ArraySegment<byte>(array: message,
                                                      offset: 0,
                                                      count: message.Length),
                       messageType: WebSocketMessageType.Binary,
                       endOfMessage: true,
                       cancellationToken: CancellationToken.None);
        }

        public async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            await SendMessageToAllAsync(message);
        }

        public string ReceiveString(WebSocketReceiveResult result, byte[] buffer)
        {
            return Encoding.UTF8.GetString(buffer, 0, result.Count);
        }

        public async Task BroadcastMessage(string message)
        {
            await SendMessageToAllAsync(message);
        }

        
        public List<string> GetAllUsers()
        {
            return ConnectionManager.GetAllUsernames();
        }

        public string GetUsernameBySocket(WebSocket socket)
        {
            return ConnectionManager.GetUsernameBySocket(socket);
        }
    }
}
