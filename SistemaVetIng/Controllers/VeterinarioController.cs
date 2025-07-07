using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinario")]
    public class VeterinarioController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly ApplicationDbContext _context;

        // Inyectamos los servicios necesarios
        public VeterinarioController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // Acción GET para mostrar el formulario de registro
        [HttpGet]
        public async Task<IActionResult> PaginaPrincipal()
        {

            var viewModel = new VeterinarioPaginaPrincipalViewModel();

            // Cargar Clientes en las tablas
            viewModel.Clientes = await _context.Clientes
                .Select(c => new ClienteViewModel
                {
                    Id = c.Id,
                    NombreCompleto = $"{c.Nombre} {c.Apellido}",
                    Telefono = c.Telefono,
                    NombreUsuario = c.Usuario.Email 
                })
                .ToListAsync();

            // Cargar Mascotas en las tablas
            viewModel.Mascotas = await _context.Mascotas
                .Include(m => m.Propietario) 
                .Select(m => new MascotaListViewModel
                {
                    Id = m.Id,
                    NombreMascota = m.Nombre,
                    Especie = m.Especie,
                    Sexo = m.Sexo,
                    Raza = m.Raza,
                    EdadAnios = DateTime.Today.Year - m.FechaNacimiento.Year - (DateTime.Today.Month < m.FechaNacimiento.Month || (DateTime.Today.Month == m.FechaNacimiento.Month && DateTime.Today.Day < m.FechaNacimiento.Day) ? 1 : 0),
                    NombreDueno = $"{m.Propietario.Nombre} {m.Propietario.Apellido}",
                    ClienteId = m.Propietario.Id
                })
                .ToListAsync();

            return View(viewModel);
        }


    }
}
