using ChatAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace ChatAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        IConnectionMultiplexer _redis;
        ChatAPI.Config _config;

        public ChatController(IConnectionMultiplexer multiplexer, Config config)
        {
            this._redis = multiplexer;
            _config = config;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Message>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<Message>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages([FromQuery] string to, [FromQuery] int id = 0, [FromQuery] int qty = 1000)
        {
            var servers = _redis.GetServers();
            var keys = servers.SelectMany(server => 
                            server.Keys(pattern: $"#{to.Trim().ToUpper()}-*")
                        ).ToArray();
            List<Message?> messages = new List<Message?>();
            if (keys.Count() > 0)
            {
                var db = _redis.GetDatabase(0);
                var values = await db.StringGetAsync(keys);
                messages = values.Select(x => x.HasValue? JsonSerializer.Deserialize<Message>(x.ToString()):null)
                                    .ToList();
            }
            var res = messages.Where(x => x != null && x.Id >= id).Take(qty);
            return Ok(res);
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> PostMessage([FromBody]Message message)
        {
            var json = JsonSerializer.Serialize<Message>(message);
            var db = _redis.GetDatabase(0);
            var ok = await db.StringSetAsync($"#{message.To.ToUpper()}-{message.From.ToUpper()}-{DateTime.Now.Ticks}", json, TimeSpan.FromSeconds(_config.RedisTimeout));
            if (!ok)
                return BadRequest("error");
            return Ok("ok");
        }
    }
}
