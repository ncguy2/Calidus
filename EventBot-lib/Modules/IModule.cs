using System.Security.Cryptography;
using Discord.WebSocket;

namespace EventBot.lib.Modules {
    public interface IModule<in ModuleConfig> where ModuleConfig : IModuleConfig {
        string Name { get; set; }

        bool Startup(ModuleConfig cfg);
    }

    public interface IModuleConfig {
        public bool Enabled { get; set; }
    }

    public interface IModuleWithDiscordClient {
        public DiscordSocketClient Discord { get; set; }
    }
    
}