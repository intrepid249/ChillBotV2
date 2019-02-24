using ChillBotV2.Attributes;
using ChillBotV2.Context;
using Discord.Commands;
using System.Threading.Tasks;

namespace ChillBotV2.Modules
{
    [AdminPrefix]
    [RequireUserPermission(Discord.GuildPermission.ManageGuild)]
    public class AdminModule : ModuleBase<PrefixCommandContext>
    {
        [AdminPrefix]
        [RequireOwner]
        [Command("initInviteChannel")]
        [Summary("Used to initialise the channel that invites must be created in\nThis command can only be issued by the owner")]
        public async Task InitInviteChannel()
        {
            Global.inviteChannelID = Context.Channel.Id;
            await Context.Message.DeleteAsync();
            var m = await Context.Channel.SendMessageAsync("Initialized invite channel");
            await Task.Delay(500)
                .ContinueWith(async task =>
                {
                    await m.DeleteAsync();
                });
        }
    }
}
