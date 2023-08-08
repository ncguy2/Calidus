using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EventBot.lib.Modules {
    public class ModuleHost {
        private List<IModule> modules = new();

        public IReadOnlyList<IModule> Modules => modules.AsReadOnly();

        public delegate void OnRegister(IModule module);

        public event OnRegister OnModuleRegister;

        public ModuleHost() {
            OnModuleRegister += module => {
                if (module is ISupervisorModule supervisorModule)
                    supervisorModule.ModuleHost = this;
            };
        }

        public void RegisterModules() {
            Assembly asm = Assembly.GetExecutingAssembly();
            IEnumerable<Type> moduleTypes = asm.GetTypes().Where(x => Attribute.IsDefined(x, typeof(ModuleAttribute)));
            foreach (Type moduleType in moduleTypes) {
                ModuleAttribute mAttr = moduleType.GetCustomAttribute<ModuleAttribute>()!;
                RegisterModuleByRuntimeType(moduleType, mAttr.ConfigType);
            }
        }

        private void RegisterModuleByRuntimeType(Type moduleType, Type configType) {
            MethodInfo method = typeof(ModuleHost).GetMethod(nameof(RegisterModuleByType), BindingFlags.NonPublic | BindingFlags.Instance)!;
            MethodInfo generic = method.MakeGenericMethod(moduleType, configType);
            generic.Invoke(this, null);
        }

        private void RegisterModuleByType<T, CFG>() where T : IModule<CFG>, new() where CFG : IModuleConfig {
            T module = new();
            RegisterModule(module);
        }

        private void RegisterModule<CFG>(IModule<CFG> module) where CFG : IModuleConfig {
            modules.Add(module);
            InjectModuleName(module);
            OnModuleRegister(module);
        }

        public void StartModules(Configuration cfg) {
            foreach (dynamic module in modules)
                module.Bootstrap(cfg);
        }
        
        private void InjectModuleName<CFG>(IModule<CFG> module) where CFG : IModuleConfig {
            ModuleAttribute? moduleAttr = module.GetType().GetCustomAttribute<ModuleAttribute>();
            if (moduleAttr == null)
                return;

            module.Name = moduleAttr.Name;
        }
    }
}