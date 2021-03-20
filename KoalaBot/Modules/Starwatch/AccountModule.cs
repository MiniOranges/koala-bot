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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KoalaBot.Modules.Starwatch
{
    public partial class StarwatchModule
    {
        [Group("account"), Aliases("acc")]
        [Description("Handles accounts.")]
        public partial class AccountModule : BaseCommandModule
        {
            public Koala Bot { get; }
            public IRedisClient Redis => Bot.Redis;
            public Logger Logger { get; }
            public StarwatchClient Starwatch => Bot.Starwatch;

            public AccountModule(Koala bot)
            {
                this.Bot = bot;
                this.Logger = new Logger("CMD-SW-ACC", bot.Logger);
            }

            [Command("create")]
            [Permission("sw.acc.create")]
            [Description("Creates an account")]
            public async Task CreateAccount(CommandContext ctx, [Description("The name of the account to create")] string account, [Description("The password to use")] string password)
            {
                await ctx.ReplyWorkingAsync();

                // this endpoint just hates everything.
                // so we gotta do things differently.
                WebClient wc = new WebClient();
                wc.Credentials = new NetworkCredential(Starwatch.Username, Starwatch.Password);

                Account ac = new Account();
                ac.Name = account;
                ac.Password = password;
                ac.IsActive = true;
                ac.IsAdmin = false;

                try
                {
                    string resp = wc.UploadString(Starwatch.Host + "/api/account", JsonConvert.SerializeObject(ac));
                    Response<Account> respe = JsonConvert.DeserializeObject<Response<Account>>(resp);

                    if (respe.Status != 0)
                    {
                        await ctx.ReplyAsync("Hmm: " + respe.Message);

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

                /*
                await ctx.ReplyWorkingAsync();
                try
                {
                    var response = await Starwatch.CreateAccountAsync(account, password);

                    await ctx.ReplyAsync("Uh:\n```" + JsonConvert.SerializeObject(response) + "```");


                    if (response.Status != RestStatus.OK)
                    {
                        throw new RestResponseException(response);
                    }

                    await ctx.ReplyReactionAsync(true);
                }
                catch (HttpRequestException ex)
                {
                    
                }
                catch (WebException ex)
                {
                    string rt = "";
                    using (var reader = new System.IO.StreamReader(ex.Response.GetResponseStream()))
                    {
                        rt = reader.ReadToEnd();
                    }
                    await ctx.ReplyAsync("Uh wex:\n" + rt);
                }
                catch (Exception ex)
                {
                    await ctx.ReplyAsync("Uh:\n" + ex.ToString());
                    Debug.WriteLine("Something happened: " + ex.ToString());
                }
                
                */
                
            }

            [Command("enable")]
            [Permission("sw.acc.enable")]
            [Description("Enables an account")]
            public async Task EnableAccount(CommandContext ctx, [Description("The name of the account to enable")] string account)
            {
                await ctx.ReplyWorkingAsync();
                var response = await Starwatch.UpdateAccountAsync(account, new Account() { IsActive = true});

                

                if (response.Status != RestStatus.OK)
                    throw new RestResponseException(response);

                //Build the response                
                await ctx.ReplyReactionAsync(true);
            }

            [Command("disable")]
            [Permission("sw.acc.disable")]
            [Description("Disables an account")]
            public async Task DisableAccount(CommandContext ctx, [Description("The name of the account to enable")] string account)
            {
                await ctx.ReplyWorkingAsync();
                var response = await Starwatch.UpdateAccountAsync(account, new Account() { IsActive = false });

                if (response.Status != RestStatus.OK)
                    throw new RestResponseException(response);

                //Build the response                
                await ctx.ReplyReactionAsync(true);
            }

        }
    }
}
