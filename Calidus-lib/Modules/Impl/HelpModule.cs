using System.Linq;
using System.Text;
using Discord;
using Discord.WebSocket;

using HelpDict = System.Collections.Generic.Dictionary<string, Calidus.lib.Modules.HelpText>;

namespace Calidus.lib.Modules {
    [Module("help", typeof(NullModuleConfig))]
    public class HelpModule : BaseModule<NullModuleConfig>, IModuleWithHelp, ISupervisorModule, ISlashCommandProviderModule {

        public override void Startup() {
            HelpDict helps = GetModuleHelps();
            Log("Help gathered. Entries found: " + helps.Keys.Count);
        }

        private HelpDict GetModuleHelps() {
            return ModuleHost.Modules.ToDictionary<IModule, string, HelpText>(module => module.Name, GetHelpFromModule);
        }

        private HelpText GetHelpFromModule(IModule module) {
            if (module is IModuleWithHelp help)
                return help.GetHelp();
            return "No help provided";
        }

        public HelpText GetHelp() {
            return "Prints help for each module";
        }

        public SlashCommandData[] BuildSlashCommands() {
            return new[] {
                new SlashCommandData {
                    Properties = new SlashCommandBuilder().WithName("help")
                                                          .WithDescription(GetHelp().ToString())
                                                          .AddOption("module", ApplicationCommandOptionType.String, "A specific module", isRequired: false)
                                                          .Build(),
                    Callback = Command_Help 
                }
            };
        }

        private void Command_Help(SocketSlashCommand cmd) {
            HelpDict helps = GetModuleHelps();
            StringBuilder sb = new();

            if (cmd.Data.Options.Count == 0) {
                foreach ((string? key, HelpText value) in helps)
                    sb.Append(Format.Bold(key)).Append(": ").AppendLine(value.Lines[0]);
            } else {
                SocketSlashCommandDataOption option = cmd.Data.Options.First();
                string value = option.Value.ToString() ?? string.Empty;
                sb.AppendLine(Format.Bold(value));
                if (helps.TryGetValue(value, out HelpText help)) {
                    foreach (HelpLine helpLine in help.Lines)
                        sb.AppendLine(helpLine);
                } else 
                    sb.AppendLine("No help found.");
            }

            cmd.RespondAsync(sb.ToString(), ephemeral: true);
        }

        public ModuleHost ModuleHost { get; set; } = null!;
    }
}
