using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChillBotV2
{
    internal class Global
    {
        // TRACK AUTO ROLE ASSIGNMENTS
        internal static ulong RoleMessageIDTracked { get; set; }

        internal static string rainbow6 = ":Rainbow6:519941356001427457";
        internal static string csgo = ":csgo:519941393502699528";
        internal static string ark = ":ARK:519941553930764289";
        internal static string overwatch = ":overwatch:519943250518212618";
        internal static string roe = ":RingOfElysium:519943453669457932";
        internal static string destiny2 = ":destiny2:519944519630979072";
        internal static string lol = ":LeagueOfLegends:519944061730160641";
        internal static string pubg = ":pubg:519946896706502697";
        // ===========================

        // MODERATION CHANNELS
        internal static ISocketMessageChannel msgEventLogChannel { get; set; } = null;
        internal static ISocketMessageChannel voiceEventLogChannel { get; set; } = null;
        internal static ISocketMessageChannel userEventLogChannel { get; set; } = null;

        internal static Nullable<ulong> inviteChannelID { get; set; } = null;
        // ===========================

        // Config Variables
        internal static String userPrefix { get; } = Core._config["prefix"];
        internal static String adminPrefix { get; } = Core._config["adminprefix"];
    }
}
