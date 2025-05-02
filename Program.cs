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
  options.Cookie.HttpOnly = true;
  options.Cookie.Name = "TacKimlikAuth";        // isterseniz özel isim
  options.ExpireTimeSpan = TimeSpan.FromDays(7); // 7 gün boyunca hatırla
  options.SlidingExpiration = true;             // her istekte yenilenir
  options.LoginPath = "/Account/Login";
  options.AccessDeniedPath = "/Error/403";
});
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddSession();
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
app.UseSession();
app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
  endpoints.MapControllerRoute(
      name: "default",
      pattern: "{controller=Home}/{action=Dashboard}/{id?}");
});

app.UseExceptionHandler("/Error/500"); // özel hata sayfasına yönlendirme
app.UseStatusCodePagesWithReExecute("/Error/{0}"); // 404 gibi durumlar için


app.Run();
