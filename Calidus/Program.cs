using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Calidus.db.mysql.Data.Drivers;
using Calidus.lib.Data;
using Calidus.lib.Defer;
using Calidus.lib.Mail;
using Calidus.lib.Modules;
using McMaster.Extensions.CommandLineUtils;

namespace Calidus {
    class Program {
        static Task Main(string[] args) => new Program().MainAsync(args);

        private DiscordSocketClient client = null!;

        private Task Log(string msg) {
            Console.WriteLine(msg);
            return Task.CompletedTask;
        }

        private Task Log(LogMessage msg) {
            return Task.CompletedTask;
        }

        private void SetupDatabaseService(Configuration.DriverConfig dbConfig) {
            IDatabaseDriver driver;
            switch (dbConfig.driver.ToLower()) {
                case "mysql":
                    MysqlDriver mysql = new();
                    MysqlDriverConfig sqlcfg = new() {
                        server = dbConfig.driverOptions["server"],
                        userid = dbConfig.driverOptions["user"],
                        password = dbConfig.driverOptions["password"],
                        database = dbConfig.driverOptions["database"]
                    };
                    mysql.Init(sqlcfg);
                    driver = mysql;
                    break;
                default:
                    throw new NotImplementedException($"SQL Driver {dbConfig.driver} not implemented");
            }

            DatabaseService.Instance.SetDriver(driver);
        }

        private void SetupMailService(Configuration.DriverConfig mailConfig) {
            IMailDriver driver;
            switch (mailConfig.driver.ToLower()) {
                case "smtp":
                    SmtpMailDriver smtp = new(mailConfig.driverOptions["host"],
                                              mailConfig.driverOptions["username"],
                                              mailConfig.driverOptions["password"],
                                              bool.Parse(mailConfig.driverOptions["enable_ssl"]));
                    driver = smtp;
                    break;
                default:
                    throw new NotImplementedException($"Mail Driver {mailConfig.driver} not implemented");
            }

            MailService.Instance.setDriver(driver);
        }

        private ModuleHost moduleHost = null!;

        private Dictionary<string, Action<SocketSlashCommand>> commandCallbacks = new();

        private Task SlashCommandHandler(SocketSlashCommand cmd) {
            if (commandCallbacks.ContainsKey(cmd.Data.Name)) 
                commandCallbacks[cmd.Data.Name].Invoke(cmd);
            return Task.CompletedTask;
        }

        public async Task MainAsync(string[] args) {
            var app = new CommandLineApplication();
            app.HelpOption();
            var configPath = app.Option("-d|--data <PATH>",
                                        "The path to find the config/data files required for runtime operation (defaults to '.')",
                                        CommandOptionType.SingleValue);
            configPath.DefaultValue = null;
            app.OnExecuteAsync(_ => MainAsyncConfigured(configPath.Value()));
            await app.ExecuteAsync(args);
        }
        public async Task MainAsyncConfigured(string? configPath) {
            Configuration cfg = Configuration.LoadConfiguration(configPath);
            InitialiseServices();
            SetupDatabaseService(cfg.database);
            SetupMailService(cfg.mail);

            moduleHost = new ModuleHost();
            moduleHost.OnModuleRegister += module => { Log("Module '" + module.Name + "' registered"); };
            moduleHost.OnModuleRegister += module => {
                // ReSharper disable ConvertIfStatementToSwitchStatement
                // ReSharper disable InvertIf

                if (module is IModuleWithDiscordClient clientModule)
                    clientModule.Discord = client;

                if (module is ISlashCommandProviderModule cmdProviderModule) {
                    SlashCommandData[] buildSlashCommands = cmdProviderModule.BuildSlashCommands();
                    if (buildSlashCommands.Length == 0)
                        return;
                    
                    foreach (SlashCommandData cmd in buildSlashCommands) {
                        if (commandCallbacks.ContainsKey(cmd.Properties.Name.Value))
                            throw new Exception($"Duplicate command name \"{cmd.Properties.Name}\" provided by {module.Name}");
                        commandCallbacks.Add(cmd.Properties.Name.Value, cmd.Callback);
                    }

                    foreach (SocketGuild guild in client.Guilds)
                        guild.BulkOverwriteApplicationCommandAsync(
                            buildSlashCommands.Select(x => x.Properties).Cast<ApplicationCommandProperties>()
                                              .ToArray());
                }

                // ReSharper restore ConvertIfStatementToSwitchStatement
                // ReSharper restore InvertIf
            };

            DiscordSocketConfig discordConfig = new() {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
                AlwaysDownloadUsers = true
            };
            client = new DiscordSocketClient(discordConfig);
            client.Log += Log;
            client.SlashCommandExecuted += SlashCommandHandler;
            client.Ready += () => {
                client.CurrentUser.ModifyAsync(user => { user.Username = cfg.client.displayName; });
                DeferService.Get().setDiscordSocketClient(client);
                moduleHost.RegisterModules();
                moduleHost.StartModules(cfg);
                return Log("Ready!");
            };

            await client.LoginAsync(TokenType.Bot, cfg.client.token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private void InitialiseServices() {
            DatabaseService.Get();
            DeferService.Get();
            MailService.Get();
        }
    }
}