using System;
using Discord.WebSocket;

namespace Calidus.lib.Event {
    public interface EventHandler {

        public void Register(DiscordSocketClient client);

    }

    public static class EventHandlerExtension {
        public static void Log(this EventHandler @this, string msg) {
            Console.WriteLine(msg);
        }
    }
    
}