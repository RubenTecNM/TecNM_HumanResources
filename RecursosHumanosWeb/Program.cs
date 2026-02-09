using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecursosHumanosWeb.Controllers;
using RecursosHumanosWeb.Models;
using Microsoft.Extensions.Options;
using RecursosHumanosWeb.Filters;
using RecursosHumanosWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AuthenticationDataAttribute>();
    options.Filters.Add<ApiExceptionFilter>();
})
.AddJsonOptions(options =>
{
    // Esto es vital para que el Usuario 1 (que se creó a sí mismo) no rompa la página
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.PropertyNamingPolicy = null; 
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddDbContext<RecursosHumanosContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("RecursosHumanosContext"));
});
builder.Services.AddHttpContextAccessor();
// AutoMapper
builder.Services.AddAutoMapper(typeof(RecursosHumanosWeb.Mapping.AutoMapperProfile));

// Memory cache for tokens
builder.Services.AddMemoryCache();

// SMTP configuration from appsettings
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// IEmailSender implementation
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

// Business Services
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<ICorteService, CorteService>();
builder.Services.AddScoped<IPermisoService, PermisoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("TipoUsuario", "1"));
    options.AddPolicy("RHOnly", policy => policy.RequireClaim("TipoUsuario", "3"));
    options.AddPolicy("AutorizadorOnly", policy => policy.RequireClaim("TipoUsuario", "2"));
    options.AddPolicy("RHOrAdmin", policy => policy.RequireClaim("TipoUsuario", "1", "3"));
    options.AddPolicy("AutorizadorOrAbove", policy => policy.RequireClaim("TipoUsuario", "1", "3", "2"));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            if (exception != null)
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError(exception.Error, "Excepci�n no manejada: {Message}", exception.Error.Message);
                await context.Response.WriteAsync($"Error: {exception.Error.Message}");
            }
        });
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogDebug("Procesando solicitud: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next(context);
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();