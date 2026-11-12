using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using KemilEditorial.API.Data;
using KemilEditorial.API.Models;
using KemilEditorial.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar la conexión a SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Agregar servicios al contenedor
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Configurar CORS para que tu frontend en React (puerto 5173 o 3000) pueda consumir la API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// 4. Servicios propios
builder.Services.AddScoped<DescuentoService>();
builder.Services.AddScoped<OrdenService>();
builder.Services.AddScoped<TokenService>();

// Portadas subidas desde el panel: se guardan en backend/uploads/portadas y se sirven en /uploads/portadas
var carpetaUploads = Path.Combine(builder.Environment.ContentRootPath, "uploads");
var carpetaPortadas = Path.Combine(carpetaUploads, "portadas");
Directory.CreateDirectory(carpetaPortadas);
builder.Services.AddScoped(sp => new LibroService(sp.GetRequiredService<AppDbContext>(), carpetaPortadas));

// 5. Autenticación JWT
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Falta Jwt:Key en la configuración.");
if (jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:Key debe tener al menos 32 caracteres.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };

        // Revalida contra la BD en cada petición: si la cuenta se desactivó o cambió
        // de rol, el token anterior deja de servir de inmediato.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var idClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var rolClaim = context.Principal?.FindFirstValue(ClaimTypes.Role);

                if (!int.TryParse(idClaim, out var id))
                {
                    context.Fail("Token inválido.");
                    return;
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var actual = await db.Usuarios
                    .AsNoTracking()
                    .Where(u => u.IdUsuario == id)
                    .Select(u => new { u.Estado, Rol = u.Rol!.NombreRol })
                    .FirstOrDefaultAsync();

                if (actual is null || !actual.Estado || actual.Rol != rolClaim)
                    context.Fail("Sesión inválida: cuenta desactivada o rol modificado.");
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    // Denegar por defecto: todo endpoint exige sesión, salvo los marcados [AllowAnonymous]
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// 6. Límite de intentos de login: 5 por minuto por IP
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

// 7. Configurar el pipeline de solicitudes HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Sirve las portadas subidas (público: son imágenes del catálogo). nosniff evita que el navegador las reinterprete.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(carpetaUploads),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx => ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff"
});

// Activar la política de CORS antes de los controladores
app.UseCors("AllowReactApp");

app.UseRateLimiter();

// ORDEN IMPORTANTE: primero autenticación (¿quién eres?), luego autorización (¿qué puedes hacer?)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();