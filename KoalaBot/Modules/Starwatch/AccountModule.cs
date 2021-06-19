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

            [Command("createadmin")]
            [Permission("sw.acc.createadmin")]
            [Description("Creates an admin account")]
            public async Task CreateAccountAdmin(CommandContext ctx, [Description("The name of the account to create")] string account, [Description("The password to use")] string password)
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
                ac.IsAdmin = true;

                try
                {
                    string resp = wc.UploadString(Starwatch.Host + "/api/account", JsonConvert.SerializeObject(ac));
                    Response<Account> respObj = JsonConvert.DeserializeObject<Response<Account>>(resp);

                    if (respObj.Status != 0)
                    {
                        await ctx.ReplyAsync("HTTP Status: " + respObj.Message);
                    }
                    else
                    {
                        await ctx.ReplyReactionAsync(true);
                    }
                }
                catch (WebException ex)
                {
                    Logger.LogError(ex.ToString());

                    string rt = "";

                    using (var reader = new System.IO.StreamReader(ex.Response.GetResponseStream()))
                    {
                        rt = reader.ReadToEnd();
                    }

                    Logger.LogError(rt);

                    Response<Account> t = JsonConvert.DeserializeObject<Response<Account>>(rt);

                    if (t.Message != null)
                    {
                        await ctx.ReplyAsync(t.Message);
                    }
                    else
                    {
                        await ctx.ReplyAsync("A web exception occurred processing this command.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                    await ctx.ReplyAsync("An exception occurred processing this command.");
                }
            }

            [Command("changepass")]
            [Permission("sw.acc.changepass")]
            [Description("Changes an account password")]

            public async Task ChangeAccountPassword (CommandContext ctx, [Description("The name of the account to enable")] string account, [Description("Pass")] string password)
            {
                await ctx.ReplyWorkingAsync();
                var response = await Starwatch.UpdateAccountAsync(account, new Account() { Password = password });

                if (response.Status != RestStatus.OK)
                    throw new RestResponseException(response);

                //Build the response                
                await ctx.ReplyReactionAsync(true);
            }

            [Command("demote")]
            [Permission("sw.acc.demote")]
            [Description("Demotes an account")]

            public async Task DemoteAccount(CommandContext ctx, [Description("The name of the account to enable")] string account)
            {
                await ctx.ReplyWorkingAsync();
                var response = await Starwatch.UpdateAccountAsync(account, new Account() { IsAdmin = false });

                if (response.Status != RestStatus.OK)
                    throw new RestResponseException(response);

                //Build the response                
                await ctx.ReplyReactionAsync(true);
            }

            [Command("promote")]
            [Permission("sw.acc.promote")]
            [Description("Promotes an account")]

            public async Task PromoteAccount(CommandContext ctx, [Description("The name of the account to enable")] string account)
            {
                await ctx.ReplyWorkingAsync();
                var response = await Starwatch.UpdateAccountAsync(account, new Account() { IsAdmin = true });

                if (response.Status != RestStatus.OK)
                    throw new RestResponseException(response);

                //Build the response                
                await ctx.ReplyReactionAsync(true);
            }

            [Command("get")]
            [Permission("sw.acc.get")]
            [Description("Gets an account")]
            public async Task GetAccount (CommandContext ctx, [Description("The name of the account to get")] string account)
            {
                await ctx.ReplyWorkingAsync();
                var response = await Starwatch.GetAccountAsync(account);

                if (response.Status != RestStatus.OK)
                {
                    throw new RestResponseException(response);
                }

                if (response.Payload == null)
                {
                    await ctx.ReplyAsync("User not found.");
                }
                else
                {
                    string output = "```\n" +
                        "Username: " + (response.Payload.Name ?? "null") + "\n" +
                        "IsAdmin: " + response.Payload.IsAdmin + "\n" +
                        "IsActive: " + response.Payload.IsActive + "\n" +
                        "```";
                    await ctx.ReplyAsync(output);

                }
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
            public async Task DisableAccount(CommandContext ctx, [Description("The name of the account to disable")] string account)
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
