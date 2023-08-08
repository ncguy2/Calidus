
using System;
using Calidus.lib.Modules;

namespace Calidus.lib.Event {
    public class GuildEventHandlerGroup : EventHandlerGroup, IModuleAttachment<EventModule> {

        public EventModule OwningModule { get; set; } = null!;

        public override void Populate(Action<EventHandler> configuration) {
            AddItem<GuildEventCreateHandler>(configuration);
            AddItem<GuildEventFinishHandler>(configuration);
            AddItem<GuildEventUserHandler>(configuration);
        }
    }
}