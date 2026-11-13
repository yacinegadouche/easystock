using EasyStock.Web.Authorization;
using EasyStock.Web.Data;
using EasyStock.Web.Models;
using EasyStock.Web.Notifications;
using EasyStock.Web.Reports;
using EasyStock.Web.Services;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var demoPassword = builder.Configuration["Seed:DemoPassword"];

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention()
           .UseAsyncSeeding(async (context, _, cancellationToken) =>
               await DbSeeder.SeedAsync((ApplicationDbContext)context, demoPassword, cancellationToken))
           .UseSeeding((context, _) =>
               DbSeeder.SeedAsync((ApplicationDbContext)context, demoPassword).GetAwaiter().GetResult()));

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<AssignmentService>();
builder.Services.AddScoped<HistoryService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddSingleton<IReportExporter, CsvReportExporter>();
builder.Services.AddSingleton<IReportExporter, ExcelReportExporter>();
builder.Services.AddSingleton<IReportExporter, PdfReportExporter>();
builder.Services.AddOptions<EmailSettings>()
    .Bind(builder.Configuration.GetSection(EmailSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddSingleton<LowStockAlertQueue>();
builder.Services.AddHostedService<LowStockNotificationWorker>();
builder.Services.AddDataProtection()
    .SetApplicationName("EasyStock")
    .PersistKeysToDbContext<ApplicationDbContext>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.Name = "EasyStock.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Events.OnValidatePrincipal = CookieValidation.ValidateAsync;
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build())
    .AddPolicy(Policies.ManageUsers, policy =>
        policy.RequireRole(nameof(UserRole.Admin)))
    .AddPolicy(Policies.ViewReports, policy =>
        policy.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Manager)));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets().AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

public partial class Program;