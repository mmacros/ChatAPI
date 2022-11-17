namespace ChatAPI
{
    public class Config
    {
        public string RedisHost { get; set; } = "";
        public string RedisPort { get; set; } = "";
        public string RedisUser { get; set; } = "";
        public string RedisPass { get; set; } = "";
        public string RedisConnectionString { get; set; } = "";
        public int RedisTimeout { get; set; } = 600;

    }
}
