namespace DNUStudyPlanner.Services;
using System.Net.Mail;
using System.Net;
public class EmailSender : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var mail = new MailMessage();
        mail.From = new MailAddress("quanrip113@gmail.com", "DNU Study Planner"); // Địa chỉ email gửi đi
        mail.To.Add(email);
        mail.Subject = subject;
        mail.Body = message;
        mail.IsBodyHtml = true; // Cho phép nội dung email có định dạng HTML

        using (var smtpClient = new SmtpClient("smtp.gmail.com", 587)) // Thay bằng host và port của bạn
        {
            smtpClient.Credentials = new NetworkCredential("quanrip113@gmail.com", "fbioklatwckerbec"); 
            smtpClient.EnableSsl = true; 
            await smtpClient.SendMailAsync(mail);
        }
    }
}