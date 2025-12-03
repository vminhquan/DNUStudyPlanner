using Microsoft.EntityFrameworkCore;
using DNUStudyPlanner.Models;

namespace DNUStudyPlanner.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<StudyPlan> StudyPlans { get; set; }
        public DbSet<Topic> Topics { get; set; }
    }
}