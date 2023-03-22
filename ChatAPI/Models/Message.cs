using ChatAPI.Utils;

namespace ChatAPI.Models
{
    public class Message
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Data { get; set; }

        public bool IsValid(string expectedUsername)
        {
            if (this.IsTypeConnection())
            {
                if (this.From == string.Empty)
                {
                    return false;
                }
            }
            else if (this.IsTypeChat())
            {
                if (this.From != expectedUsername || this.Data == string.Empty)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public string BuildChatMessageBody()
        {
            string receiver = this.To == string.Empty ? "TODOS" : this.To;

            return $"De {this.From} a {receiver}: {this.Data}";
        }

        public string GetMessageType()
        {
            return this.Type.ToUpper();
        }

        public bool IsTypeConnection()
        {
            return this.GetMessageType() == MessageType.CONNECTION.ToString();
        }

        public bool IsTypeChat()
        {
            return this.GetMessageType() == MessageType.CHAT.ToString();
        }
    }
}
