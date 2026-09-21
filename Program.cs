using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using UTHMLibrary.Data;
using UTHMLibrary.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// ADD SERVICES
// ============================================

// Add MVC Controllers with Views
builder.Services.AddControllersWithViews();

// Add Database Context (SQLite)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================
// EMAIL SERVICE
// ============================================

builder.Services.AddScoped<IEmailService, EmailService>();

// ============================================
// GEMINI AI SERVICE
// ============================================

builder.Services.AddScoped<IAIChatService, GeminiChatService>();

// ============================================
// SESSION & CACHE
// ============================================

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ============================================
// AUTHENTICATION
// ============================================

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
});

// ============================================
// HTTP CONTEXT
// ============================================

builder.Services.AddHttpContextAccessor();

// ============================================
// HTTP CLIENT
// ============================================

builder.Services.AddHttpClient();

// ============================================
// LOGGING
// ============================================

builder.Services.AddLogging();

// ============================================
// BUILD APP
// ============================================

var app = builder.Build();

// ============================================
// SEED DATABASE
// ============================================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
    DbSeeder.Seed(db);
}

// ============================================
// MIDDLEWARE PIPELINE
// ============================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ============================================
// ROUTING
// ============================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ============================================
// RUN APP
// ============================================

app.Run();