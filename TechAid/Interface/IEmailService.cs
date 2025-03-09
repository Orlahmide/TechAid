namespace TechAid.Interface
{
    public interface IEmailService
    {
        void SendEmail(string toEmail, string subject, string messageBody);
    }
}
