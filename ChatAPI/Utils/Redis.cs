using StackExchange.Redis;

namespace ChatAPI.Utils
{
    public class Redis
    {
        // Return Redis server information to prove we can connect to our Redis instance
        public string TestConnection()
        {
            var connection = ConnectionMultiplexer.Connect("localhost:6379");

            // Server is frequently where more "admin" type operations are
            var server = connection.GetServer(connection.GetEndPoints().First());
            var serverInfo = server.InfoRaw();

            /* serverInfo should contain a string like:
                # Server
                redis_version:4.0.9
                redis_git_sha1:00000000
                redis_git_dirty:0
                redis_build_id:9435c3c2879311f3
                redis_mode:standalone
                ...
            */

            return serverInfo;
        }
    }
}
