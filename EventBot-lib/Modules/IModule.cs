using System.Linq;
using System.Security.Cryptography;
using Discord.WebSocket;

namespace EventBot.lib.Modules {
    public interface IModule {
        string Name { get; set; }
    }

    public interface IModule<in ModuleConfig> : IModule where ModuleConfig : IModuleConfig {
        bool Startup(ModuleConfig cfg);
    }

    public interface IModuleConfig {
        public bool Enabled { get; set; }
    }

    public struct NullModuleConfig : IModuleConfig {
        public bool Enabled { get; set; }
    }

    public struct HelpText {
        public HelpLine[] Lines;

        public static implicit operator HelpText(string line) {
            return new HelpText { Lines = new[] { new HelpLine { Text = line } } };
        }
        
        public static implicit operator HelpText(string[] lines) {
            return new HelpText { Lines = lines.Select(x => new HelpLine{Text = x}).ToArray()};
        }

        public static implicit operator HelpText(HelpLine line) {
            return new HelpText { Lines = new[] { line } };
        }
    }

    public struct HelpLine {
        public string Text;

        public static implicit operator HelpLine(string text) {
            return new HelpLine { Text = text };
        }
        
        public static implicit operator string(HelpLine line) {
            return line.Text;
        }
    }
}