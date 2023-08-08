using System;

namespace EventBot.lib.Modules {
    public abstract class BaseModule<ModuleConfig> : IModule<ModuleConfig> where ModuleConfig : IModuleConfig, new() {
        public string Name { get; set; }
        
        public ModuleConfig Config { get; private set; }

        public bool Startup(ModuleConfig cfg) {
            Config = cfg;
            if (!Config.Enabled) {
                Log(Name + " module disabled by config.");
                return false;
            }
            Startup();
            return true;
        }

        public abstract void Startup();

        public void Log(string message) {
            Console.WriteLine($"[{DateTime.Now:yy-MM-dd H:mm:ss}][{Name}] {message}");
        }
        
        public void Bootstrap(Configuration cfg) {
            bool success = cfg.GetModuleConfig(Name.ToLower(), out ModuleConfig moduleConfig);
            if (!success)
                Log($"Failed to load config for module {Name}. Starting with default config");
            Startup(moduleConfig);
        }
        
    }
}