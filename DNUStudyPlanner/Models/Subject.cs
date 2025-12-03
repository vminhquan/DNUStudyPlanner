using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DNUStudyPlanner.Models
{
    public class Subject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã môn học là bắt buộc")]
        public string SubjectCode { get; set; }

        [Required(ErrorMessage = "Tên môn học là bắt buộc")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Số tín chỉ là bắt buộc")]
        [Range(1, 10, ErrorMessage = "Số tín chỉ không hợp lệ")]
        public int Credits { get; set; }

        // Mối quan hệ: Một môn học có nhiều chủ đề
        public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
        public string Instructor { get; set; }
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}