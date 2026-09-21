using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace UTHMLibrary.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendVerificationCodeAsync(string email, string code, string userName)
    {
        var subject = "🔐 UTHM Library - Email Verification Code";
        var body = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f0f2f5; padding: 20px; }}
                    .container {{ max-width: 500px; margin: 0 auto; background: white; border-radius: 16px; padding: 30px; box-shadow: 0 4px 20px rgba(0,0,0,0.08); }}
                    .header {{ text-align: center; border-bottom: 2px solid #1a237e; padding-bottom: 15px; margin-bottom: 20px; }}
                    .header h1 {{ color: #1a237e; margin: 0; }}
                    .code {{ font-size: 36px; font-weight: bold; color: #1a237e; text-align: center; padding: 20px; background: #e8eaf6; border-radius: 10px; letter-spacing: 8px; margin: 20px 0; }}
                    .footer {{ text-align: center; color: #666; font-size: 12px; border-top: 1px solid #e0e0e0; padding-top: 15px; margin-top: 20px; }}
                    .expiry {{ color: #e65100; font-weight: bold; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>📚 UTHM Library</h1>
                        <p style='color: #666;'>Email Verification</p>
                    </div>
                    <p>Hello <strong>{userName}</strong>,</p>
                    <p>Thank you for registering with UTHM Library Smart Monitoring System.</p>
                    <p>Please use the verification code below to complete your registration:</p>
                    <div class='code'>{code}</div>
                    <p>This code will expire in <span class='expiry'>15 minutes</span>.</p>
                    <p style='color: #666; font-size: 14px;'>If you did not request this, please ignore this email.</p>
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} UTHM Library - Perpustakaan Tunku Tun Aminah</p>
                        <p>Universiti Tun Hussein Onn Malaysia</p>
                    </div>
                </div>
            </body>
            </html>
        ";

        return await SendEmailAsync(email, subject, body);
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration["EmailSettings:SenderEmail"]));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _configuration["EmailSettings:SmtpServer"],
                int.Parse(_configuration["EmailSettings:SmtpPort"]),
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                _configuration["EmailSettings:SenderEmail"],
                _configuration["EmailSettings:SenderPassword"]
            );

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation($"✅ Email sent to {to}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Failed to send email: {ex.Message}");
            return false;
        }
    }
}