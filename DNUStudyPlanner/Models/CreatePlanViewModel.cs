using System.ComponentModel.DataAnnotations;

namespace DNUStudyPlanner.Models
{
    public class CreatePlanViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        public string Title { get; set; }

        public string Notes { get; set; }

        [Range(0, 100, ErrorMessage = "Tiến độ phải từ 0 đến 100")]
        public int Progress { get; set; }
    }
}