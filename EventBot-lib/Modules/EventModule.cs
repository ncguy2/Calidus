using System;
using System.Collections.Generic;
using Discord.WebSocket;
using EventBot.Compose;
using EventBot.lib.Event;
using EventBot.lib.Event.Interactions;
using EventHandler = EventBot.lib.Event.EventHandler;

namespace EventBot.lib.Modules {
    
    [Module("event")]
    public class EventModule : BaseModule<EventModuleConfig>, IModuleWithDiscordClient, IComposeable<EventHandler> {
        
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

    }

    public struct EventModuleConfig : IModuleConfig {
        public bool Enabled { get; set; }
        public bool SendEmails { get; set; }
        public bool ManageRoles { get; set; }
        public string RoleFormat { get; set; }
    }
}