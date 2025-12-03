using Microsoft.AspNetCore.Mvc;
using DNUStudyPlanner.Data;
using DNUStudyPlanner.Models;
using DNUStudyPlanner.Services;
using Microsoft.EntityFrameworkCore;


namespace DNUStudyPlanner.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        
        public AccountController(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }
        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken] // biện pháp bảo mật quan trọng của ASP.NET Core, giúp ngăn chặn tấn công Cross-Site Request Forgery (CSRF)
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid) // Kiểm tra xem dữ liệu trong model có hợp lệ theo các Data Annotations (như [Required],
                                    // [EmailAddress]) đã được định nghĩa trong RegisterViewModel hay không
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                // Kiểm tra xem đã có người dùng nào sử dụng Email này chưa
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model);
                }

                var user = new User
                {
                    Email = model.Email,
                    Password = model.Password, 
                    FullName = model.FullName,
                    DateOfBirth = model.DateOfBirth,
                    Major = model.Major,
                    Role = "User"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // Thực thi lệnh INSERT vào cơ sở dữ liệu

                return RedirectToAction("Login");
            }
            return View(model);
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                // THAY ĐỔI 2: So sánh mật khẩu trực tiếp, không dùng Verify
                if (user != null && user.Password == model.Password)
                {
                    // Lưu ID người dùng vào Session để duy trì trạng thái đăng nhập.
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    // Chuyển hướng đến trang Dashboard
                    return RedirectToAction("Dashboard", "Home");
                }

                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            }

            return View(model);
        }
        
        // Inject dịch vụ email

       
        // GET: /Account/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }
        // POST: /Account/ForgotPassword (Xử lý yêu cầu gửi OTP)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            
                // **Luôn trả về thông báo thành công giả** để tránh tiết lộ email nào tồn tại.
                if (user != null)
                {
                    /// 1. Tạo và Lưu Mã OTP (như đã giải thích ở mục 1)
                    string otp = new Random().Next(100000, 999999).ToString(); 
                    user.OtpCode = otp;
                    user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(5);
                    await _context.SaveChangesAsync();
            
                    // 2. Gửi Email
                    string messageBody = $"<h3>Mã Đặt lại Mật khẩu</h3>" +
                                         $"<p>Chào bạn {user.FullName},</p>" +
                                         $"<p>Mã OTP của bạn là: <b>{otp}</b></p>" +
                                         $"<p>Vui lòng nhập mã này vào trang xác minh. Mã sẽ hết hạn sau 5 phút.</p>";

                    await _emailSender.SendEmailAsync(user.Email, "Mã Xác minh Đặt lại Mật khẩu", messageBody); 

                    // 3. Chuyển hướng đến trang xác minh
                    return RedirectToAction("VerifyOtp", new { email = user.Email });
                }
                // Trả về thông báo thành công giả để không tiết lộ email tồn tại
                return RedirectToAction("VerifyOtp", new { email = model.Email });
            }
            return View(model);
        }
        public IActionResult VerifyOtp(string email)
        {
            // Truyền email ẩn đi để form POST có thể dùng lại
            var model = new VerifyOtpViewModel { Email = email }; 
            return View(model);
        }

    // POST: /Account/VerifyOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null && user.OtpCode == model.Otp && user.OtpExpiryTime > DateTime.UtcNow)
                {
                    user.OtpCode = null; // Vô hiệu hóa OTP sau khi dùng
                    user.OtpExpiryTime = null; 
                    await _context.SaveChangesAsync(); 
            
                    // Lưu email vào Session với một khóa an toàn (ResetToken)
                    // hoặc chuyển hướng kèm theo một token reset được mã hóa.
                    HttpContext.Session.SetString("ResetTokenEmail", model.Email);
            
                    // Chuyển hướng đến trang đặt lại mật khẩu
                    return RedirectToAction("ResetPassword");
                }

                ModelState.AddModelError(string.Empty, "Mã OTP không hợp lệ hoặc đã hết hạn.");
            }
            return View(model);
        }
        // GET: /Account/ResendOtp?email=user@example.com
        public async Task<IActionResult> ResendOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword"); // Nếu không có email, quay lại yêu cầu
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    
            if (user != null)
            {
                // 1. TẠO MÃ OTP MỚI VÀ LƯU VÀO DB (Copy logic từ ForgotPassword/POST)
                string otp = new Random().Next(100000, 999999).ToString(); 
                user.OtpCode = otp;
                user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(5);
                await _context.SaveChangesAsync();

                // 2. GỬI EMAIL MỚI
                string messageBody = $"<h3>Mã Đặt lại Mật khẩu</h3>" +
                                     $"<p>Mã OTP MỚI của bạn là: <b>{otp}</b></p>";
                await _emailSender.SendEmailAsync(user.Email, "Mã Xác minh MỚI", messageBody);

                // 3. Chuyển hướng lại trang xác minh
                TempData["SuccessMessage"] = "Mã xác minh mới đã được gửi đến email của bạn.";
                return RedirectToAction("VerifyOtp", new { email = user.Email });
            }

            // Nếu không tìm thấy user (tránh tiết lộ thông tin)
            return RedirectToAction("VerifyOtp", new { email = email });
        }
        // GET: /Account/ResetPassword
        public IActionResult ResetPassword()
        {
            // Kiểm tra session token để đảm bảo người dùng đã qua bước VerifyOtp
            if (HttpContext.Session.GetString("ResetTokenEmail") == null)
            {
                return RedirectToAction("ForgotPassword");
            }
            return View();
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            string resetEmail = HttpContext.Session.GetString("ResetTokenEmail");
    
            // Kiểm tra token và dữ liệu hợp lệ
            if (resetEmail == null || !ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetEmail);

            if (user != null)
            {
                // 1. Cập nhật Mật khẩu (Quan trọng: Trong thực tế, bạn phải HASH mật khẩu)
                // Dựa trên code cũ của bạn (so sánh trực tiếp), ta gán thẳng:
                user.Password = model.NewPassword; 
        
                // 2. Vô hiệu hóa token/session
                HttpContext.Session.Remove("ResetTokenEmail");

                await _context.SaveChangesAsync();

                // Chuyển hướng về trang đăng nhập với thông báo thành công
                TempData["SuccessMessage"] = "Mật khẩu của bạn đã được đặt lại thành công. Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            // Nếu không tìm thấy user (rất hiếm khi xảy ra ở đây), chuyển hướng về login
            return RedirectToAction("Login");
        }
        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa tất cả dữ liệu được lưu trong Session (bao gồm cả UserId).
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId"); // Dữ nguyên trạng thái đăng nhập của người dùng
                                                                         // lấy giá trị số nguyên (ID người dùng) được lưu trữ với khóa là "UserId".
            if (userId == null)
            {
                return RedirectToAction("Login"); // Tải lại trang Login
            }

            var user = await _context.Users.FindAsync(userId.Value); // Lấy thông tin người dùng từ DB.
            if (user == null)
            {
                return NotFound();
            }
            
            // Chuyển dữ liệu từ User model sang ViewModel
            var viewModel = new EditProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                DateOfBirth = user.DateOfBirth,
                Major = user.Major
            };

            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel viewModel)
        
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId.Value != viewModel.Id)
            {
                return BadRequest("Bạn không có quyền thực hiện hành động này.");
            }

            var userInDb = await _context.Users.FindAsync(userId.Value); // Lấy thông tin người dùng từ DB.
            if (userInDb == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin từ ViewModel vào đối tượng User trong DB
            userInDb.FullName = viewModel.FullName;
            userInDb.DateOfBirth = viewModel.DateOfBirth;
            userInDb.Major = viewModel.Major;

            await _context.SaveChangesAsync();

            // Chuyển hướng về trang Profile trong HomeController
            return RedirectToAction("Profile", "Home");
        }
    }
}