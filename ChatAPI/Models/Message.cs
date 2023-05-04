using ChatAPI.Utils;
using System.Text.Json.Serialization;

namespace ChatAPI.Models
{
    public class Message
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        [JsonPropertyName("Type")]
        public string Type { get; set; }

        [JsonPropertyName("From")]
        public string From { get; set; }

        [JsonPropertyName("To")]
        public string To { get; set; }

        [JsonPropertyName("Data")]
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
