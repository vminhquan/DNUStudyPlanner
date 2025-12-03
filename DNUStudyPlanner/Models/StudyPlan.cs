namespace DNUStudyPlanner.Models
{
    public class StudyPlan
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Notes { get; set; }
        public int Progress { get; set; }
        public int UserId { get; set; }       
        public virtual User User { get; set; }  
    }
}