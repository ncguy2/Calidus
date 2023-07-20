using System;

namespace EventBot.lib.Modules {
    public class ModuleAttribute : Attribute {
        public readonly string Name;

        public ModuleAttribute(string name) {
            Name = name;
        }
    }
}