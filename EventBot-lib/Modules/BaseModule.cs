using System;

namespace EventBot.lib.Modules {
    public abstract class BaseModule<ModuleConfig> : IModule<ModuleConfig> where ModuleConfig : IModuleConfig, new() {
        public string Name { get; set; }
        public ModuleConfig Config { get; private set; }

        public bool Startup(ModuleConfig cfg) {
            Config = cfg;
            if (!Config.Enabled)
                return false;
            Startup();
            return true;
        }

        public abstract void Startup();
        
        public void Bootstrap(Configuration cfg) {
            bool success = cfg.GetModuleConfig(Name.ToLower(), out ModuleConfig moduleConfig);
            if (!success)
                Console.WriteLine($"Failed to load config for module {Name}. Starting with default config");
            Startup(moduleConfig);
        }
        
    }
}