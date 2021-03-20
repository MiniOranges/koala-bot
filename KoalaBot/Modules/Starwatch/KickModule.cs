using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using KoalaBot.CommandNext;
using KoalaBot.Exceptions;
using KoalaBot.Extensions;
using KoalaBot.Logging;
using KoalaBot.Redis;
using KoalaBot.Starwatch;
using KoalaBot.Starwatch.Entities;
using KoalaBot.Starwatch.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KoalaBot.Modules.Starwatch
{
    public partial class StarwatchModule
    {
        [Group("kick"), Aliases("k")]
        [Description("Kicks people.")]
        public partial class KickModule : BaseCommandModule
        {
            public Koala Bot { get; }
            public IRedisClient Redis => Bot.Redis;
            public Logger Logger { get; }
            public StarwatchClient Starwatch => Bot.Starwatch;

            public KickModule(Koala bot)
            {
                this.Bot = bot;
                this.Logger = new Logger("CMD-SW-KICK", bot.Logger);
            }


            [Command("cid"), Aliases("$")]
            [Permission("sw.kick.player")]
            [Description("Kicks a player by CID ($)")]
            public async Task KickPlayerCID(CommandContext ctx, [Description("Player CID")] long cid, [Description("Kick reason")] string reason)
            {
                await ctx.ReplyWorkingAsync();
                var response = await Starwatch.KickPlayerAsync(cid, reason);

                if (response.Status != RestStatus.OK)
                    throw new RestResponseException(response);

                //Build the response                
                await ctx.ReplyReactionAsync(response.Payload);
            }

            [Command("character"), Aliases("char", "chara")]
            [Permission("sw.kick.player")]
            [Description("Kicks a player by character name")]
            public async Task KickPlayerCharacter(CommandContext ctx, [Description("Character Name")] string character, [Description("Kick reason")] string reason)
            {
                await ctx.ReplyWorkingAsync();



                /*var response = await Starwatch.KickPlayerAsync(cid, reason);

                if (response.Status != RestStatus.OK)
                    throw new RestResponseException(response);

                //Build the response                
                await ctx.ReplyReactionAsync(response.Payload);*/


            }


            private DiscordEmbed BuildKickEmbed(Session session)
            {
                DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
                builder.WithTitle($"Kicked {session.Username}")
                    .AddField("IP", session.IP ?? "N/A")
                    .AddField("UUID", session.UUID ?? "N/A");
                return builder.Build();
            }
        }
    }
}
