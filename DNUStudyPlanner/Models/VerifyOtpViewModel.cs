namespace DNUStudyPlanner.Models;
using System.ComponentModel.DataAnnotations;
public class VerifyOtpViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập mã OTP.")]
    public string Otp { get; set; }
    
    public string Email { get; set; }
}