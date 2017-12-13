using System;
using System.Net;
using System.Net.Mail;

namespace blockchain_parser.Utils
{
    public class Email
    {
        private string subject;
        private string message;
        private string to;

        public Email(string subject, string message, string to) {
            this.subject = subject;
            this.message = message;
            this.to = to;
        }
        public bool Send() {
            
            var result = true;
            SmtpClient client = new SmtpClient(AppConfig.SmtpHost, AppConfig.SmtpPort);
            client.UseDefaultCredentials = false;
            client.EnableSsl = AppConfig.SmtpEncryption;
            client.Credentials = new NetworkCredential(AppConfig.SmtpUsername, AppConfig.SmtpPassword);
 
            var from = AppConfig.SmtpFrom;
            MailMessage mailMessage = new MailMessage();
         

            mailMessage.From = new MailAddress(from.Item1, from.Item2);
            mailMessage.To.Add(to);
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = message;
            mailMessage.Subject = subject;

            try
           {
               client.Send(mailMessage);
           }
           catch(Exception ex) {
               Logger.LogStatus(ConsoleColor.Red, ex.ToString());
               result = false;
           }

            mailMessage.Dispose();
            client.Dispose();
            return result;
        }
    }
}