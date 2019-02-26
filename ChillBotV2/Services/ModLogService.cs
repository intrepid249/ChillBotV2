using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace ChillBotV2.Services
{
    class ModLogService
    {
        private readonly DiscordSocketClient _client;

        private readonly string bigBrother = "*Big Brother is watching*";

        public ModLogService(DiscordSocketClient client)
        {
            _client = client;

            _client.MessageDeleted += LogMessageDeleted;
            _client.MessageUpdated += LogMessageUpdated;

            _client.UserUpdated += LogUserUpdated;
            _client.UserVoiceStateUpdated += LogUserVoiceUpdated;
        }

        private Task LogMessageDeleted(Cacheable<IMessage, ulong> msg, ISocketMessageChannel channel)
        {
            if (!hasModLogChannel()) return Task.CompletedTask;

            string msgContents = msg.HasValue ? msg.Value.Content : "could not be retrieved";
            string msgAuthor = msg.HasValue ? msg.Value.Author.Mention : "could not be retrieved";

            var embed = new EmbedBuilder()
                .WithTitle($"Message Deleted - {bigBrother}")
                .WithTimestamp(DateTimeOffset.UtcNow.LocalDateTime)
                .WithDescription($"Message: \n```{msgContents}```\nAuthor: {msgAuthor}\nChannel: #{channel.Name}")
                .WithColor(Color.Red).Build();

            if (!msg.Value.Author.IsBot && !msg.Value.Content.Contains(Core._config["prefix"]) && !msg.Value.Content.Contains(Core._config["adminprefix"]))
                Global.msgEventLogChannel.SendMessageAsync(embed: embed);

            return Task.CompletedTask;
        }

        private Task LogMessageUpdated(Cacheable<IMessage, ulong> msg, SocketMessage message, ISocketMessageChannel channel)
        {
            if (!hasModLogChannel()) return Task.CompletedTask;

            string originalMsg = msg.HasValue ? msg.Value.Content : "could not be retrieved";
            string msgAuthor = msg.HasValue ? msg.Value.Author.Mention : "could not be retrieved";

            var embed = new EmbedBuilder()
                .WithTitle($"Message Edited - {bigBrother}")
                .WithTimestamp(DateTimeOffset.UtcNow.LocalDateTime)
                .WithDescription($"Original: \n```{originalMsg}```\nNew: ```{message.Content}```\nAuthor: {msgAuthor}\nChannel: #{channel.Name}")
                .WithColor(Color.DarkOrange).Build();

            if (!msg.Value.Author.IsBot && !msg.Value.Content.Contains(Core._config["prefix"]) && !msg.Value.Content.Contains(Core._config["adminprefix"]))
                Global.msgEventLogChannel.SendMessageAsync(embed: embed);
            
            return Task.CompletedTask;
        }

        private Task LogUserUpdated(SocketUser before, SocketUser after)
        {
            if (!hasUserLogChannel()) return Task.CompletedTask;

            var embed = new EmbedBuilder()
                .WithTitle($"User Updated - {bigBrother}")
                .WithTimestamp(DateTimeOffset.UtcNow.LocalDateTime)
                .WithColor(0xfa107a);

            if (!before.Username.Equals(after.Username))
            {
                string oldUsername = before.Username, newUsername = after.Username;
                embed.WithDescription($"Old Username: {oldUsername}\nNew Username: {newUsername}");
            }

            if (embed.Description != null)
                Global.userEventLogChannel.SendMessageAsync(embed: embed.Build());

            return Task.CompletedTask;
        }

        private Task LogUserVoiceUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if (!hasVoiceLogChannel()) return Task.CompletedTask;

            var embed = new EmbedBuilder()
                .WithTimestamp(DateTimeOffset.UtcNow.LocalDateTime)
                .WithColor(Color.Purple);

            // User joined voice channel
            if (before.VoiceChannel == null && after.VoiceChannel != null && after.VoiceChannel.Guild == (Global.voiceEventLogChannel as SocketGuildChannel).Guild)
            {
                embed.WithTitle($"User Joined Voice - {bigBrother}");
                embed.WithDescription($"Channel: {after.VoiceChannel.Name}\nUser: {user.Mention}");
            }
            // User left voice channel
            if (before.VoiceChannel != null && after.VoiceChannel == null && before.VoiceChannel.Guild == (Global.voiceEventLogChannel as SocketGuildChannel).Guild)
            {
                embed.WithTitle($"User Left Voice - {bigBrother}");
                embed.WithDescription($"Channel: {before.VoiceChannel.Name}\nUser: {user.Mention}");
            }
            // User changed voice channel
            if (after.VoiceChannel != null && before.VoiceChannel != null && after.VoiceChannel != before.VoiceChannel && 
                before.VoiceChannel.Guild == (Global.voiceEventLogChannel as SocketGuildChannel).Guild)
            {
                embed.WithTitle($"User Moved Channel - {bigBrother}");
                embed.WithDescription($"From: {before.VoiceChannel.Name}\nTo: {after.VoiceChannel.Name}\nUser: {user.Mention}");
            }


            if (embed.Description != null)
                Global.voiceEventLogChannel.SendMessageAsync(embed: embed.Build());

            return Task.CompletedTask;
        }

        private bool hasModLogChannel()
        {
            return (Global.msgEventLogChannel != null);
        }

        private bool hasUserLogChannel()
        {
            return (Global.userEventLogChannel != null);
        }

        private bool hasVoiceLogChannel()
        {
            return (Global.voiceEventLogChannel != null);
        }
    }
}
