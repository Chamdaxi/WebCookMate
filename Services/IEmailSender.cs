using System;

namespace demo.Services
{
    public interface IEmailSender
    {
        void SendEmail(string toEmail, string subject, string htmlBody);
    }
}



