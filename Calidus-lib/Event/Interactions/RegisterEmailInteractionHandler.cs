using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Calidus.lib.Data;
using Calidus.lib.Data.Types;
using Calidus.lib.Defer;
using Calidus.lib.Modules;

namespace Calidus.lib.Event.Interactions {
    public class RegisterEmailInteractionHandler : EventHandler, IModuleAttachment<EventModule> {
        public void Register(DiscordSocketClient client) {
            client.ButtonExecuted += onButtonExecutedFunc(DeferService.REGISTER_EMAIL_KEY, onRegisterEmailClicked);
            client.ButtonExecuted += onButtonExecutedFunc(DeferService.OPT_OUT_KEY, onOptOutClicked);
            
            client.ModalSubmitted += onModalConfirmedFunc(DeferService.REGISTER_EMAIL_KEY + ".modal", registerEmailSubmitted);
            client.ModalSubmitted += onModalConfirmedFunc(DeferService.OPT_OUT_KEY + ".modal", registerOptOut);
        }

        private async Task registerEmailSubmitted(SocketModal arg) {
            DatabaseFacade<UserEmail> facade = new();
            UserEmail userEmail = facade.createNew();
            userEmail.discordId = arg.User.Id;
            userEmail.email = arg.Data.Components.First(x => x.CustomId == DeferService.REGISTER_EMAIL_KEY + ".email")
                                 .Value;
            facade.Insert(userEmail);
            await arg.RespondAsync("Email registered, thanks!");
        }

        private async Task registerOptOut(SocketModal arg) {
            DatabaseFacade<UserEmail> facade = new();
            UserEmail userEmail = facade.createNew();
            userEmail.discordId = arg.User.Id;
            userEmail.email = "";
            facade.Insert(userEmail);
            await arg.RespondAsync("You have now opted out, you will no longer be asked to register your email when signing up for events.");
        }
        
        private Func<SocketModal, Task> onModalConfirmedFunc(
            string modalId, Func<SocketModal, Task> predFunc) {
            return msg => msg.Data.CustomId == modalId ? predFunc(msg) : Task.CompletedTask;
        }

        private Func<SocketMessageComponent, Task> onButtonExecutedFunc(
            string btnId, Func<SocketMessageComponent, Task> predFunc) {
            return msg => msg.Data.CustomId == btnId ? predFunc(msg) : Task.CompletedTask;
        }

        private async Task onRegisterEmailClicked(SocketMessageComponent arg) {
            Modal mb = new ModalBuilder().WithTitle("Email registration")
                                         .WithCustomId(DeferService.REGISTER_EMAIL_KEY + ".modal")
                                                .AddTextInput("Email address",
                                                              DeferService.REGISTER_EMAIL_KEY + ".email",
                                                              placeholder: "Email address")
                                                .Build();

            await arg.RespondWithModalAsync(mb);
            await arg.DeleteOriginalResponseAsync();
        }

        private async Task onOptOutClicked(SocketMessageComponent arg) {
            Modal mb = new ModalBuilder().WithTitle("Are you sure you want to opt out? This operation currently cannot be reversed through the bot")
                                         .WithCustomId(DeferService.OPT_OUT_KEY + ".modal")
                                         .Build();

            await arg.RespondWithModalAsync(mb);
            await arg.DeleteOriginalResponseAsync();
        }

        public EventModule OwningModule { get; set; } = null!;
    }
}