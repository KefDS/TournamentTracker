using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace TrackerLibrary
{
    class EmailLogic
    {
        public static void SendEmail(List<string> to, List<string> bcc, string subject, string body)
        {
            MailAddress fromMailAddress = new(GlobalConfig.AppKeyLookup("senderEmail"), GlobalConfig.AppKeyLookup("senderDisplayName"));

            MailMessage mail = new();
            to.ForEach(x => mail.To.Add(x));
            to.ForEach(x => mail.Bcc.Add(x));
            mail.From = fromMailAddress;
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            SmtpClient client = new("127.0.0.1", 25);
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("test", "test");
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = false;

            client.Send(mail);
        }

        public static void SendEmail(string to, string subject, string body)
        {
            SendEmail(new List<string> { to }, new List<string> { }, subject, body);
        }
    }
}
