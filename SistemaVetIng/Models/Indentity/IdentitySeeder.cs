using Microsoft.AspNetCore.Identity;

namespace SistemaVetIng.Models.Indentity
{
    public class IdentitySeeder
    {
        // Este método recibe el service provider del sistema y lo usamos para acceder a los managers de usuario y roles
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
                var roleExist = await roleManager.RoleExistsAsync(roleName); // ¿Ya existe este rol?
                if (!roleExist)
                    await roleManager.CreateAsync(new Rol { Name = roleName }); // Si no existe, lo creamos
            }

            // Ahora creamos un usuario inicial para la veterinaria o admin
            var adminEmail = "admin@veting.com";

            // Buscamos si ese usuario ya existe
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // Si no existe, lo creamos manualmente con los datos que queramos
                var user = new Usuario
                {
                    UserName = "admin",           // Nombre de usuario en el sistema
                    Email = adminEmail,           // Email
                    EmailConfirmed = true,        // Confirmamos el email de una
                    NombreUsuario = "admin",      // Atributo personalizado
                    Clave = "admin123"            // Atributo personalizado (NO se usa para autenticar)
                };

                // Creamos el usuario con una contraseña segura (mínimo una mayúscula, número, símbolo, etc.)
                var result = await userManager.CreateAsync(user, "Admin123!");

                // Si lo pudimos crear, le asignamos el rol
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, "Veterinaria");
            }
        }
    }
}
