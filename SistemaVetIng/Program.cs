using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NToastNotify;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Repository.Implementacion;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Implementacion;
using SistemaVetIng.Servicios.Interfaces;


var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<Usuario, Rol>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<SmtpSettings>>().Value);
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Inyección del Repositorio 
builder.Services.AddScoped(typeof(IGeneralRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IGeneralRepository<Veterinario>, VeterinarioRepository>();
builder.Services.AddScoped<IGeneralRepository<Cliente>, ClienteRepository>();

// Inyección de Servicios
builder.Services.AddScoped<IVeterinarioService, VeterinarioService>();
builder.Services.AddScoped<IClienteService, ClienteService>();

// AddNToastNotifyToastr
builder.Services.AddControllersWithViews()
    .AddNToastNotifyToastr(new ToastrOptions()
    {
        // Posición
        PositionClass = ToastPositions.TopRight,

        // Botón de cierre y barra de progreso
        CloseButton = true,
        ProgressBar = true,

        // Duración y comportamiento
        TimeOut = 5000, 
        ExtendedTimeOut = 1000, 
        NewestOnTop = true,
        TapToDismiss = false // La notificación no se cierra al hacer clic
    });

var app = builder.Build();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Seeder de Identity
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentitySeeder.SeedRolesAndAdminAsync(services);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Middleware de Autenticación, Autorización y Routing (orden correcto)
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Middleware de NToastNotify 
app.UseNToastNotify();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();