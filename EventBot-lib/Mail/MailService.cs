using System;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Discord.WebSocket;

namespace EventBot.lib.Mail {
    public class MailService {
        private static MailService? _instance;
        public static MailService Get() => _instance ??= new MailService();
        public static MailService Instance => Get();

        private IMailDriver _driver;

        public void setDriver(IMailDriver driver) {
            this._driver = driver;
        }

        public void Send(MailMessage msg) {
            _driver.Send(msg);
        }
        
        private static string BuildICS(MailMessage msg, MailEvent evt) {
            StringBuilder str = new();
            str.AppendLine("BEGIN:VCALENDAR");
            str.AppendLine("PRODID:-//Schedule a Meeting");
            str.AppendLine("VERSION:2.0");
            str.AppendLine("METHOD:REQUEST");
            str.AppendLine("BEGIN:VEVENT");
            str.AppendLine($"DTSTART:{evt.StartTime.UtcDateTime:yyyyMMddTHHmmssZ}");
            str.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
            if(evt.EndTime != null)
                str.AppendLine($"DTEND:{evt.EndTime.Value.UtcDateTime:yyyyMMddTHHmmssZ}");
            str.AppendLine("LOCATION: " + evt.Location);
            str.AppendLine($"UID:{Guid.NewGuid()}");
            str.AppendLine($"DESCRIPTION:{evt.Description}");
            str.AppendLine($"X-ALT-DESC;FMTTYPE=text/html:{evt.Description}");
            str.AppendLine($"SUMMARY:{evt.Name}");
            str.AppendLine($"ORGANIZER:MAILTO:{msg.From.Address}");

            str.AppendLine($"ATTENDEE;CN=\"{msg.To[0].DisplayName}\";RSVP=TRUE:mailto:{msg.To[0].Address}");

            str.AppendLine("BEGIN:VALARM");
            str.AppendLine("TRIGGER:-PT15M");
            str.AppendLine("ACTION:DISPLAY");
            str.AppendLine("DESCRIPTION:Reminder");
            str.AppendLine("END:VALARM");
            str.AppendLine("END:VEVENT");
            str.AppendLine("END:VCALENDAR");
            return str.ToString();
        }
        
        public void sendEventIcsToUserEmail(string userEmail, MailEvent evt) {
            MailMessage msg = new("pterodactyl-nick.guy@outlook.com", userEmail);
            string ics = BuildICS(msg, evt);
            byte[] byteArray = Encoding.ASCII.GetBytes(ics);
            MemoryStream stream = new(byteArray);
            Attachment attach = new(stream,evt.Name + ".ics");
            msg.Attachments.Add(attach);

            ContentType conType = new("text/calendar");
            conType.Parameters.Add("method", "REQUEST");
            AlternateView avCal = AlternateView.CreateAlternateViewFromString(ics, conType);
            msg.AlternateViews.Add(avCal);
            
            Send(msg);
        }

        public struct MailEvent {
            public string Name;
            public string Description;
            public string Location;
            public DateTimeOffset StartTime;
            public DateTimeOffset? EndTime;
        }

        public static MailEvent ConvertSocketGuildEventToMailEvent(SocketGuildEvent socketEvt) {
            return new MailEvent {
                Name = socketEvt.Name,
                Description = socketEvt.Description,
                Location = socketEvt.Location,
                StartTime = socketEvt.StartTime,
                EndTime = socketEvt.EndTime
            };
        }
        
    }
}