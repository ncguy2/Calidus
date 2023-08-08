using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Calidus.lib.Data;
using Calidus.lib.Data.Types;
using Calidus.lib.Mail;

namespace Calidus.lib.Defer {
    public class DeferService {
        private static DeferService? _instance;
        
        public const string REGISTER_EMAIL_KEY = "action.register_email";
        public const string OPT_OUT_KEY = "action.opt_out";

        public static DeferService Get() {
            return _instance ??= new DeferService();
        }

        public static DeferService Instance => Get();
        private readonly DatabaseFacade<QueuedIcs> icsQueue;
        private DiscordSocketClient? client;

        private DeferService() {
            icsQueue = new DatabaseFacade<QueuedIcs>();
            DatabaseService.Instance.RegisterOnInsertEvent<UserEmail>(OnUserEmailInsert);
        }

        public void setDiscordSocketClient(DiscordSocketClient client) {
            this.client = client;
        }

        public void OnUserEmailInsert(UserEmail userEmail) {
            List<QueuedIcs> queuedIcsList = icsQueue.Query.Where(x => x.userId == userEmail.discordId).ToList();
            if (queuedIcsList.Count == 0)
                return;
            
            foreach (QueuedIcs queuedIcs in queuedIcsList) 
                handleQueuedIcs(userEmail, queuedIcs);
        }

        public void QueueIcs(SocketGuildUser user, SocketGuildEvent evt) {
            QueuedIcs queuedIcs = icsQueue.createNew();
            queuedIcs.userId = user.Id;
            queuedIcs.eventId = evt.Id;
            queuedIcs.guildId = evt.GuildId;
            icsQueue.Insert(queuedIcs);
        }

        public void handleQueuedIcs(UserEmail userEmail, QueuedIcs queuedIcs) {
            SocketGuildEvent guildEvent = client!.GetGuild(queuedIcs.guildId).GetEvent(queuedIcs.eventId);
            MailService.MailEvent mailEvent = MailService.ConvertSocketGuildEventToMailEvent(guildEvent);
            MailService.Instance.sendEventIcsToUserEmail(userEmail.email!, mailEvent);
            
            icsQueue.Delete(queuedIcs);
        }

        public void RequestUserEmail(SocketGuildUser user) {
            EmbedBuilder eb = new EmbedBuilder()
                              .WithTitle("Email required for event calendar invites")
                              .WithDescription(
                                  "EmbedBot requires you to provide your email before it can send you calendar invites.");

            ComponentBuilder cb = new ComponentBuilder()
                .AddRow(new ActionRowBuilder()
                        .WithButton("Register Email", REGISTER_EMAIL_KEY, style: ButtonStyle.Primary)
                        .WithButton("Opt out", OPT_OUT_KEY, ButtonStyle.Danger));

            user.SendMessageAsync("Email required", embed: eb.Build(), components: cb.Build());
        }
    }

    public class QueuedIcs {
        [DBColumn("primary_key", DBColumnType.LONG_INTEGER_UNSIGNED, true, true)]
        public ulong primaryKey;

        [DBColumn("user_id", DBColumnType.LONG_INTEGER_UNSIGNED)]
        public ulong userId;

        [DBColumn("event_id", DBColumnType.LONG_INTEGER_UNSIGNED)]
        public ulong eventId;

        [DBColumn("guild_id", DBColumnType.LONG_INTEGER_UNSIGNED)]
        public ulong guildId;
    }
}