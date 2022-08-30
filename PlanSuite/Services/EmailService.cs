using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace PlanSuite.Services
{
    public class EmailService : IEmailSender
    {
        public static string SmtpUser { get; private set; }
        public static string SmtpPassword { get; private set; }
        public static string SmtpConfigSet { get; private set; }
        public static string SmtpHost { get; private set; }
        public static int SmtpPort { get; private set; }

        public static string ConfigEmail { get; private set; }
        public static string ConfigName { get; private set; }

        public EmailService()
        {
            
        }

        public static async Task InitEmailService(string user, string pass, string configSet, string host, int port, string configEmail, string configName)
        {
            Console.WriteLine($"Configuring email service...");
            SmtpUser = user;
            SmtpPassword = pass;
            SmtpConfigSet = configSet;
            SmtpHost = host;
            SmtpPort = port;

            ConfigEmail = configEmail;
            ConfigName = configName;

            using (var smtpClient = new SmtpClient())
            {
                try
                {
                    await smtpClient.ConnectAsync(SmtpHost, SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    await smtpClient.AuthenticateAsync(SmtpUser, SmtpPassword);
                    Console.WriteLine($"Email service configured for {user}@{host} on port {port}");
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Exception during InitEmailService: {ex.Message}\n{ex.StackTrace}\n\n---InnerException---\n{ex.InnerException}");
                }
                finally
                {
                    await smtpClient.DisconnectAsync(true);
                }
            }
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress(ConfigName, ConfigEmail));
            mailMessage.To.Add(new MailboxAddress(email, email));
            mailMessage.Subject = subject;
            mailMessage.Body = new TextPart("html")
            {
                Text = htmlMessage
            };

            using (var smtpClient = new SmtpClient())
            {
                try
                {
                    await smtpClient.ConnectAsync(SmtpHost, SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    await smtpClient.AuthenticateAsync(SmtpUser, SmtpPassword);
                    await smtpClient.SendAsync(mailMessage);
                    Console.WriteLine($"Sent email to {mailMessage.To.ElementAt(0).Name} with subject {mailMessage.Subject}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send email: {ex.Message}");
                }
                finally
                {
                    await smtpClient.DisconnectAsync(true);
                    smtpClient.Dispose();
                }
            }
        }
    }
}
