using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PCRETAIL.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    try
    {
        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "crash.log");
        var sb = new StringBuilder();
        sb.AppendLine("=== UnhandledException ===");
        sb.AppendLine(DateTime.Now.ToString("O"));
        sb.AppendLine("IsTerminating: " + e.IsTerminating);
        if (e.ExceptionObject is Exception ex)
        {
            sb.AppendLine(ex.ToString());
        }
        else
        {
            sb.AppendLine("ExceptionObject: " + (e.ExceptionObject?.ToString() ?? "<null>"));
        }

        sb.AppendLine();
        File.AppendAllText(path, sb.ToString());
    }
    catch
    {
    }
};

TaskScheduler.UnobservedTaskException += (_, e) =>
{
    try
    {
        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "crash.log");
        File.AppendAllText(path, "=== UnobservedTaskException ===\n" + DateTime.Now.ToString("O") + "\n" + e.Exception + "\n\n");
    }
    catch
    {
    }
};

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

var picturesRoot = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
    "PCRETAILIMAGES");
var configurationsImagesRoot = Path.Combine(picturesRoot, "CONFIGURATIONS");
var componentsImagesRoot = Path.Combine(picturesRoot, "COMPONENTS");

Directory.CreateDirectory(configurationsImagesRoot);
Directory.CreateDirectory(componentsImagesRoot);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(configurationsImagesRoot),
    RequestPath = "/configimages"
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(componentsImagesRoot),
    RequestPath = "/componentimages"
});

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
