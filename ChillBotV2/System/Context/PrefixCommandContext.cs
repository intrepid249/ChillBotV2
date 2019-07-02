using Discord.Commands;
using Discord.WebSocket;
using System;

namespace ChillBotV2.Context
{
    public sealed class PrefixCommandContext : SocketCommandContext
    {
        public string Prefix { get; }

        public PrefixCommandContext(string _prefix, DiscordSocketClient client, SocketUserMessage msg) : base(client, msg)
        {
            Prefix = _prefix;
        }
    }
}
