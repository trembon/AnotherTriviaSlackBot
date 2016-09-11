using SlackAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotherTriviaSlackBot.Extensions
{
    public static class SlackSocketClientExtensions
    {
        public static string GetChannelID(this SlackSocketClient client, string channelName)
        {
            var channel = client.Channels.FirstOrDefault(c => c.name.Equals(channelName, StringComparison.InvariantCultureIgnoreCase));
            return channel?.id;
        }
    }
}
