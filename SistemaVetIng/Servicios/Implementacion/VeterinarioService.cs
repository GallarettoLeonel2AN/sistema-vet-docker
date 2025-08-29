using Microsoft.AspNetCore.Identity;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class VeterinarioService : IVeterinarioService
    {
        private readonly IGeneralRepository<Veterinario> _veterinarioRepository;
        private readonly UserManager<Usuario> _userManager;

        public VeterinarioService(IGeneralRepository<Veterinario> veterinarioRepository, UserManager<Usuario> userManager)
        {
            _veterinarioRepository = veterinarioRepository;
            _userManager = userManager;
        }

        // REGISTRAR VETERINARIO
        public async Task<Veterinario> Registrar(VeterinarioRegistroViewModel viewModel)
        {
            // Crear el usuario de Identity
            var usuario = new Usuario
            {
                UserName = viewModel.Email,
                Email = viewModel.Email,
                NombreUsuario = $"{viewModel.Nombre} {viewModel.Apellido}",
            };

            var result = await _userManager.CreateAsync(usuario, viewModel.Password);
            if (!result.Succeeded)
            {
                throw new Exception("Error al crear el usuario en Identity.");
            }
            await _userManager.AddToRoleAsync(usuario, "Veterinario");

            // Mapear el ViewModel a la entidad
            var veterinario = new Veterinario
            {
                Nombre = viewModel.Nombre,
                Apellido = viewModel.Apellido,
                Dni = viewModel.Dni,
                Direccion = viewModel.Direccion,
                Telefono = viewModel.Telefono,
                Matricula = viewModel.Matricula,
                UsuarioId = usuario.Id
            };

            // Repository
            await _veterinarioRepository.Agregar(veterinario);
            await _veterinarioRepository.Guardar();

            return veterinario;
        }

        // MODIFICAR VETERINARIO
        public async Task<Veterinario> Modificar(VeterinarioEditarViewModel viewModel)
        {
  
            var veterinario = await _veterinarioRepository.ObtenerPorId(viewModel.Id);
            if (veterinario == null)
            {
                throw new KeyNotFoundException("Veterinario no encontrado.");
            }

            // Lógica de negocio: Mapear los datos del ViewModel a la entidad
            veterinario.Nombre = viewModel.Nombre;
            veterinario.Apellido = viewModel.Apellido;
            veterinario.Dni = viewModel.Dni;
            veterinario.Direccion = viewModel.Direccion;
            veterinario.Telefono = viewModel.Telefono;
            veterinario.Matricula = viewModel.Matricula;

            _veterinarioRepository.Modificar(veterinario);
            await _veterinarioRepository.Guardar();

            return veterinario;
        }

        // ELIMINAR VETERINARIO
        public async Task Eliminar(int id)
        {
            var veterinario = await _veterinarioRepository.ObtenerPorId(id);
            if (veterinario == null)
            {
                throw new KeyNotFoundException("Veterinario no encontrado.");
            }

            if (veterinario.Usuario != null)
            {
                _veterinarioRepository.Eliminar(veterinario);
                await _veterinarioRepository.Guardar();

                var result = await _userManager.DeleteAsync(veterinario.Usuario);
                if (!result.Succeeded)
                {
                    throw new Exception("Error al eliminar el usuario asociado.");
                }
            }
        }

        // Método para obtener un veterinario por ID
        public async Task<Veterinario> ObtenerPorId(int id)
        {
            return await _veterinarioRepository.ObtenerPorId(id);
        }

        // Método para listar todos los veterinarios
        public async Task<IEnumerable<Veterinario>> ListarTodo()
        {
            return await _veterinarioRepository.ListarTodo();
        }
    }
}
