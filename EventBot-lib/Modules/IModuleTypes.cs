using System;
using Discord;
using Discord.WebSocket;

namespace EventBot.lib.Modules {
    public interface IModuleWithDiscordClient {
        public DiscordSocketClient Discord { get; set; }
    }

    public interface IModuleWithHelp {
        HelpText GetHelp();
    }

    public interface ISupervisorModule {
        public ModuleHost ModuleHost { get; set; }
    }

    public interface ISlashCommandProviderModule {
        SlashCommandData[] BuildSlashCommands();
    }



    public struct SlashCommandData {
        public SlashCommandProperties Properties;
        public Action<SocketSlashCommand> Callback;
    }
    
}