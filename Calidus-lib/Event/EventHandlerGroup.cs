using System;
using System.Collections.Generic;
using Discord.WebSocket;
using Calidus.Compose;

namespace Calidus.lib.Event {
    public abstract class EventHandlerGroup : EventHandler, IComposeable<EventHandler> {
        
        private List<EventHandler> eventHandlers = new();
        
        List<EventHandler> IComposeable<EventHandler>.Items {
            get => eventHandlers;
            set => eventHandlers = value;
        }

        public abstract void Populate(Action<EventHandler> configuration);

        public void AddItem<T>(Action<EventHandler> config) where T : EventHandler, new() {
            this.Add<EventHandler, T>(config);
        }

        public void Register(DiscordSocketClient client) {
            eventHandlers.ForEach(e => e.Register(client));
        }

    }
}