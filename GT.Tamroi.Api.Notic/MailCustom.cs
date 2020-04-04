using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace GT.Tamroi.Api.Notic
{
    public class MailCustom
    {
        private string sender_string;
        private string reciever_string;
        private string password_mail;
        private string mail_server = @"smtp.office365.com";//@"smtp.office365.com" @"smtp.gmail.com";    
        private int mail_port = 587;
        public MailCustom()
        {

        }

        public MailCustom(string sender, string recieve, string password_mail)
        {
            this.sender_string = sender;
            this.reciever_string = recieve;
            this.password_mail = password_mail;
        }

        public void sendMail(string topic, string message)
        {
            //public bool SendMail(string sender, string receiver, string mail_server, string mail_pw, string subject, string text_massage, int mail_port = 587)

            try
            {
                string[] to = reciever_string.Split(';');
                string from = sender_string;
                MailMessage message_mail = new MailMessage(from, to[0]);
                for (int to_reciever = 1; to_reciever < to.Length; to_reciever++)
                {
                    message_mail.To.Add(to[to_reciever]);
                }
                message_mail.Subject = topic;
                message_mail.Body = message;

                //message.Body = @"Using this new feature, you can send an e-mail message from an application very easily.";

                SmtpClient client = new SmtpClient(mail_server);

                // Credentials are necessary if the server requires the client  
                // to authenticate before it will send e-mail on the client's behalf.
                // client.UseDefaultCredentials = true;

                client.UseDefaultCredentials = true;
                client.EnableSsl = true;
                client.Port = mail_port;
                client.Timeout = 60 * 1 * 1000;
                NetworkCredential crdntl = new NetworkCredential(sender_string, password_mail);
                //NetworkCredential crdntl = new NetworkCredential("nutthapaul.a@gmail.com", "30091989");

                client.Credentials = crdntl;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Send(message_mail);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        public void setSeder(string sender)
        {
            this.sender_string = sender;
        }

        public void setReciever(string reviever)
        {
            this.reciever_string = reviever;
        }

        public void setPasswoedMail(string password_mail)
        {
            this.password_mail = password_mail;
        }

        public void setMailServer(string mail_server)
        {
            this.mail_server = mail_server;
        }

        public string getSeder()
        {
            return sender_string;
        }

        public string getReciever()
        {
            return reciever_string;
        }

        public string getPasswoedMail()
        {
            return password_mail;
        }

        public string getMailServer()
        {
            return mail_server;
        }
    }
}