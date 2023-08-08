using System.Net.Mail;

namespace Calidus.lib.Mail {
    public interface IMailDriver {
        void Send(MailMessage msg);
    }
}