using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Discord;
using Discord.WebSocket;
using EventBot.Compose;
using EventBot.lib.Event;
using EventBot.lib.Event.Interactions;
using EventHandler = EventBot.lib.Event.EventHandler;

namespace EventBot.lib.Modules {
    
    [Module("event", typeof(EventModuleConfig))]
    public class EventModule : BaseModule<EventModuleConfig>, IModuleWithDiscordClient, IComposeable<EventHandler>, IModuleWithHelp {
        
        private List<EventHandler> eventHandlers = new();
        
        List<EventHandler> IComposeable<EventHandler>.Items {
            get => eventHandlers;
            set => eventHandlers = value;
        }

        public void Populate(Action<EventHandler> configuration) {
            this.Add<EventHandler, GuildEventHandlerGroup>(configuration);
            if(!Config.SendEmails)
                this.Add<EventHandler, RegisterEmailInteractionHandler>(configuration);
        }

        public override void Startup() {
            Populate(t => {
                if (t is IModuleAttachment<EventModule> a)
                    a.OwningModule = this;
                t.Register(Discord);
            });
        }


        public DiscordSocketClient Discord { get; set; }

        public string FormatRole(string roleName) {
            string format = Config.RoleFormat.Replace("%", "{0}");
            return string.Format(format, roleName);
        }

        public HelpText GetHelp() {
            return new[] {
                "Handles event invites and roles",
                "",
                Format.Bold("Creating an event role"),
               $"When creating the event, {Discord.CurrentUser.Username} can also create a role for said event, " +
                "automatically adding interested individuals to said role.",
                "To make use of this function, the role name must be defined within the event description. Prefixing a " +
               $"string with {Format.Code("@")} will use that as the event role (following the format of " +
               $"{Format.Code(Config.RoleFormat)}, where {Format.Code("%")} is replaced by the string provided).",
                "",
                Format.Bold("Handling email invites"),
               $"When a user registers with an event, {Discord.CurrentUser.Username} will attempt to send an ics email " +
               $"to the email associated with their account (within {Discord.CurrentUser.Username}).",
               $"If they have not yet registered with {Discord.CurrentUser.Username}, {Discord.CurrentUser.Username} will DM them prompting them for registration.",
               $"Registration consists of providing an email, and nothing else. There is also the option of opting-out, which will prevent {Discord.CurrentUser.Username} from " +
                "asking for an email again from that user whenever they register with an event, however do note that this is not user-reversible."
            };
        }
    }

    public struct EventModuleConfig : IModuleConfig {
        public bool Enabled { get; set; }
        public bool SendEmails { get; set; }
        public bool ManageRoles { get; set; }
        public string RoleFormat { get; set; }

    }
}