using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Veritaban� ba�lant�s�n� yap�land�r
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity yap�land�rmas�
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Yetkilendirme ve kimlik do�rulama ayarlar�
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login"; // Kullan�c� giri� yapmad���nda y�nlendirilecek sayfa
    options.AccessDeniedPath = "/Auth/AccessDenied"; // Yetkisiz eri�im y�nlendirme
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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}");
});


app.Run();
