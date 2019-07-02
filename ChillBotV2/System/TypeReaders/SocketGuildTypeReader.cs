using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ChillBotV2.System.TypeReaders
{
    class SocketGuildTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            DiscordSocketClient client = services.GetService<DiscordSocketClient>();
            ulong guildID;
            if (ulong.TryParse(input, out guildID))
                return Task.FromResult(TypeReaderResult.FromSuccess(client.GetGuild(guildID)));

            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Could not parse SocketGuild"));
        }
    }
}
