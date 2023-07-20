using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using EventBot.db.mysql.Data.Drivers;
using EventBot.lib.Data;
using EventBot.lib.Defer;
using EventBot.lib.Event;
using EventBot.lib.Event.Interactions;
using EventBot.lib.Mail;
using EventBot.lib.Modules;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace EventBot {
    class Program {
        static Task Main(string[] args) => new Program().MainAsync();

        private DiscordSocketClient client;

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

        private ModuleHost moduleHost;

        public async Task MainAsync() {
            Configuration cfg = Configuration.LoadConfiguration();
            InitialiseServices();
            SetupDatabaseService(cfg.database);
            SetupMailService(cfg.mail);

            moduleHost = new ModuleHost();
            moduleHost.OnModuleRegister += module => {
                if (module is IModuleWithDiscordClient clientModule)
                    clientModule.Discord = client;
            };
            
            DiscordSocketConfig discordConfig = new() {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
                AlwaysDownloadUsers = true
            };
            client = new DiscordSocketClient(discordConfig);
            client.Log += Log;
            client.Ready += () => {
                client.CurrentUser.ModifyAsync(user => {
                    user.Username = cfg.client.displayName;
                });
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