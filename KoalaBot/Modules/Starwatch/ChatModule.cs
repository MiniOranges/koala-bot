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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KoalaBot.Modules.Starwatch
{
    public partial class StarwatchModule
    {
        [Group("chat"), Aliases("c")]
        [Description("Interacts with chat")]
        public partial class ChatModule : BaseCommandModule
        {
            public Koala Bot { get; }
            public IRedisClient Redis => Bot.Redis;
            public Logger Logger { get; }
            public StarwatchClient Starwatch => Bot.Starwatch;

            public ChatModule(Koala bot)
            {
                this.Bot = bot;
                this.Logger = new Logger("CMD-SW-CHAT", bot.Logger);
            }

            [Command("send"), Aliases("s")]
            [Permission("sw.chat.send")]
            [Description("Sends a message to global chat")]
            public async Task SendMessage(CommandContext ctx, [Description("Message")] string message)
            {
                try
                {
                    await ctx.ReplyWorkingAsync();
                    var response = JsonConvert.DeserializeObject<KoalaBot.Starwatch.Entities.RestResponse>(Starwatch.SendMessage(message));

                    if (response.Status != RestStatus.OK)
                    {
                        await ctx.ReplyReactionAsync(false);
                    }
                    else
                    {
                        await ctx.ReplyReactionAsync(true);
                    }
                }
                
                catch (WebException ex)
                {
                    Debug.Write(ex.ToString());
                    Console.WriteLine(ex.ToString());
                    string rt = "";
                    using (var reader = new System.IO.StreamReader(ex.Response.GetResponseStream()))
                    {
                        rt = reader.ReadToEnd();
                    }
                    Console.WriteLine(rt);
                    await ctx.ReplyAsync("Uh wex:\n" + rt);
                }
                catch (Exception ex)
                {
                    await ctx.ReplyAsync("Uh ex:\n" + ex.ToString());
                }
            }


        }
    }
}
