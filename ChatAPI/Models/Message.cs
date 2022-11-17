namespace ChatAPI.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string From { get; set; } = "";
        public string To { get; set; } = "";
        public string Data { get; set; } = "";
    }
}
