namespace DNUStudyPlanner.Models
{
    public class DashboardViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int WeeklyCompletionPercentage { get; set; }
        public int WeeklyHoursPercentage { get; set; }
        public int OverdueTasksCount { get; set; }
    }
}