using System;

namespace Calidus.lib.Modules {
    public class ModuleAttribute : Attribute {
        public readonly string Name;
        public readonly Type ConfigType;

        public ModuleAttribute(string name, Type configType) {
            Name = name;
            ConfigType = configType;
        }
    }
}