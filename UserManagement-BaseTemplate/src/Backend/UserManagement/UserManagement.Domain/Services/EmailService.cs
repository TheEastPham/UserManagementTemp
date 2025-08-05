using UserManagement.Domain.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace UserManagement.Domain.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendEmailVerificationAsync(string email, string firstName, string verificationToken)
    {
        try
        {
            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
            var verificationUrl = $"{baseUrl}/api/auth/verify-email?token={verificationToken}&email={Uri.EscapeDataString(email)}";

            var subject = "XÃ¡c thá»±c tÃ i khoáº£n cá»§a báº¡n";
            var body = GetEmailVerificationTemplate(firstName, verificationUrl, verificationToken);

            return await SendEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email verification to {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendPasswordResetAsync(string email, string firstName, string resetToken)
    {
        try
        {
            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
            var resetUrl = $"{baseUrl}/api/auth/reset-password?token={resetToken}&email={Uri.EscapeDataString(email)}";

            var subject = "Äáº·t láº¡i máº­t kháº©u";
            var body = GetPasswordResetTemplate(firstName, resetUrl, resetToken);

            return await SendEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string email, string firstName)
    {
        try
        {
            var subject = "ChÃ o má»«ng báº¡n Ä‘áº¿n vá»›i há»‡ thá»‘ng!";
            var body = GetWelcomeEmailTemplate(firstName);

            return await SendEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome email to {Email}", email);
            return false;
        }
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var smtpHost = _configuration["EmailSettings:SmtpHost"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var fromName = _configuration["EmailSettings:FromName"] ?? "UserManagement System";

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogWarning("Email settings not configured. Email will not be sent.");
                return false;
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail ?? smtpUsername, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            return false;
        }
    }

    private string GetEmailVerificationTemplate(string firstName, string verificationUrl, string verificationCode)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>XÃ¡c thá»±c tÃ i khoáº£n</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background: #f9f9f9; }}
        .verification-code {{ font-size: 24px; font-weight: bold; color: #007bff; text-align: center; padding: 15px; background: white; border: 2px dashed #007bff; margin: 20px 0; }}
        .button {{ display: inline-block; padding: 12px 30px; background: #007bff; color: white; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>XÃ¡c thá»±c tÃ i khoáº£n</h1>
        </div>
        <div class='content'>
            <h2>Xin chÃ o {firstName}!</h2>
            <p>Cáº£m Æ¡n báº¡n Ä‘Ã£ Ä‘Äƒng kÃ½ tÃ i khoáº£n. Äá»ƒ hoÃ n táº¥t quÃ¡ trÃ¬nh Ä‘Äƒng kÃ½, vui lÃ²ng xÃ¡c thá»±c email cá»§a báº¡n.</p>
            
            <div class='verification-code'>
                MÃ£ xÃ¡c thá»±c: {verificationCode}
            </div>
            
            <p>Báº¡n cÃ³ thá»ƒ click vÃ o nÃºt bÃªn dÆ°á»›i Ä‘á»ƒ xÃ¡c thá»±c ngay:</p>
            <p style='text-align: center;'>
                <a href='{verificationUrl}' class='button'>XÃ¡c thá»±c tÃ i khoáº£n</a>
            </p>
            
            <p>Hoáº·c copy vÃ  paste Ä‘Æ°á»ng link sau vÃ o trÃ¬nh duyá»‡t:</p>
            <p style='word-break: break-all; background: #f0f0f0; padding: 10px;'>{verificationUrl}</p>
            
            <p><strong>LÆ°u Ã½:</strong> Link xÃ¡c thá»±c sáº½ háº¿t háº¡n sau 30 phÃºt.</p>
            
            <p>Náº¿u báº¡n khÃ´ng táº¡o tÃ i khoáº£n nÃ y, vui lÃ²ng bá» qua email nÃ y.</p>
        </div>
        <div class='footer'>
            <p>Â© 2025 UserManagement System. Táº¥t cáº£ quyá»n Ä‘Æ°á»£c báº£o lÆ°u.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetPasswordResetTemplate(string firstName, string resetUrl, string resetToken)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Äáº·t láº¡i máº­t kháº©u</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #dc3545; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 30px; background: #dc3545; color: white; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Äáº·t láº¡i máº­t kháº©u</h1>
        </div>
        <div class='content'>
            <h2>Xin chÃ o {firstName}!</h2>
            <p>ChÃºng tÃ´i nháº­n Ä‘Æ°á»£c yÃªu cáº§u Ä‘áº·t láº¡i máº­t kháº©u cho tÃ i khoáº£n cá»§a báº¡n.</p>
            
            <p>Click vÃ o nÃºt bÃªn dÆ°á»›i Ä‘á»ƒ Ä‘áº·t láº¡i máº­t kháº©u:</p>
            <p style='text-align: center;'>
                <a href='{resetUrl}' class='button'>Äáº·t láº¡i máº­t kháº©u</a>
            </p>
            
            <p>MÃ£ Ä‘áº·t láº¡i: <strong>{resetToken}</strong></p>
            
            <p><strong>LÆ°u Ã½:</strong> Link nÃ y sáº½ háº¿t háº¡n sau 30 phÃºt.</p>
            
            <p>Náº¿u báº¡n khÃ´ng yÃªu cáº§u Ä‘áº·t láº¡i máº­t kháº©u, vui lÃ²ng bá» qua email nÃ y.</p>
        </div>
        <div class='footer'>
            <p>Â© 2025 UserManagement System. Táº¥t cáº£ quyá»n Ä‘Æ°á»£c báº£o lÆ°u.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetWelcomeEmailTemplate(string firstName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>ChÃ o má»«ng</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #28a745; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background: #f9f9f9; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>ChÃ o má»«ng!</h1>
        </div>
        <div class='content'>
            <h2>Xin chÃ o {firstName}!</h2>
            <p>ChÃ o má»«ng báº¡n Ä‘áº¿n vá»›i há»‡ thá»‘ng quáº£n lÃ½ ngÆ°á»i dÃ¹ng cá»§a chÃºng tÃ´i!</p>
            <p>TÃ i khoáº£n cá»§a báº¡n Ä‘Ã£ Ä‘Æ°á»£c kÃ­ch hoáº¡t thÃ nh cÃ´ng. Báº¡n cÃ³ thá»ƒ báº¯t Ä‘áº§u sá»­ dá»¥ng cÃ¡c tÃ­nh nÄƒng cá»§a há»‡ thá»‘ng.</p>
            <p>Náº¿u báº¡n cÃ³ báº¥t ká»³ cÃ¢u há»i nÃ o, Ä‘á»«ng ngáº§n ngáº¡i liÃªn há»‡ vá»›i chÃºng tÃ´i.</p>
            <p>ChÃºc báº¡n cÃ³ tráº£i nghiá»‡m tuyá»‡t vá»i!</p>
        </div>
        <div class='footer'>
            <p>Â© 2025 UserManagement System. Táº¥t cáº£ quyá»n Ä‘Æ°á»£c báº£o lÆ°u.</p>
        </div>
    </div>
</body>
</html>";
    }
}
