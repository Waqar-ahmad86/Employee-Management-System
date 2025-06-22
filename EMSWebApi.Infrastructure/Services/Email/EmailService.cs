using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit.Text;
using MimeKit;
using Microsoft.Extensions.Logging;
using EMSWebApi.Application.Interfaces.Infrastructure;

namespace EMSWebApi.Infrastructure.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _emailSettings;

        public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> emailSettings)
        {
            _logger = logger;
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrEmpty(_emailSettings.SmtpHost) || string.IsNullOrEmpty(_emailSettings.SmtpUser))
            {
                _logger.LogError("Email settings (SmtpHost, SmtpUser) are not configured. Email not sent to {Email}.", email);
                _logger.LogWarning($"DUMMY EMAIL (Config Missing): To: {email}, Subject: {subject}, Message: {htmlMessage}");
                return;
            }

            try
            {
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                mimeMessage.To.Add(MailboxAddress.Parse(email));
                mimeMessage.Subject = subject;

                mimeMessage.Body = new TextPart(TextFormat.Html)
                {
                    Text = htmlMessage
                };

                using (var client = new SmtpClient())
                {

                    if (_emailSettings.EnableSsl)
                    {
                        await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, SecureSocketOptions.SslOnConnect);
                    }
                    else
                    {
                        await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, SecureSocketOptions.StartTlsWhenAvailable);
                    }

                    if (!string.IsNullOrEmpty(_emailSettings.SmtpUser) && !string.IsNullOrEmpty(_emailSettings.SmtpPass))
                    {
                        await client.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPass);
                    }

                    await client.SendAsync(mimeMessage);
                    await client.DisconnectAsync(true);
                }
                _logger.LogInformation("Email sent successfully to {Email} with subject {Subject}.", email, subject);
            }
            catch (SmtpCommandException ex)
            {
                _logger.LogError(ex, "SMTP Command Error sending email to {Email}: StatusCode={StatusCode}, Message={SmtpMessage}", email, ex.StatusCode, ex.Message);
                throw;
            }
            catch (SmtpProtocolException ex)
            {
                _logger.LogError(ex, "SMTP Protocol Error sending email to {Email}: Message={SmtpMessage}", email, ex.Message);
                throw;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Socket connection error sending email to {Email} (Host: {SmtpHost}, Port: {SmtpPort})", email, _emailSettings.SmtpHost, _emailSettings.SmtpPort);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while sending email to {Email}.", email);
                throw;
            }
        }
    }
}
