using BacklogApp.Models.Options;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text.RegularExpressions;

namespace BacklogApp.Services
{
    public interface IEmailService
    {
        bool CheckSmtpConnection();
        void Send(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailOptions _opts;

        public EmailService(IOptions<EmailOptions> options)
        {
            _opts = options.Value;
        }

        public bool CheckSmtpConnection()
        {
            try
            {
                using var client = new SmtpClient();
                client.Connect(_opts.Host, _opts.SmtpPort);
                client.Authenticate(_opts.Username, _opts.Password);
                client.Disconnect(true);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void Send(string to, string subject, string body)
        {
            if(!IsEmailValid(to)) return;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_opts.Sender, _opts.Sender));
            message.To.Add(new MailboxAddress(to, to));

            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            client.Connect(_opts.Host, _opts.SmtpPort);
            client.Authenticate(_opts.Username, _opts.Password);

            client.Send(message);

            client.Disconnect(true);
        }

        private const string validEmailPattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";
        public static bool IsEmailValid(string email)
        {
            return Regex.IsMatch(email, validEmailPattern, RegexOptions.IgnoreCase);
        }
    }
}
