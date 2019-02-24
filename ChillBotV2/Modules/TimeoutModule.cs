using ChillBotV2.Attributes;
using ChillBotV2.Context;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace ChillBotV2.Modules
{
    public class TimeoutModule : ModuleBase<PrefixCommandContext>
    {
        private IGuildUser _timedOutUser; // change this to an array??
        private string _timeoutRole = "timeout";
        private System.Timers.Timer _timeoutTimer;
        private ulong _currentVoiceChannel;

        [AdminPrefix]
        [Command("timeout")]
        [Summary("Prevent a user from sending messages and move them to an isolated corner to \'cool down\'")]
        [RequireUserPermission(GuildPermission.Administrator | GuildPermission.ManageGuild)]
        public async Task Timeout([Summary("@user")] IGuildUser user, [Summary("5")] double time = 5, [Summary("s")] char measure = 's')
        {
            await Context.Message.DeleteAsync();

            if (UserHasRole((SocketGuildUser)user, "Admin") || user.GuildPermissions.Has(GuildPermission.Administrator))
            {
                await Context.Channel.SendMessageAsync("Administrators cannot be timed out");
                return;
            }

            // Convert the timeout duration into milliseconds
            string timeMeasure = "";
            double timeInMilliseconds = 0;
            if (measure == 's')
            {
                timeMeasure = "seconds";
                timeInMilliseconds = time * 1000;
            }
            if (measure == 'm')
            {
                timeMeasure = "minutes";
                timeInMilliseconds = time * 60.0 * 1000;
            }

            // Create a new timer object that will restore user chat permissions upon completion
            _timeoutTimer = new System.Timers.Timer(timeInMilliseconds);
            _timeoutTimer.AutoReset = false;
            _timeoutTimer.Elapsed += TimeoutAction;
            _timeoutTimer.Start();
            //_timeoutTimer.Elapsed += TimeoutAction;

            // Alert the user they have been given a timeout
            // Specific to Games and Chill discord - Send the message in the "Visa-Application" channel
            var channelResult = from ch in Context.Guild.TextChannels
                                where ch.Name == "visa-application"
                                select ch.Id;
            ulong channelID = channelResult.FirstOrDefault();
            if (channelID == 0)
            {
                await Context.Channel.SendMessageAsync($"{user.Mention} You have been given a timeout  for {time / 1.0} {timeMeasure}");
            }
            else
            {
                var deportChannel = Context.Guild.GetTextChannel(channelID);
                await deportChannel.SendMessageAsync($"{user.Mention} You have been given a timeout for {time / 1.0} {timeMeasure}\nPlease see the pinned messages" +
                    $" about how you can appeal the timeout");
            }

            // Strip the user of chat privelages
            _timedOutUser = user;
            var result = from r in user.Guild.Roles
                         where r.Name == _timeoutRole
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0)
            {
                var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync($"Unable to find the role \'{_timeoutRole}\'");
                return;
            }
            await user.AddRoleAsync(user.Guild.GetRole(roleID));

            // Store the current connected voice channel to reconnect the user to after the timeout has been lifted
            _currentVoiceChannel = user.VoiceChannel.Id;

            // Move the user (if connected to a voice channel)
            var deportVoice = from ch in Context.Guild.VoiceChannels
                              where ch.Name == "deportation-zone"
                              select ch.Id;
            ulong voiceChannelID = deportVoice.FirstOrDefault();
            if (voiceChannelID == 0)
            {
                var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync("Could not locate the deportation-zone channel");
                return;
            }
            await user.ModifyAsync(x =>
            {
                x.Channel = Context.Guild.GetVoiceChannel(voiceChannelID);
            });

            return;
        }

        private async void TimeoutAction(object sender, ElapsedEventArgs e)
        {
            // Do nothing if timeout has been appealed already
            if (!UserHasRole((SocketGuildUser)_timedOutUser, "timeout")) return;

            // Restore user chat permissions
            await RemoveTimeoutRole(_timedOutUser);
        }

        [AdminPrefix]
        [Command("removetimeout")]
        [Summary("Lift a user's timeout before the specified time has expired")]
        [RequireUserPermission(GuildPermission.Administrator | GuildPermission.ManageGuild)]
        public async Task RemoveTimeout([Summary("@user")] IGuildUser user)
        {
            await RemoveTimeoutRole(user);
        }

        private async Task RemoveTimeoutRole(IGuildUser user)
        {
            var result = from r in user.Guild.Roles
                         where r.Name == _timeoutRole
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0)
            {
                var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync($"Problem removing the role \'{_timeoutRole}\' from {user}");
            }
            await user.RemoveRoleAsync(user.Guild.GetRole(roleID));
            if (_currentVoiceChannel != 0)
            {
                await user.ModifyAsync(x =>
                {
                    x.Channel = Context.Guild.GetVoiceChannel(_currentVoiceChannel);
                });
                _currentVoiceChannel = 0;
            }

            await Context.Channel.SendMessageAsync($"{user.Mention} your timeout has been lifted!\nWelcome back to the chill zone");
        }

        private bool UserHasRole(SocketGuildUser user, string targetRoleName)
        {
            var result = from r in user.Roles
                         where r.Name == targetRoleName
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return false;
            var targetRole = user.Guild.GetRole(roleID);
            return user.Roles.Contains(targetRole);
        }
    }
}
