using System;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace demo.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(string toEmail, string subject, string htmlBody)
        {
            var section = _configuration.GetSection("Email");
            var smtpServer = section.GetValue<string>("SmtpServer");
            var smtpPort = section.GetValue<int>("SmtpPort");
            var useStartTls = section.GetValue<bool>("UseStartTls");
            var senderEmail = section.GetValue<string>("SenderEmail");
            var senderName = section.GetValue<string>("SenderName") ?? "CookMate";
            var username = section.GetValue<string>("Username") ?? senderEmail;
            var password = section.GetValue<string>("Password");

            if (string.IsNullOrWhiteSpace(smtpServer) || smtpPort == 0 || string.IsNullOrWhiteSpace(senderEmail))
            {
                throw new InvalidOperationException("Email configuration is missing required fields.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody, TextBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            client.Connect(smtpServer, smtpPort, useStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
            if (!string.IsNullOrEmpty(username))
            {
                client.Authenticate(username, password);
            }
            client.Send(message);
            client.Disconnect(true);
        }
    }
}



