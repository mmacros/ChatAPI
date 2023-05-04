using ChatAPI.Utils;

namespace ChatAPI.Models
{
    public class ServerMessage
    {
        public ServerMessage()
        {
            Users = new List<string>();
        }

        public ServerMessage(String messageType, string messageContent, List<string> users)
        {
            Type = messageType.ToString();
            Data = messageContent;
            Users = users;
        }

        public ServerMessage(Message clientMessage)
        {
            Type = clientMessage.GetMessageType();
            Data = clientMessage.Data; //clientMessage.BuildChatMessageBody();
            Id = clientMessage.Id;
            From = clientMessage.From;
            To = clientMessage.To;

        }

        public ServerMessage(string username, bool isDisconnect, List<string> users)
        {
            Type = MessageType.CONNECTION.ToString();
            Data = this.BuildConnectionMessageBody(username, isDisconnect);
            Users = users;
        }

        public string Type { get; set; }
        public string Data { get; set; }
        public List<string> Users { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public int Id { get; set; }
        private string BuildConnectionMessageBody(string username, bool isDisconnect)
        {
            if (isDisconnect)
            {
                return $"{username} se a desconectado.";
            }
            else
            {
                return $"{username} se a conectado.";
            }
        }
    }
}
