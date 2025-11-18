using System.Net;
using System.Net.Mail;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];
                var username = _configuration["EmailSettings:Username"];
                var password = _configuration["EmailSettings:Password"];
                var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

                // Nếu không cấu hình email, chỉ log và không gửi
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("Email settings not configured. Email not sent to {Email}", toEmail);
                    _logger.LogInformation("Email content - Subject: {Subject}, Body: {Body}", subject, body);
                    return;
                }

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail ?? "noreply@elegantsuits.com", senderName ?? "Elegant Suits"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", toEmail);
                // Không throw exception để không làm gián đoạn quá trình đăng nhập
            }
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "Chào mừng đến với Elegant Suits!";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>Xin chào {userName}!</h2>
                        <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>Elegant Suits</strong>.</p>
                        <p>Chúng tôi rất vui mừng được chào đón bạn đến với cộng đồng của chúng tôi.</p>
                        <p>Bạn có thể bắt đầu khám phá các sản phẩm vest cao cấp của chúng tôi ngay bây giờ!</p>
                        <div style='margin-top: 30px; padding: 20px; background-color: #f5f5f5; border-radius: 5px;'>
                            <p style='margin: 0;'><strong>Thông tin tài khoản:</strong></p>
                            <p style='margin: 5px 0;'>Email: {toEmail}</p>
                            <p style='margin: 5px 0;'>Tên: {userName}</p>
                        </div>
                        <p style='margin-top: 30px;'>Trân trọng,<br/>Đội ngũ Elegant Suits</p>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendGoogleLoginWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "Đăng nhập thành công với Google - Elegant Suits";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>Xin chào {userName}!</h2>
                        <p>Bạn đã đăng nhập thành công vào <strong>Elegant Suits</strong> bằng tài khoản Google.</p>
                        <div style='margin: 20px 0; padding: 15px; background-color: #e8f5e9; border-left: 4px solid #4caf50; border-radius: 4px;'>
                            <p style='margin: 0; color: #2e7d32;'><strong>✓ Đăng nhập thành công</strong></p>
                            <p style='margin: 5px 0 0 0; color: #555;'>Thời gian: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                        </div>
                        <p>Tài khoản của bạn đã được tạo và liên kết với Google. Bạn có thể sử dụng tài khoản Google để đăng nhập nhanh chóng trong tương lai.</p>
                        <div style='margin-top: 30px; padding: 20px; background-color: #f5f5f5; border-radius: 5px;'>
                            <p style='margin: 0;'><strong>Thông tin tài khoản:</strong></p>
                            <p style='margin: 5px 0;'>Email: {toEmail}</p>
                            <p style='margin: 5px 0;'>Tên: {userName}</p>
                            <p style='margin: 5px 0;'>Phương thức đăng nhập: Google OAuth</p>
                        </div>
                        <div style='margin-top: 20px; padding: 15px; background-color: #fff3cd; border-left: 4px solid #ffc107; border-radius: 4px;'>
                            <p style='margin: 0; color: #856404;'><strong>Lưu ý bảo mật:</strong></p>
                            <p style='margin: 5px 0 0 0; color: #555;'>Nếu bạn không thực hiện đăng nhập này, vui lòng liên hệ với chúng tôi ngay lập tức.</p>
                        </div>
                        <p style='margin-top: 30px;'>Bắt đầu khám phá các sản phẩm vest cao cấp của chúng tôi ngay bây giờ!</p>
                        <p style='margin-top: 30px;'>Trân trọng,<br/>Đội ngũ Elegant Suits</p>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}

