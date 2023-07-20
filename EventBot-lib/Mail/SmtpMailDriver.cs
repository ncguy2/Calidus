using System.Net;
using System.Net.Mail;

namespace EventBot.lib.Mail {
    public class SmtpMailDriver : IMailDriver {

        private readonly string host;
        private readonly bool enableSsl;
        private readonly string username;
        private readonly string password;

        public SmtpMailDriver(string host, string username, string password, bool enableSsl = true) {
            this.host = host;
            this.username = username;
            this.password = password;
            this.enableSsl = enableSsl;
        }

        public void Send(MailMessage msg) {
            SmtpClient smtp = new();
            smtp.Host = host;
            smtp.EnableSsl = enableSsl;
            smtp.Credentials = new NetworkCredential(username, password);
            smtp.Send(msg);
        }
    }
}