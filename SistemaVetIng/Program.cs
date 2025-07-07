using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SistemaVetIng.Data;
using SistemaVetIng.Models.Indentity;


var builder = WebApplication.CreateBuilder(args);

// Cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Registrar el contexto con Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity con Usuario y Rol personalizados
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
// seeder de identity
using (var scope = app.Services.CreateScope()) // Creamos un scope manual para acceder a los servicios
{
    var services = scope.ServiceProvider;

    // Llamamos al seeder para que cree roles y el usuario admin si no existen
    await IdentitySeeder.SeedRolesAndAdminAsync(services);
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
