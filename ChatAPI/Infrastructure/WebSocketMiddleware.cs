using ChatAPI.Models;
using System.Net.WebSockets;
using System.Text.Json;


namespace ChatAPI.Infrastructure
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private WebSocketHandler _webSocketHandler { get; set; }

        public WebSocketMiddleware(RequestDelegate next, WebSocketHandler webSocketHandler)
        {
            _next = next;
            _webSocketHandler = webSocketHandler;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next.Invoke(context);
                return;
            }

            string from = context.Request.Query["from"];
            string to = context.Request.Query["to"];

            WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
            await _webSocketHandler.OnConnected(socket, from);

            await Receive(socket, async (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var msg = _webSocketHandler.ReceiveString(result, buffer);

                    await HandleMessage(socket, msg);

                    return;
                }

                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await HandleDisconnect(socket);
                    return;
                }

            });
        }

        private async Task HandleDisconnect(WebSocket socket)
        {
            string disconnectedUser = await _webSocketHandler.OnDisconnected(socket);

            ServerMessage disconnectMessage = new ServerMessage(disconnectedUser, true, _webSocketHandler.GetAllUsers());

            await _webSocketHandler.BroadcastMessage(JsonSerializer.Serialize(disconnectMessage));
        }

        private async Task HandleMessage(WebSocket socket, string message)
        {
            Message clientMessage = TryDeserializeClientMessage(message);

            if (clientMessage == null)
            {
                return;
            }

            if (clientMessage.IsTypeConnection())
            {
                // For future improvements
            }
            else if (clientMessage.IsTypeChat())
            {
                string expectedUsername = _webSocketHandler.GetUsernameBySocket(socket);

                if (clientMessage.IsValid(expectedUsername))
                {
                    ServerMessage chatMessage = new ServerMessage(clientMessage);
                    await _webSocketHandler.BroadcastMessage(JsonSerializer.Serialize(chatMessage));
                }
            }
        }

        private Message TryDeserializeClientMessage(string str)
        {
            try
            {
                return JsonSerializer.Deserialize<Message>(str);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Mensaje con formato invalido.");
                return null;
            }
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                                                           cancellationToken: CancellationToken.None);

                    handleMessage(result, buffer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                await HandleDisconnect(socket);
            }
        }
    }
}
