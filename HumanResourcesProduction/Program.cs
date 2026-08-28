using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HumanResourcesProduction.Filters;
using HumanResourcesProduction.Models;
using HumanResourcesProduction.Services;
using HumanResourcesProduction.Settings;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

// 1. Configurar CORS (Open para microservicios)

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
            new() { Url = "https://nexsmart.online" },
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

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AuthenticationDataAttribute>();
    options.Filters.Add<ApiExceptionFilter>();
})
.AddJsonOptions(options =>
{
    // Esto es vital para que el User 1 (que se creó a sí mismo) no rompa la página
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    // El SPA consume respuestas en camelCase (axios) — caso: /api/PermissionTypes devolvía
    // PascalCase (Id, Date1) y el frontend espera id/config/date1. 
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; 
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        // En desarrollo el SPA corre en :5173 y la API en :5270.
        // SameSite=Strict bloquea las cookies en peticiones cross-port del proxy de Vite.
        // SameSite=Lax permite el flujo de login vía proxy sin abrir seguridad en producción.
        if (builder.Environment.IsDevelopment())
            options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Relajar SameSite del token antiforgery en desarrollo para el proxy de Vite
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddAntiforgery(options =>
    {
        options.Cookie.SameSite = SameSiteMode.Lax;
    });
}

// Persistencia de llaves para evitar errores de Antiforgery en Docker
var keysPath = builder.Environment.IsDevelopment() 
    ? Path.Combine(builder.Environment.ContentRootPath, "Keys") 
    : "/app/keys";

if (!Directory.Exists(keysPath))
{
    Directory.CreateDirectory(keysPath);
}

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("TecRHPermisosApp");


builder.Services.AddDbContext<HumanResourcesContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("HumanResourcesContext"));
});

// Registrar el Guardián de Integridad de Periodos
builder.Services.AddHostedService<HumanResourcesProduction.Services.PeriodIntegrityWorker>();
builder.Services.AddHttpContextAccessor();
// AutoMapper
builder.Services.AddAutoMapper(typeof(HumanResourcesProduction.Mapping.AutoMapperProfile));

// Memory cache for tokens
builder.Services.AddMemoryCache();

// Login attempt lockout (anti fuerza bruta)
builder.Services.Configure<LoginAttemptOptions>(builder.Configuration.GetSection(LoginAttemptOptions.SectionName));
builder.Services.AddSingleton<ILoginAttemptTracker, MemoryLoginAttemptTracker>();

// SMTP configuration from appsettings
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// IEmailSender implementation
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

// Business Services
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<ExcelImportService>();
builder.Services.AddScoped<ICutoffService, CutoffService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IEvidenceStorage, PhysicalEvidenceStorage>();

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("UserType", "1"));
    options.AddPolicy("RHOnly", policy => policy.RequireClaim("UserType", "3"));
    options.AddPolicy("AutorizadorOnly", policy => policy.RequireClaim("UserType", "2"));
    options.AddPolicy("RHOrAdmin", policy => policy.RequireClaim("UserType", "1", "3"));
    options.AddPolicy("AutorizadorOrAbove", policy => policy.RequireClaim("UserType", "1", "3", "2"));
});

var app = builder.Build();

// 3. Middlewares prioritarios e infraestructura
// IMPORTANTE: ForwardedHeaders debe ir antes que cualquier otro middleware
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    // Permitir redes locales de Docker
    KnownNetworks = { }, 
    KnownProxies = { }
});

app.UseCors("OpenCors");

// Health Check para monitoreo del túnel
app.MapGet("/health", () => Results.Ok(new { Message = "TecRH Permissions API is running", Environment = app.Environment.EnvironmentName }));



// 4. Lógica de Inicialización de Base de Datos Automatizada con Reintentos
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<HumanResourcesContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    int retries = 10;
    while (retries > 0)
    {
        try 
        {
            logger.LogInformation("Intentando conectar a la base de datos... (Intentos restantes: {Retries})", retries);
            
            // Si la base de datos no existe, la crea. 
            // Si existe, NO hace nada (ni siquiera crea tablas faltantes).
            context.Database.EnsureCreated();

            // Lógica extra: Si la DB existe pero no tiene tablas, forzamos la creación del esquema.
            var databaseCreator = context.Database.GetService<IRelationalDatabaseCreator>() 
                                  as RelationalDatabaseCreator;
            
            try {
                if (databaseCreator != null) {
                    databaseCreator.CreateTables();
                    logger.LogInformation("Esquema de tablas creado exitosamente.");
                }
            } catch (Exception) {
                // Si las tablas ya existen, CreateTables lanzará una excepción, la ignoramos.
                logger.LogDebug("Las tablas ya existen o no se pudieron crear manualmente (omitiendo).");
            }


            
            // Diagnóstico: Listar tablas existentes
            try {
                var tables = new List<string>();
                using (var command = context.Database.GetDbConnection().CreateCommand()) {
                    command.CommandText = "SELECT name FROM sys.tables";
                    if (command.Connection.State != System.Data.ConnectionState.Open) command.Connection.Open();
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) tables.Add(reader.GetString(0));
                    }
                }
                logger.LogInformation("DatabaseTables encontradas en la DB: {Tables}", string.Join(", ", tables));
            } catch (Exception ex) {
                logger.LogWarning("No se pudo listar las tablas: {Message}", ex.Message);
            }

            if (!context.Users.Any())

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
                    
                    // Separamos por el delimitador estándar GO (insensible a mayúsculas y con posibles espacios)
                    var blocks = System.Text.RegularExpressions.Regex.Split(objectsContent, @"^\s*GO\s*$", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    
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

            // Aplicar upgrades de esquema versionados (idempotentes) a bases existentes
            var upgradeBase = new[] { AppContext.BaseDirectory, app.Environment.ContentRootPath }
                .FirstOrDefault(dir => Directory.Exists(Path.Combine(dir, "db", "upgrade")))
                ?? AppContext.BaseDirectory;
            SchemaUpgradeRunner.ApplyPendingUpgrades(context, logger, upgradeBase);

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
            logger.LogWarning("SQL Server no está listo: {ErrorMessage}. Reintentando en 10s...", ex.Message);
            Thread.Sleep(10000);
        }
    }
}


if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseExceptionHandler();

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
    pattern: "{controller}/{action}/{id?}");

// SPA fallback: serve index.html for Vue Router routes that don't match MVC controllers
app.MapFallbackToFile("index.html");

// 5. Configurar Scalar API Documentation
app.MapOpenApi();
app.MapScalarApiReference(options => {
    options.WithTitle("Recursos Humanos API")
           .WithTheme(ScalarTheme.Moon)
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.Run();