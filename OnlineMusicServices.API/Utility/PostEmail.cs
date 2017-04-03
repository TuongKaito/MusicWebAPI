using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace OnlineMusicServices.API.Utility
{
    public class PostEmail
    {
        public static bool Send(string toEmail, string subject, string htmlBody)
        {
            SmtpClient client = new SmtpClient();
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.Host = "smtp.gmail.com";
            client.Port = 587;

            // Setup Smtp authentication
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("tuong.adm13@gmail.com", "programmer");
            client.UseDefaultCredentials = false;
            client.Credentials = credentials;

            // Setup content email
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress("tuong.adm13@gmail.com");
            msg.To.Add(new MailAddress(toEmail));
            msg.Subject = subject;
            msg.IsBodyHtml = true;
            msg.Body = htmlBody;

            try
            {
                client.Send(msg);
                return true;
            }
            catch { }
            return false;
        }
    }
}