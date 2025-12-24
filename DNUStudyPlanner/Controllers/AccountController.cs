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
            if (ModelState.IsValid) // Kiểm tra xem dữ liệu trong model có hợp lệ theo các Data Annotations.
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
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

        // GET: /Account/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }
       // POST: /Account/ForgotPassword
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
{
    if (ModelState.IsValid)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
    
        // Trả về thông báo thành công giả để tránh tiết lộ email nào tồn tại.
        if (user != null)
        {
            /// 1. Tạo và Lưu Mã OTP (Vẫn phải chờ lưu DB xong mới được đi tiếp)
            string otp = new Random().Next(100000, 999999).ToString(); 
            user.OtpCode = otp;
            user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(5);
            await _context.SaveChangesAsync();
    
            // 2. Gửi Email
            string messageBody = $"<h3>Mã Đặt lại Mật khẩu</h3>" +
                                    $"<p>Chào bạn {user.FullName},</p>" +
                                    $"<p>Mã OTP của bạn là: <b>{otp}</b></p>" +
                                    $"<p>Vui lòng nhập mã này vào trang xác minh. Mã sẽ hết hạn sau 5 phút.</p>";
            
            string emailTo = user.Email;
            Task.Run(async () => 
            {
                try 
                {
                    await _emailSender.SendEmailAsync(emailTo, "Mã Xác minh Đặt lại Mật khẩu", messageBody); 
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi vào Console của VPS để debug nếu mail không đi
                    Console.WriteLine($"Lỗi gửi mail ngầm (ForgotPass): {ex.Message}");
                }
            });
            // -------------------------

            // 3. Chuyển hướng NGAY LẬP TỨC
            return RedirectToAction("VerifyOtp", new { email = user.Email });
        }
        
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
                    //lưu Email vào Session, Controller của Action ResetPassword (GET) có thể kiểm tra xem
                    //người dùng đã hoàn thành bước OTP hay chưa trước khi hiển thị form đặt lại mật khẩu.
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
                return RedirectToAction("ForgotPassword");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                // 1. TẠO MÃ OTP MỚI VÀ LƯU VÀO DB 
                string otp = new Random().Next(100000, 999999).ToString(); 
                user.OtpCode = otp;
                user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(5);
                await _context.SaveChangesAsync();

                // 2. GỬI EMAIL MỚI (CHẠY NGẦM)
                string messageBody = $"<h3>Mã Đặt lại Mật khẩu</h3>" +
                                     $"<p>Mã OTP MỚI của bạn là: <b>{otp}</b></p>";
        
                // --- ĐOẠN CODE SỬA ĐỔI ---
                string emailTo = user.Email;
                Task.Run(async () => 
                {
                    try 
                    {
                        await _emailSender.SendEmailAsync(emailTo, "Mã Xác minh MỚI", messageBody);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi gửi mail ngầm (Resend): {ex.Message}");
                    }
                });
                // -------------------------

                // 3. Chuyển hướng NGAY LẬP TỨC
                TempData["SuccessMessage"] = "Mã xác minh mới đang được gửi đến email của bạn.";
                return RedirectToAction("VerifyOtp", new { email = user.Email });
            }
    
            return RedirectToAction("VerifyOtp", new { email = email });
        }
        // GET: /Account/ResetPassword
        public IActionResult ResetPassword()
        {
            // đảm bảo người dùng đã qua bước VerifyOtp
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
                // 1. Cập nhật Mật khẩu 
                user.Password = model.NewPassword; 
        
                // 2. Vô hiệu hóa token/session
                HttpContext.Session.Remove("ResetTokenEmail");

                await _context.SaveChangesAsync();

                // Chuyển hướng về trang đăng nhập với thông báo thành công
                TempData["SuccessMessage"] = "Mật khẩu của bạn đã được đặt lại thành công. Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            // Nếu không tìm thấy user, chuyển hướng về login
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