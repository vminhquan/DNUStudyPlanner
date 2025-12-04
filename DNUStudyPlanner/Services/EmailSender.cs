using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;
using DNUStudyPlanner.Configuration; 

namespace DNUStudyPlanner.Services;

public class EmailSender : IEmailSender
{
    private readonly SmtpSettings _smtpSettings;
    
    public EmailSender(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var mail = new MailMessage();
        
        mail.From = new MailAddress(_smtpSettings.SmtpMailEmail!, "DNU Study Planner"); 
        mail.To.Add(email);
        mail.Subject = subject;
        mail.Body = message;
        mail.IsBodyHtml = true; 
        
        using (var smtpClient = new SmtpClient(_smtpSettings.SmtpHost!, _smtpSettings.SmtpPort)) 
        {
            smtpClient.Credentials = new NetworkCredential(_smtpSettings.SmtpMailEmail, _smtpSettings.SmtpMailPassword); 
            smtpClient.EnableSsl = true; 
            await smtpClient.SendMailAsync(mail);
        }
    }
}