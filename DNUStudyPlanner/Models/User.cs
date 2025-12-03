using System.ComponentModel.DataAnnotations;

namespace DNUStudyPlanner.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }  

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        public string Major { get; set; }
        public string Role { get; set; }
        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
        
        public string? OtpCode { get; set; }
        
        public DateTime? OtpExpiryTime { get; set; }

    }
}