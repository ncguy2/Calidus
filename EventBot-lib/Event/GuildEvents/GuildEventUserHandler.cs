#nullable enable
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using EventBot.lib.Data;
using EventBot.lib.Data.Types;
using EventBot.lib.Defer;
using EventBot.lib.Mail;
using EventBot.lib.Modules;

namespace EventBot.lib.Event {
    public class GuildEventUserHandler : EventHandler, IModuleAttachment<EventModule> {
        public void Register(DiscordSocketClient client) {
            if(OwningModule.Config.ManageRoles) {
                client.GuildScheduledEventUserAdd += addUserToEventRole;
                client.GuildScheduledEventUserRemove += removeUserFromEventRole;
            }
            if(OwningModule.Config.SendEmails)
                client.GuildScheduledEventUserAdd += sendEventICSToUser;
        }

        private async Task addUserToEventRole(Cacheable<SocketUser, RestUser, IUser, ulong> userCache,
                                              SocketGuildEvent evt) {
            string? roleName = evt.Description.Split(" ").FirstOrDefault(x => x.StartsWith("@"));
            if (roleName == null) {
                this.Log("No role name found");
                return;
            }

            roleName = OwningModule.FormatRole(roleName[1..]);

            SocketRole? role = evt.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
            if (role == null) {
                this.Log($"Role {roleName} not found");
                return;
            }

            IUser user = await userCache.GetOrDownloadAsync();
            user = evt.Guild.GetUser(user.Id);
            switch (user) {
                case null:
                    this.Log("User is null");
                    return;
                case IGuildUser gUser when !gUser.RoleIds.Contains(role.Id):
                    this.Log($"Adding role {role.Name} to {gUser.DisplayName}");
                    await gUser.AddRoleAsync(role);
                    break;
                case IGuildUser gUser:
                    this.Log($"User {gUser.DisplayName} already has role {role.Name}");
                    break;
                default:
                    this.Log($"User {user.Username} isn't a guild user");
                    break;
            }
        }

        private async Task removeUserFromEventRole(Cacheable<SocketUser, RestUser, IUser, ulong> userCache, SocketGuildEvent evt) {
            string? roleName = evt.Description.Split(" ").FirstOrDefault(x => x.StartsWith("@"));
            if (roleName == null) {
                this.Log("No role name found");
                return;
            }
            roleName = OwningModule.FormatRole(roleName[1..]);

            SocketRole role = evt.Guild.Roles.First(x => x.Name == roleName);
            IUser user = await userCache.GetOrDownloadAsync();
            user = evt.Guild.GetUser(user.Id);
            switch (user) {
                case null:
                    return;
                case IGuildUser gUser when gUser.RoleIds.Contains(role.Id):
                    this.Log($"Removing role {role.Name} from {gUser.DisplayName}");
                    await gUser.RemoveRoleAsync(role);
                    break;
                case IGuildUser gUser:
                    this.Log($"User {gUser.DisplayName} doesn't have role {role.Name}");
                    break;
                default:
                    this.Log($"User {user.Username} isn't a guild user");
                    break;
            }
        }

        private string? getEmailFromUser(IUser user) {
            DatabaseFacade<UserEmail> userEmailFacade = new();
            UserEmail? userEmail = userEmailFacade.Select(user.Id);
            return userEmail?.email;
        }

        private async Task sendEventICSToUser(Cacheable<SocketUser, RestUser, IUser, ulong> userCache,
                                              SocketGuildEvent evt) {
            IUser user = await userCache.GetOrDownloadAsync();
            SocketGuildUser gUser = evt.Guild.GetUser(user.Id);
            string? userEmail = getEmailFromUser(gUser);
            if (userEmail == null) {
                this.Log($"No email registered for user {gUser.Username}");
                DeferService.Instance.QueueIcs(gUser, evt);
                DeferService.Instance.RequestUserEmail(gUser);
                return;
            }
            if (userEmail == "") {
                this.Log($"Email field is empty, {gUser.Username} has opted out");
                return;
            }

            MailService.MailEvent mailEvt = MailService.ConvertSocketGuildEventToMailEvent(evt);
            MailService.Instance.sendEventIcsToUserEmail(userEmail, mailEvt);
        }

        public EventModule OwningModule { get; set; }
    }
}