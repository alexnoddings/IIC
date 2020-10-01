using System.Collections.Generic;

namespace ImmersiveInCharacterChat.Config
{
    public class BotConfig
    {
        public string Token { get; set; }
        public ulong ChannelId { get; set; }
        public string Webhook { get; set; }

        public IDictionary<string, PersonInfo> People { get; set; }
    }
}
