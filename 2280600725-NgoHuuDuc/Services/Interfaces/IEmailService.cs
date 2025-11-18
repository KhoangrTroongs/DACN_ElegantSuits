namespace NgoHuuDuc_2280600725.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendWelcomeEmailAsync(string toEmail, string userName);
        Task SendGoogleLoginWelcomeEmailAsync(string toEmail, string userName);
    }
}

