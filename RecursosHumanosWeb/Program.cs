using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RecursosHumanosWeb.Filters;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Services;
using RecursosHumanosWeb.Settings;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar CORS (Abierto para microservicios)

builder.Services.AddCors(options =>
{
    options.AddPolicy("OpenCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 2. Configurar OpenAPI con servidores explícitos para Producción
builder.Services.AddOpenApi(options => {
    options.AddDocumentTransformer((document, context, cancellationToken) => {
        document.Servers = new List<Microsoft.OpenApi.Models.OpenApiServer> {
            new() { Url = "https://apitecrh.monclova.tecnm.mx" }, // Ajustar al dominio real
            new() { Url = "http://localhost:5011" }
        };
        return Task.CompletedTask;
    });
});

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

// 3. Middlewares prioritarios e infraestructura
app.UseCors("OpenCors");

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// 4. Lógica de Inicialización de Base de Datos Automatizada con Reintentos
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<RecursosHumanosContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    int retries = 10;
    while (retries > 0)
    {
        try 
        {
            logger.LogInformation("Intentando conectar a la base de datos... (Intentos restantes: {Retries})", retries);
            context.Database.EnsureCreated();
            
            if (!context.Usuarios.Any())
            {
                // 4.1 Inserción de Datos Básicos
                var insertsPath = Path.Combine(AppContext.BaseDirectory, "db", "inserts.sql");
                if (File.Exists(insertsPath))
                {
                    logger.LogInformation("Poblando base de datos con datos básicos...");
                    var sqlContent = File.ReadAllText(insertsPath);
                    context.Database.ExecuteSqlRaw(sqlContent);
                }

                // 4.2 Creación de Objetos (SPs, Triggers, etc.)
                var objectsPath = Path.Combine(AppContext.BaseDirectory, "db", "schema_objects.sql");
                if (File.Exists(objectsPath))
                {
                    logger.LogInformation("Creando procedimientos almacenados y triggers...");
                    var objectsContent = File.ReadAllText(objectsPath);
                    
                    // Separamos por el delimitador personalizado --###--
                    var blocks = objectsContent.Split(new[] { "--###--" }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var block in blocks)
                    {
                        if (!string.IsNullOrWhiteSpace(block))
                        {
                            try {
                                context.Database.ExecuteSqlRaw(block);
                            } catch (Exception ex) {
                                logger.LogWarning("Error al crear objeto: {Message}", ex.Message);
                                // Continuamos con el siguiente objeto
                            }
                        }
                    }
                }
                
                logger.LogInformation("Base de datos inicializada al 100% (Datos y Objetos).");
            }
            break;
        }
        catch (Exception ex)
        {
            retries--;
            if (retries == 0) 
            {
                logger.LogCritical(ex, "Error fatal: No se pudo conectar a SQL Server.");
                throw;
            }
            logger.LogWarning("SQL Server no está listo. Reintentando en 10s...");
            Thread.Sleep(10000);
        }
    }
}

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

// app.UseHttpsRedirection(); // Comentado para evitar errores 502 en Tunnel Cloudflare

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

// 5. Configurar Scalar API Documentation
app.MapOpenApi();
app.MapScalarApiReference(options => {
    options.WithTitle("Recursos Humanos API")
           .WithTheme(ScalarTheme.Moon)
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.Run();