using Microsoft.EntityFrameworkCore;
using PerrosPeligrososApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configuración de CORS para permitir cualquier origen (modo desarrollo/pruebas)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin() // Permite peticiones desde cualquier origen
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// cadena conexion
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PerrosPeligrososApiDbContext>(options =>
    options.UseSqlServer(connectionString));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Usa la política de CORS definida
app.UseCors("AllowAllOrigins"); // Cambiado a "AllowAllOrigins"

app.MapControllers();

app.Run();