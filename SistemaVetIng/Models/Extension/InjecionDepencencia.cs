using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SistemaVetIng.Data;
using SistemaVetIng.Repository.Implementacion;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Implementacion;
using SistemaVetIng.Servicios.Interfaces;

namespace SistemaVetIng.Models.Extension
{
    public static class InjecionDepencencia 
    {
        public static IServiceCollection AddDatabase(
           this IServiceCollection services,
           IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGeneralRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IGeneralRepository<Veterinario>, VeterinarioRepository>();
            services.AddScoped<IGeneralRepository<Cliente>, ClienteRepository>();
            services.AddScoped<IGeneralRepository<Mascota>, MascotaRepository>();
            services.AddScoped<IGeneralRepository<Chip>, ChipRepository>();
            services.AddScoped<IGeneralRepository<ConfiguracionVeterinaria>, VeterinariaRepository>();
            return services;
        }


        public static IServiceCollection AddServices(this IServiceCollection services)
        {

            services.AddControllersWithViews();
            services.AddRazorPages();         
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<SmtpSettings>>().Value);           
            services.AddScoped<IVeterinarioService, VeterinarioService>();
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IMascotaService, MascotaService>();
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IVeterinariaService, VeterinariaService>();
            return services;
        }
    }
}
