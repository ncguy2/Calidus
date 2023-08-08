
using System;
using EventBot.lib.Modules;

namespace EventBot.lib.Event {
    public class GuildEventHandlerGroup : EventHandlerGroup, IModuleAttachment<EventModule> {

        public EventModule OwningModule { get; set; } = null!;

        public override void Populate(Action<EventHandler> configuration) {
            AddItem<GuildEventCreateHandler>(configuration);
            AddItem<GuildEventFinishHandler>(configuration);
            AddItem<GuildEventUserHandler>(configuration);
        }
    }
}