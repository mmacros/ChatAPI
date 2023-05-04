using ChatAPI.Models;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;


namespace ChatAPI.Infrastructure
{
    public class WebSocketMiddleware
    {
        private const int BUFFER_SIZE = 1024 * 32; //32 KB

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

                    await HandleTextMessage(socket, msg);

                    return;
                }
                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    await HandleBinaryMessage(socket, buffer);

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

        private async Task HandleTextMessage(WebSocket socket, string message)
        {
            Message clientMessage = TryDeserializeTextClientMessage(message);

            if (clientMessage == null)
            {
                return;
            }

            if (clientMessage.IsTypeConnection())
            {
                // TODO:
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

        private async Task HandleBinaryMessage(WebSocket socket, byte[] message)
        {
            if (message != null && message.Length > 6)
            {
                // Extract from to data
                var segments = TryDeserializeBinaryClientMessage(message);
                 
                var from = "";
                var to = "";

                if (segments == null || segments.Count() < 3)
                {
                    return;
                }
                var messageSegments = segments.ToList();

                from = System.Text.Encoding.UTF8.GetString(messageSegments[1]);
                to = System.Text.Encoding.UTF8.GetString(messageSegments[2]);

                await _webSocketHandler.SendBinaryToAsync(to, message);
            }
        }

        private Message TryDeserializeTextClientMessage(string str)
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

        private static IEnumerable<Byte[]> TryDeserializeBinaryClientMessage(byte[] source)
        {
            var marker = Encoding.UTF8.GetBytes("@*");

            if (null == source)
                throw new ArgumentNullException("source");

            return SplitByteArray(source, marker );
        }


        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[BUFFER_SIZE];

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

        private static List<byte[]> SplitByteArray(byte[] data, byte[] delimiter)
        {
            List<byte[]> result = new List<byte[]>();
            int start = 0;

            for (int i = 0; i < data.Length - delimiter.Length + 1; i++)
            {
                bool isMatch = true;

                for (int j = 0; j < delimiter.Length; j++)
                {
                    if (data[i + j] != delimiter[j])
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch)
                {
                    byte[] segment = new byte[i - start];
                    Array.Copy(data, start, segment, 0, i - start);
                    result.Add(segment);

                    start = i + delimiter.Length;
                    i += delimiter.Length - 1;
                }
            }

            // Agrega el último segmento
            byte[] lastSegment = new byte[data.Length - start];
            Array.Copy(data, start, lastSegment, 0, data.Length - start);
            result.Add(lastSegment);

            return result;
        }
    }
}
