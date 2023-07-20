using System;
using System.Collections.Generic;
using System.Reflection;

namespace EventBot.lib.Modules {
    public class ModuleHost {
        private List<dynamic> modules = new();

        public delegate void OnRegister(object module);

        public event OnRegister OnModuleRegister;

        public void RegisterModules() {
            RegisterModule<EventModule, EventModuleConfig>();
        }

        private void RegisterModule<T, CFG>() where T : IModule<CFG>, new() where CFG : IModuleConfig {
            T module = new();
            modules.Add(module);
            InjectModuleName<T, CFG>(module);
            OnModuleRegister(module);
        }

        public void StartModules(Configuration cfg) {
            foreach (dynamic module in modules)
                module.Bootstrap(cfg);
        }
        
        private void InjectModuleName<T, CFG>(T module) where T : IModule<CFG>, new() where CFG : IModuleConfig {
            ModuleAttribute? moduleAttr = typeof(T).GetCustomAttribute<ModuleAttribute>();
            if (moduleAttr == null)
                return;

            module.Name = moduleAttr.Name;
        }
    }
}