using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace ChillBotV2.Services
{
    class ModLogService
    {
        private readonly DiscordSocketClient _client;

        public ModLogService(DiscordSocketClient client)
        {
            _client = client;

            _client.MessageDeleted += LogMessageDeleted;
        }

        private Task LogMessageDeleted(Cacheable<IMessage, ulong> msg, ISocketMessageChannel channel)
        {
            if (!hasModLogChannel()) return Task.CompletedTask;

            String msgContents = msg.HasValue ? msg.Value.Content : "could not be retrieved";
            String msgAuthor = msg.HasValue ? msg.Value.Author.Mention : "could not be retrieved";

            var embed = new EmbedBuilder()
                .WithTitle("Message Deleted")
                .WithDescription($"Message: \n```{msgContents}```\nAuthor: {msgAuthor}\nChannel: #{channel.Name}")
                .WithColor(255, 0, 0).Build();
            Global.msgEventLogChannel.SendMessageAsync("", false, embed);

            return Task.CompletedTask;
        }

        private bool hasModLogChannel()
        {
            return (Global.msgEventLogChannel != null);
        }
    }
}
