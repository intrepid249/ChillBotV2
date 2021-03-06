﻿using ChillBotV2.Context;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace ChillBotV2.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Module | AttributeTargets.Method)]
    class UserPrefixAttribute : PreconditionAttribute
    {
        private string _prefix;

        public UserPrefixAttribute()
        {
            _prefix = Global.userPrefix;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {

            if (context is PrefixCommandContext && ((PrefixCommandContext)context).Prefix == _prefix)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }

            return Task.FromResult(PreconditionResult.FromError("Invalid command prefix"));
        }
    }
}
