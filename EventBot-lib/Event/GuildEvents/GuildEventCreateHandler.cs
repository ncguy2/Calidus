using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using EventBot.lib.Modules;

namespace EventBot.lib.Event {
    public class GuildEventCreateHandler : EventHandler, IModuleAttachment<EventModule> {
        public void Register(DiscordSocketClient client) {
            if(OwningModule.Config.ManageRoles)
                client.GuildScheduledEventCreated += createRoleForEvent;
        }

        private async Task createRoleForEvent(SocketGuildEvent arg) {
            string? role = arg.Description.Split(" ").FirstOrDefault(x => x.StartsWith("@"));
            if (role == null) {
                this.Log("No role name found");
                return;
            }

            // Remove leading @, and add evt_ prefix
            role = OwningModule.FormatRole(role[1..]);

            if (arg.Guild.Roles.Any(x => x.Name == role)) {
                this.Log($"Role with name {role} already exists");
                return;
            }

            this.Log($"Creating role {role} for event {arg.Name}");
            RestRole roleAsync = await arg.Guild.CreateRoleAsync(role, isMentionable: true);

            await arg.Creator.AddRoleAsync(roleAsync);
        }

        public EventModule OwningModule { get; set; }
    }
}