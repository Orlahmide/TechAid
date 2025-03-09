using MailKit.Net.Smtp;
using MimeKit;
using TechAid.Interface;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void SendEmail(string toEmail, string subject, string messageBody)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Support", emailSettings["SenderEmail"]));
        email.To.Add(new MailboxAddress("", toEmail));
        email.Subject = subject;

        email.Body = new TextPart("html")
        {
            Text = messageBody
        };

        using var smtp = new SmtpClient();
        smtp.Connect(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]), bool.Parse(emailSettings["EnableSSL"]));
        smtp.Authenticate(emailSettings["SenderEmail"], emailSettings["SenderPassword"]);
        smtp.Send(email);
        smtp.Disconnect(true);
    }
}
