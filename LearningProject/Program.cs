using LearningProject.Data;
using LearningProject.RoleMiddleware;
using LearningProject.Services;
using LearningProject.Services.Impl;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LearningProject.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LearningProjectContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LearningProjectContext") ?? throw new InvalidOperationException("Connection string 'LearningProjectContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Bind ApiUrls
builder.Services.Configure<ApiUrlsOptions>(builder.Configuration.GetSection("ApiUrls"));

//Inregistram serviul Send E-mail 
//builder.Services.AddScoped<SendEmailService>();

builder.Services.AddHttpClient<SendEmailService>();


// Adaugă suport pentru controller-e
builder.Services.AddControllers();


// scoped to match  context lifecycle
builder.Services.AddScoped<IClaimsTransformation, ClaimsTransformer>();
builder.Services.AddScoped<ICereri, CereriService>();
builder.Services.AddScoped<ICereriCacheService, CereriCacheService>();
builder.Services.AddScoped<IBufferedFileUploadService, BufferedFileUploadLocalService>();


builder.Services.AddScoped<ErrorLoggerService>();
builder.Services.AddScoped<IUsers, Users>();


builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();

}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


// Rută doar pentru API (JSON)
app.MapControllers();

// Rută unică pentru View (MVC) -- rutare "directa" catre pagina de Razor 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();