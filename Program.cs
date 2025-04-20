using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Veritabanı bağlantısını yapılandır
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity yapılandırması
builder.Services.ConfigureApplicationCookie(options =>
{
  // Çerez süresi ayarları
  options.Cookie.HttpOnly = true;
  options.ExpireTimeSpan = TimeSpan.FromDays(30);  // Çerez süresi (30 gün)
  options.SlidingExpiration = true;  // Eğer kullanıcı tekrar giriş yaparsa sürenin sıfırlanmasını sağlar
});
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Yetkilendirme ve kimlik doğrulama ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
  options.LoginPath = "/Auth/Login"; // Kullanıcı giriş yapmadığında yönlendirilecek sayfa
  options.AccessDeniedPath = "/Auth/AccessDenied"; // Yetkisiz erişim yönlendirme
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
  endpoints.MapControllerRoute(
      name: "default",
      pattern: "{controller=Account}/{action=Login}/{id?}");
});

app.UseExceptionHandler("/Error/500"); // özel hata sayfasına yönlendirme
app.UseStatusCodePagesWithReExecute("/Error/{0}"); // 404 gibi durumlar için


app.Run();
