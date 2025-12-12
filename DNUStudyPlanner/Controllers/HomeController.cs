using Microsoft.AspNetCore.Mvc;
using DNUStudyPlanner.Models;
using DNUStudyPlanner.Data;
using Microsoft.EntityFrameworkCore;

namespace DNUStudyPlanner.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var features = new List<Feature>
            {
                new Feature
                {
                    Title = "Lập kế hoạch linh hoạt",
                    Description = "Tạo kế hoạch học tập phù hợp với bạn.",
                    Image = "lap-ke-hoach.jpg"
                },
                new Feature
                {
                    Title = "Theo dõi tiến độ trực quan",
                    Description = "Biểu đồ và thống kê trực quan giúp bạn theo dõi tiến độ.",
                    Image = "quan-ly-tien-do-du-an.jpg"
                },
                new Feature
                {
                    Title = "Quản lý thời gian hiệu quả",
                    Description = "Sử dụng thời gian hợp lý để đạt mục tiêu.",
                    Image = "ky-nang-quan-ly-thoi-gian.jpg"
                }
            };
            return View(features);
        }

        public IActionResult Dashboard()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new DashboardViewModel
            {
                FullName = user.FullName,
                WeeklyCompletionPercentage = 75,
                WeeklyHoursPercentage = 60,
                OverdueTasksCount = 2
            };
            return View(model);
        }

        public async Task<IActionResult> Plan()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách các kế hoạch của người dùng hiện tại và truyền qua View
            var userPlans = await _context.StudyPlans
                .Where(p => p.UserId == userId)
                .ToListAsync();

            return View(userPlans); // Truyền danh sách kế hoạch vào View
        }
        public IActionResult CreatePlan()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePlan(CreatePlanViewModel model)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                // Nếu không có session, không cho phép tạo
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                // Tạo một đối tượng StudyPlan mới từ dữ liệu của ViewModel
                var newPlan = new StudyPlan
                {
                    Title = model.Title,
                    Notes = model.Notes,
                    Progress = model.Progress,
                    UserId = userId.Value // Gán kế hoạch này cho người dùng đang đăng nhập
                };

                // Thêm vào DbContext và lưu vào database
                _context.StudyPlans.Add(newPlan);
                await _context.SaveChangesAsync();
                
                return RedirectToAction("Plan");
            }

            // Nếu dữ liệu không hợp lệ, hiển thị lại form với các lỗi
            return View(model);
        }
        public async Task<IActionResult> EditPlan(int id)
        {
            var studyPlan = await _context.StudyPlans.FindAsync(id);
            if (studyPlan == null)
            {
                return NotFound();
            }

            // Chuyển dữ liệu từ Model sang ViewModel
            var viewModel = new EditPlanViewModel
            {
                Id = studyPlan.Id,
                UserId = studyPlan.UserId,
                Title = studyPlan.Title,
                Notes = studyPlan.Notes,
                Progress = studyPlan.Progress
            };
            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPlan(int id, EditPlanViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var studyPlanInDb = await _context.StudyPlans.FindAsync(id);
                if (studyPlanInDb == null)
                {
                    return NotFound();
                }
                // Cập nhật các thuộc tính từ ViewModel
                studyPlanInDb.Title = viewModel.Title;
                studyPlanInDb.Notes = viewModel.Notes;
                studyPlanInDb.Progress = viewModel.Progress;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Plan));
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePlan(int id)
        {
            // Tìm kế hoạch theo id
            var studyPlan = await _context.StudyPlans.FindAsync(id);
            if (studyPlan == null)
            {
                return NotFound(); 
            }

            // Xóa kế hoạch khỏi DbContext
            _context.StudyPlans.Remove(studyPlan);
    
            // Lưu thay đổi vào database
            await _context.SaveChangesAsync();

            // Chuyển hướng về trang danh sách kế hoạch
            return RedirectToAction(nameof(Plan));
        }
        public async Task<IActionResult> MySubjects()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            // Lấy thông tin người dùng và các môn học đã đăng ký
            var user = await _context.Users
                .Include(u => u.Subjects)
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user == null) return NotFound();

            // Lấy tất cả môn học trong hệ thống
            var allSubjects = await _context.Subjects.ToListAsync();

            // Tạo ViewModel và gán dữ liệu
            var viewModel = new MySubjectsViewModel
            {
                EnrolledSubjects = user.Subjects.ToList(),
                AllSubjects = allSubjects
            };

            return View(viewModel);
        }


        public IActionResult Profile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int subjectId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var user = await _context.Users.Include(u => u.Subjects).FirstOrDefaultAsync(u => u.Id == userId.Value);
            var subject = await _context.Subjects.FindAsync(subjectId);

            if (user != null && subject != null && !user.Subjects.Any(s => s.Id == subjectId))
            {
                user.Subjects.Add(subject);
                await _context.SaveChangesAsync();
            }
            // Sửa dòng này để chuyển hướng về trang MySubjects
            return RedirectToAction("MySubjects");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unenroll(int subjectId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var user = await _context.Users.Include(u => u.Subjects).FirstOrDefaultAsync(u => u.Id == userId.Value);
            var subject = user?.Subjects.FirstOrDefault(s => s.Id == subjectId);

            if (user != null && subject != null)
            {
                user.Subjects.Remove(subject);
                await _context.SaveChangesAsync();
            }
            // Sửa dòng này để chuyển hướng về trang MySubjects
            return RedirectToAction("MySubjects");
        }

    }
}