using Microsoft.EntityFrameworkCore;
using DNUStudyPlanner.Data;
using DNUStudyPlanner.Services;
using DotNetEnv; 
using DNUStudyPlanner.Configuration;
Env.Load(); 
var builder = WebApplication.CreateBuilder(args); 
var emailCheck = builder.Configuration["SmtpSettings:SmtpMailEmail"];

// Nếu giá trị là NULL, chứng tỏ file .env CHƯA ĐƯỢC ĐỌC hoặc TÊN BIẾN SAI
if (string.IsNullOrEmpty(emailCheck))
{
    // Thêm một điểm dừng (Breakpoint) ở đây để kiểm tra trong debug
    System.Console.WriteLine("LỖI CẤU HÌNH: SmtpMailEmail vẫn là NULL!"); 
    // Hoặc tạm thời gán một giá trị hardcode để ứng dụng chạy (chỉ để test)
}

builder.Services.Configure<DNUStudyPlanner.Configuration.SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));
// ...
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); 

builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();