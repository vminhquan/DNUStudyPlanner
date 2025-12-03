using System.ComponentModel.DataAnnotations;

namespace DNUStudyPlanner.Models
{
    public class Topic
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } // Tên chương/chủ đề

        // Khóa ngoại để liên kết với Subject
        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; }
    }
}