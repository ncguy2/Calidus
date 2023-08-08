using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using EventBot.lib.Modules;

namespace EventBot.lib.Event {
    public class GuildEventFinishHandler : EventHandler, IModuleAttachment<EventModule> {
        public void Register(DiscordSocketClient client) {
            client.GuildScheduledEventCompleted += cleanupRoleForEvent;
            client.GuildScheduledEventCancelled += cleanupRoleForEvent;
        }

        private async Task cleanupRoleForEvent(SocketGuildEvent arg) {
            string? role = arg.Description.Split(" ").FirstOrDefault(x => x.StartsWith("@"));
            if (role == null) {
                this.Log("No role name found");
                return;
            }

            role = OwningModule.FormatRole(role[1..]);

            SocketRole? socketRole = arg.Guild.Roles.First(x => x.Name == role);
            this.Log($"Removing stale role {socketRole.Name}");
            await socketRole.DeleteAsync();
        }

        public EventModule OwningModule { get; set; } = null!;
    }
}