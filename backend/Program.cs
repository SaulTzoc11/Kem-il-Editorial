using Microsoft.EntityFrameworkCore;
using KemilEditorial.API.Data;
using KemilEditorial.API.Models;

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
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// 4. Configurar el pipeline de solicitudes HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Activar la política de CORS antes de los controladores
app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

app.Run();