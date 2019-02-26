using ChillBotV2.Attributes;
using ChillBotV2.Context;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChillBotV2.Modules.Moderation
{
    [AdminPrefix]
    [Group("Purge"), Remarks("admin")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    [RequireBotPermission(GuildPermission.ManageMessages)]
    [Summary("Contains commands used for bulk deletion of messages")]
    public class Purge : ModuleBase<PrefixCommandContext>
    {
        [Command]
        [Summary("Bulk delete a specified number of messages")]
        public async Task PurgeAsync([Summary("10")] int amount)
        {
            await Context.Message.DeleteAsync();

            IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync();

            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
        }

        [Command]
        [Summary("Bulk delete a specified number of messages, filtering by user")]
        public async Task PurgeAsync([Summary("10")] int amount, [Summary("@user")] IUser user)
        {
            await Context.Message.DeleteAsync();

            var messages = (await Context.Channel.GetMessagesAsync(amount).FlattenAsync()).Where(msg => msg.Author.Id == user.Id);

            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
        }

        [Command]
        [Summary("Bulk delete a specified number of messages, filtering by user(s)")]
        public async Task PurgeAsync([Summary("10")] int amount, [Summary("@user...")] params IUser[] users)
        {
            await Context.Message.DeleteAsync();

            var messages = (await Context.Channel.GetMessagesAsync(amount).FlattenAsync()).Where(msg => users.Any(u => u.Id == msg.Author.Id));

            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
        }
    }
}
