using ChillBotV2.Attributes;
using ChillBotV2.Context;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChillBotV2.Modules.Core
{
    [AdminPrefix]
    [Group("Help")]
    [Remarks("admin, user")]
    [Summary("Provides description of commands to provide information on how to use commands")]
    public class Help : ModuleBase<PrefixCommandContext>
    {
        private readonly CommandService _commands;

        public Help(CommandService commands) => _commands = commands;

        [Command]
        [AdminPrefix]
        [Summary("Get a list of all the admin modules")]
        public async Task AdminHelpAsync()
        {
            var modules = _commands.Modules.Where(m => !m.IsSubmodule && m.Remarks.Contains("admin"));
            var embed = new EmbedBuilder()
                .WithColor(Color.DarkBlue)
                .WithTitle("Modules");
            var sb = new StringBuilder();
            foreach (var moduleInfo in modules)
            {
                //Console.WriteLine($"{moduleInfo.Name}");
                //var attribs = moduleInfo.GetType().GetCustomAttributes(true);
                //foreach (var att in attribs)
                //    Console.Write($"{att.GetType().Name} ");

                AppendModuleHelp(sb, moduleInfo);
            }
                
                

            embed.WithDescription(sb.ToString());

            await ReplyAsync(embed: embed.Build());
        }

        [Command]
        [AdminPrefix]
        [Summary("Get help for a specific module or command")]
        public async Task AdminHelpAsync([Remainder, Summary("name")] string searchString)
        {
            await Context.Message.DeleteAsync();

            var modules = _commands.Modules.Where(m => m.Name.Equals(searchString, StringComparison.OrdinalIgnoreCase)
                || m.Aliases.Any(a => a.Equals(searchString, StringComparison.OrdinalIgnoreCase)));
            var commands = _commands.Commands
                .Where(c => c.Aliases.Any(a => a.Equals(searchString, StringComparison.OrdinalIgnoreCase))
                    && !modules.Any(m => m == c.Module));

            if (!modules.Any() && !commands.Any())
            {
                var m = await ReplyAsync("No command or Module matching that name was found");
                await Task.Delay(1500);
                await m.DeleteAsync();
            }
            var msb = new StringBuilder();
            var csb = new StringBuilder();
            foreach (var module in modules)
            {
                AppendModuleHelp(msb, module);
                foreach (var cinfo in module.Commands)
                {
                    if (await ValidatePreconditionsAsync(cinfo))
                        AppendCommandHelp(msb, cinfo);
                }
            }

            foreach (var command in commands)
            {
                if (await ValidatePreconditionsAsync(command))
                    AppendCommandHelp(csb, command);
            }

            var embed = new EmbedBuilder()
                .WithColor(Color.DarkBlue);
            if (msb.Length > 0)
                embed.AddField("Modules", msb.ToString());
            if (csb.Length > 0)
                embed.AddField("Commands", csb.ToString());

            embed.WithFooter("<> = required, [] = optional ... = multiword");

            await ReplyAsync(embed: embed.Build());
        }

        private async Task<bool> ValidatePreconditionsAsync(CommandInfo info)
        {
            foreach (var precon in info.Preconditions)
                if (!(await precon.CheckPermissionsAsync(Context, info, null)).IsSuccess)
                    return false;
            return true;
        }

        private void AppendModuleHelp(StringBuilder sb, ModuleInfo moduleInfo)
        {
            sb.AppendLine($"**{(char.ToUpper(moduleInfo.Name[0]) + moduleInfo.Name.Substring(1, moduleInfo.Name.Length - 1))}**");
            if (moduleInfo.Aliases.Count > 1)
                sb.AppendLine($"Aliases: {string.Join(", ", moduleInfo.Aliases.Where(a => !a.Equals(moduleInfo.Name, StringComparison.OrdinalIgnoreCase)))}");
            if (moduleInfo.Summary != null)
                sb.AppendLine($"{moduleInfo.Summary}");
            sb.AppendLine();
        }

        private void AppendCommandHelp(StringBuilder sb, CommandInfo commandInfo)
        {
            sb.AppendLine($"**{(commandInfo.Name.Contains("Async") ? commandInfo.Aliases.First() : commandInfo.Name)}**");
            if (commandInfo.Aliases.Count > 1)
                sb.AppendLine($"Aliases: {string.Join(", ", commandInfo.Aliases.Where(a => !a.Equals(commandInfo.Name, StringComparison.OrdinalIgnoreCase)))}");
            sb.AppendLine($"Signature: {(commandInfo.Parameters.Count > 0 ? string.Join(", ", commandInfo.Parameters.Select(p => FormatParameter(p))) : "none.")}");
            if (commandInfo.Summary != null)
                sb.AppendLine($"Summary: {commandInfo.Summary}");
        }

        private string FormatParameter(Discord.Commands.ParameterInfo info)
        {
            var fs = info.IsOptional ? "[{0}]" : "<{0}>";
            if (info.IsRemainder)
                fs += " ...";

            return string.Format(fs, info.Summary ?? info.Name);
        }
    }
}
