using ChillBotV2.Attributes;
using ChillBotV2.Context;
using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace ChillBotV2.Modules
{
    [AdminPrefix]
    [Remarks("admin, user")]
    [Summary("Commands that provide utility to members of the server")]
    public class Utilities : ModuleBase<PrefixCommandContext>
    {
        //private readonly CommandService _commands;

        //public Utilities(CommandService commands) => _commands = commands;

        [AdminPrefix]
        [Command("invite")]
        [RequireUserPermission(GuildPermission.CreateInstantInvite)]
        [RequireBotPermission(GuildPermission.CreateInstantInvite)]
        public async Task GetInviteCode(IUser user)
        {
            var server_invite = (await Context.Guild.GetInvitesAsync()).Where(inv => inv.ChannelId == Global.inviteChannelID).FirstOrDefault();

            await Context.Message.DeleteAsync();

            if (server_invite == null)
            {
                var m = await Context.Channel.SendMessageAsync($"{Context.User.Mention} check your DMs");
                await Task.Delay(1500);
                await m.DeleteAsync();
                var errChannel = await Context.Message.Author.GetOrCreateDMChannelAsync();
                //if (Global.inviteChannelID == null)
                //    await errChannel.SendMessageAsync($"The invite channel has not been initialised. Please call {_commands.Commands.Where(cmd => cmd.Name.Equals("initInviteChannel")).First()}");
                //else
                    await errChannel.SendMessageAsync($"No invite exists. Invites must be created to the channel `#{Context.Guild.Channels.Where(c => c.Id == Global.inviteChannelID).First()}`");

                return;
            }

            var channel = await user.GetOrCreateDMChannelAsync();
            if (server_invite != null)
                await channel.SendMessageAsync(server_invite.Url);

            return;
        }
    }
}
