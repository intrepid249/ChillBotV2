using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace ChillBotV2.Services
{
	class ModLogService
	{
		private readonly DiscordSocketClient _client;

		private const string bigBrother = "*Big Brother is watching*";

		public ModLogService(DiscordSocketClient client)
		{
			_client = client;

			_client.MessageDeleted += LogMessageDeleted;
			_client.MessageUpdated += LogMessageUpdated;

			_client.UserUpdated += LogUserUpdated;
			_client.UserVoiceStateUpdated += LogUserVoiceUpdated;

			_client.UserJoined += LogUserJoined;
			_client.UserLeft += LogUserLeft;
			_client.UserBanned += LogUserBanned;
			_client.UserUnbanned += LogUserUnbanned;
		}

		private async Task LogMessageDeleted(Cacheable<IMessage, ulong> msg, ISocketMessageChannel channel)
		{
			if (!hasModLogChannel()) return;

			string msgContents = msg.HasValue ? msg.Value.Content : "could not be retrieved";
			string msgAuthor = msg.HasValue ? msg.Value.Author.Mention : "could not be retrieved";

			RestAuditLogEntry logEntry = null;

			if (msg.HasValue)
			{

				if (msg.Value.Author.IsBot || msg.Value.Content.StartsWith(Core._config["prefix"]) || msg.Value.Content.StartsWith(Core._config["adminprefix"]))
					return;

				var guild = ((SocketTextChannel)msg.Value.Channel).Guild;

				// Get the last 10 logs in case something else happened in-between
				var lastLogs = await guild.GetAuditLogsAsync(10).FlattenAsync();
				// Check to make sure the entry is for the right message
				logEntry = lastLogs.FirstOrDefault(x =>
				{
					var correctMessageType = false;
					var sameMessage = false;

					if (x.Action == ActionType.MessageDeleted)
					{
						correctMessageType = true;
					}
					else
					{
						return false;
					}

					var data = ((MessageDeleteAuditLogData)x.Data);

					if (data.AuthorId == msg.Value.Author.Id)
					{
						sameMessage = true;
					}

					return correctMessageType && sameMessage;
				});
			}

			var deletedBy = logEntry?.User?.Mention ?? "No User found";

			var embed = new EmbedBuilder()
				.WithTitle($"Message Deleted - {bigBrother}")
				.WithCurrentTimestamp()
				.WithDescription($"Message: \n```{msgContents}```\nAuthor: {msgAuthor}\nChannel: #{channel.Name}\nDeleted by: {deletedBy}")
				.WithColor(Color.Red).Build();

			await Global.msgEventLogChannel.SendMessageAsync(embed: embed);
		}

		private Task LogMessageUpdated(Cacheable<IMessage, ulong> msg, SocketMessage message, ISocketMessageChannel channel)
		{
			if (!hasModLogChannel()) return Task.CompletedTask;

			string originalMsg = msg.HasValue ? msg.Value.Content : "could not be retrieved";
			string msgAuthor = msg.HasValue ? msg.Value.Author.Mention : "could not be retrieved";

			var embed = new EmbedBuilder()
				.WithTitle($"Message Edited - {bigBrother}")
				.WithCurrentTimestamp()
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
				.WithCurrentTimestamp()
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
				.WithCurrentTimestamp()
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

		private Task LogUserJoined(SocketGuildUser user)
		{
			var embed = new EmbedBuilder()
				.WithTitle("User joined")
				.WithColor(Color.Orange)
				.WithCurrentTimestamp()
				.WithDescription($"User: {user.Mention}\nUser ID: {user.Id}\nBot user: {isBotUser(user)}").Build();

			if (hasUserLogChannel())
				Global.userEventLogChannel.SendMessageAsync(embed: embed);

			return Task.CompletedTask;
		}

		private Task LogUserLeft(SocketGuildUser user)
		{
			var embed = new EmbedBuilder()
				.WithTitle("User left or was kicked")
				.WithColor(Color.Orange)
				.WithCurrentTimestamp()
				.WithDescription($"User: {user.Mention}\nUser ID: {user.Id}\nBot user: {isBotUser(user)}").Build();

			if (hasUserLogChannel())
				Global.userEventLogChannel.SendMessageAsync(embed: embed);

			return Task.CompletedTask;
		}

		private async Task LogUserBanned(SocketUser user, SocketGuild guild)
		{
			RestBan ban = await guild.GetBanAsync((IUser)user);

			var embed = new EmbedBuilder()
				.WithTitle("User Banned")
				.WithColor(Color.DarkRed)
				.WithCurrentTimestamp()
				.WithDescription($"User: {user.Username}\nUser ID: {user.Id}\nBot User: {isBotUser(user)}" +
				$"\nReason for ban: ```{((ban?.Reason != null) ? ban?.Reason : "No reason given")}```").Build();

			if (Global.userEventLogChannel != null)
				await Global.userEventLogChannel.SendMessageAsync(embed: embed);
		}

		private Task LogUserUnbanned(SocketUser user, SocketGuild guild)
		{
			var embed = new EmbedBuilder()
				.WithTitle("User Unbanned")
				.WithColor(Color.DarkBlue)
				.WithCurrentTimestamp()
				.WithDescription($"User: {user.Username}\nUser ID: {user.Id}\nBot User: {isBotUser(user)}").Build();

			if (hasUserLogChannel())
				Global.userEventLogChannel.SendMessageAsync(embed: embed);

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

		private string isBotUser(SocketUser user)
		{
			return (user.IsBot) ? "yes" : "no";
		}
	}
}
