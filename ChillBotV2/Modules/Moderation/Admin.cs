using ChillBotV2.Attributes;
using ChillBotV2.Context;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace ChillBotV2.Modules
{
    [AdminPrefix, Remarks("admin")]
    [Summary("Commands to help with moderation of the server")]
    [RequireUserPermission(Discord.GuildPermission.ManageGuild)]
    public class Admin : ModuleBase<PrefixCommandContext>
    {
        [Command("ban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Bans a user from the server")]
        public async Task BanUser([Summary("The user to be banned")]SocketUser user, [Summary("Number of days to remove messages from this user - must be between [0, 7]")]int pruneDays = 0, 
            [Summary("The reason of the ban to be written in the audit log"), Remainder]string reason = null)
        {
            await Context.Message.DeleteAsync();
            await (user as SocketGuildUser).BanAsync(pruneDays, reason);
        }

        [Command("unban")]
        [RequireBotPermission(GuildPermission.CreateInstantInvite)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task UnbanUser(SocketGuild guild, ulong userID)
        {
            await Context.Message.DeleteAsync();
            IUser user = (await guild.GetBansAsync()).Where(uid => uid.User.Id == userID).First().User;
            await guild.RemoveBanAsync(user);
        }

        // OWNER COMMANDS
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

        [RequireOwner]
        [Command("initMsgLogChannel")]
        [Summary("Used to initialise the channel message events are logged to\nThis command can only be issued by the owner")]
        public async Task InitMsgLogChannel()
        {
            Global.msgEventLogChannel = Context.Channel;
            await Context.Message.DeleteAsync();
            var m = await Context.Channel.SendMessageAsync("Initialized message log channel");
            await Task.Delay(500)
                .ContinueWith(async task =>
                {
                    await m.DeleteAsync();
                });
        }

        [RequireOwner]
        [Command("initUserLogChannel")]
        [Summary("Used to initialise the channel message events are logged to\nThis command can only be issued by the owner")]
        public async Task InitUserLogChannel()
        {
            Global.userEventLogChannel = Context.Channel;
            await Context.Message.DeleteAsync();
            var m = await Context.Channel.SendMessageAsync("Initialized user log channel");
            await Task.Delay(500)
                .ContinueWith(async task =>
                {
                    await m.DeleteAsync();
                });
        }

        [RequireOwner]
        [Command("initVoiceLogChannel")]
        [Summary("Used to initialise the channel message events are logged to\nThis command can only be issued by the owner")]
        public async Task InitVoiceLogChannel()
        {
            Global.voiceEventLogChannel = Context.Channel;
            await Context.Message.DeleteAsync();
            var m = await Context.Channel.SendMessageAsync("Initialized voice log channel");
            await Task.Delay(500)
                .ContinueWith(async task =>
                {
                    await m.DeleteAsync();
                });
        }

        [RequireOwner]
        [Command("post-rules", RunMode = RunMode.Async)]
        [Summary("Used to display the rules that all members of Games and Chill should abide by. Additionally provides role auto-assignment through reactions")]
        public async Task PostRules()
        {
            await Context.Message.DeleteAsync();

            var embed = new EmbedBuilder()
                .WithTitle("Server Rules:")
                .WithDescription("🔹\tPlease don't spam the text channels. Once is fine. This includes overly spamming/using @ people, especially mods and admins\n\n" +
                "🔹\tSwearing and cussing is allowed here, but don't be too excessive. Be civil. Any defamatory, insulting, or downright jerk-like language will be dealt with " +
                "according to severity and repetition\n\n" +
                "🔹\tPersonal attacks against other users are not allowed. Keep it friendly\n\n" +
                "🔹\tListen to members of staff and comply with any requests they make. If you think they are being unreasonable, do not start arguments in global text channels, " +
                "DM an admin and we'll take it from there\n\n" +
                "🔹\tBe respectful, and don't be a dumbass\n\n" +
                "🔹\tThe discord terms of service apply in this server. You can view their terms here https://discordapp.com/terms as well as their guidelines " +
                "https://discordapp.com/guidelines you will be held accountable if caught breaking any of them")
                .WithColor(0, 0, 255)
                .Build();

            var sent = await Context.Channel.SendMessageAsync("Welcome to Games and Chill!\nClick on the reactions underneath the rules to assign yourself roles and join the rest of the server", false, embed);
            await sent.AddReactionAsync(new Emoji(Global.rainbow6));
            await sent.AddReactionAsync(new Emoji(Global.csgo));
            await sent.AddReactionAsync(new Emoji(Global.ark));
            await sent.AddReactionAsync(new Emoji(Global.overwatch));
            await sent.AddReactionAsync(new Emoji(Global.roe));
            await sent.AddReactionAsync(new Emoji(Global.destiny2));
            await sent.AddReactionAsync(new Emoji(Global.lol));
            await sent.AddReactionAsync(new Emoji(Global.pubg));
            Global.RoleMessageIDTracked = sent.Id;
        }
    }
}
