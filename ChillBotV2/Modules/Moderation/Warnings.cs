using ChillBotV2.Attributes;
using ChillBotV2.Context;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChillBotV2.Modules.Moderation
{
    [AdminPrefix, Remarks("admin")]
    [Summary("Provides administration commands used to formally issue people with warnings")]
    [RequireUserPermission(Discord.GuildPermission.Administrator)]
    public class Warnings : ModuleBase<PrefixCommandContext>
    {
        [Command("warn"), Alias("w")]
        [Summary("Formally issue a specified user with a warning")]
        public async Task WarnUser()
        {

        }
    }
}
