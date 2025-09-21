namespace InternshipManagement.Api.Services
{
    public interface IAppEmailSender
    {
        Task SendEmailAsync(string to, string subject, string message);
    }
}
