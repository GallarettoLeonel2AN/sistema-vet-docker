using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SistemaVetIng.Data;
using SistemaVetIng.Models.Extension;
using SistemaVetIng.Models.Indentity;
using MercadoPago.Config;
using SistemaVetIng.Models;
using Microsoft.AspNetCore.DataProtection;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n de conexion
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// MErcado Pago
var mpSettings = builder.Configuration.GetSection("MercadoPagoSettings").Get<MercadoPagoSettings>();
if (mpSettings != null && !string.IsNullOrEmpty(mpSettings.AccessToken))
{
    MercadoPagoConfig.AccessToken = mpSettings.AccessToken;
}
else
{
    // Manejo de error si falta la configuraci�n
    throw new InvalidOperationException("Falta o no est� configurado el AccessToken de Mercado Pago.");
}

builder.Services.AddHttpClient("Api", client =>
{
    var apiUrl = builder.Configuration["Urls:Api"];
    client.BaseAddress = new Uri(apiUrl!);
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
);


builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>();

builder.Services.AddIdentity<Usuario, Rol>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Inyecci�n del Repositorio 
builder.Services.AddRepositories()
    .AddServices();

// AddNToastNotifyToastr
builder.Services.AddControllersWithViews()
    .AddNToastNotifyToastr(new ToastrOptions()
    {
        // Posici�n
        PositionClass = ToastPositions.TopRight,

        // Bot�n de cierre y barra de progreso
        CloseButton = true,
        ProgressBar = true,

        // Duraci�n y comportamiento
        TimeOut = 5000,
        ExtendedTimeOut = 1000,
        NewestOnTop = true,
        TapToDismiss = false // La notificaci�n no se cierra al hacer clic
    });

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Busca el contexto de tu base de datos
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Aplica cualquier migraci�n pendiente.
        // Esto crear� la base de datos y todas sus tablas si no existen.
        context.Database.Migrate();

        // El log muestra que ten�s un "IdentitySeeder" para crear roles y usuarios.
        // Lo llamamos aqu� para que se ejecute despu�s de crear las tablas.
        await IdentitySeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurri� un error al migrar o inicializar la base de datos.");
    }
}
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


app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.UseNToastNotify();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();