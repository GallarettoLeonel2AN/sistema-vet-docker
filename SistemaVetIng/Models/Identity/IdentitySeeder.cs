using Microsoft.AspNetCore.Identity;

namespace SistemaVetIng.Models.Indentity
{
    public class IdentitySeeder
    {
        // Este metodo recibe el service provider del sistema y lo usamos para acceder a los managers de usuario y roles
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            // Obtenemos el RoleManager para manejar los roles de Identity
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Rol>>();

            // Obtenemos el UserManager para manejar usuarios de Identity
            var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();

            // Creamos un array con los nombres de los roles que vamos a usar
            string[] roles = { "Cliente", "Veterinario", "Veterinaria" };

            // Recorremos cada rol y lo creamos si no existe
            foreach (var roleName in roles)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName); // Existe?
                if (!roleExist)
                    await roleManager.CreateAsync(new Rol { Name = roleName }); // Si no existe, lo creamos
            }

            // Creamos un usuario inicial para la veterinaria (admin)
            var adminEmail = "admin@veting.com";

            // Buscamos si ese usuario ya existe
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // Si no existe, lo creamos manualmente
                var user = new Usuario
                {
                    UserName = "admin",     
                    Email = adminEmail,          
                    EmailConfirmed = true,        
                    NombreUsuario = "admin",      
                            
                };

                // Creamos Contraseña
                var result = await userManager.CreateAsync(user, "Admin123!");

                // Le asignamos el rol
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, "Veterinaria");
            }
        }
    }
}
