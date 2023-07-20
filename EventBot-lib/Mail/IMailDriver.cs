using System.Net.Mail;

namespace EventBot.lib.Mail {
    public interface IMailDriver {
        void Send(MailMessage msg);
    }
}