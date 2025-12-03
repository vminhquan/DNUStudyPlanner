// DNUStudyPlanner/Controllers/SubjectAdminController.cs
using DNUStudyPlanner.Data;
using DNUStudyPlanner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DNUStudyPlanner.Controllers
{
    public class SubjectAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubjectAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("UserRole") == "Admin";

        // GET: /SubjectAdmin
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Admin");
            return View(await _context.Subjects.ToListAsync());
        }

        // GET: /SubjectAdmin/Create
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Admin");
            return View();
        }

        // POST: /SubjectAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subject subject)
        {
            if (!IsAdmin()) return Forbid();
            if (ModelState.IsValid)
            {
                _context.Add(subject);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subject);
        }

        // GET: /SubjectAdmin/Edit/5
        // Trang Edit sẽ là nơi quản lý cả chủ đề (Topic)
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Admin");
            if (id == null) return NotFound();

            var subject = await _context.Subjects
                .Include(s => s.Topics) // Quan trọng: Lấy cả các chủ đề liên quan
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (subject == null) return NotFound();
            return View(subject);
        }

        // POST: /SubjectAdmin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Subject subject)
        {
            if (!IsAdmin()) return Forbid();
            if (id != subject.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(subject);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subject);
        }
        
        // POST: /SubjectAdmin/AddTopic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTopic(int subjectId, string topicTitle)
        {
            if (!IsAdmin()) return Forbid();
            if (!string.IsNullOrEmpty(topicTitle))
            {
                var topic = new Topic { Title = topicTitle, SubjectId = subjectId };
                _context.Topics.Add(topic);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Edit", new { id = subjectId });
        }

        // Tương tự, bạn có thể thêm các action để Sửa/Xóa Topic
    }
}