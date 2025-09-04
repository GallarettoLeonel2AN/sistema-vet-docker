using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Repository.Implementacion;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class ClienteService : IClienteService
    {

        private readonly IGeneralRepository<Cliente> _clienteRepository;
        private readonly UserManager<Usuario> _userManager;

        public ClienteService(IGeneralRepository<Cliente> clienteRepository, UserManager<Usuario> userManager)
        {
            _clienteRepository = clienteRepository;
            _userManager = userManager;
        }

        #region REGISTRAR CLIENTE
        public async Task<Cliente> Registrar(ClienteRegistroViewModel viewModel)
        {
           
            var usuario = new Usuario
            {
                UserName = viewModel.Email, 
                Email = viewModel.Email,
                NombreUsuario = $"{viewModel.Nombre} {viewModel.Apellido}",
            };

            // Intentamos crear el usuario en el sistema de autenticación de Identity con la contraseña proporcionada.

            var result = await _userManager.CreateAsync(usuario, viewModel.Password);
            if (!result.Succeeded)
            {
                throw new Exception("Error al crear el usuario en Identity.");
            }
            await _userManager.AddToRoleAsync(usuario, "Cliente");

            // Creamos al cliente 
            var cliente = new Cliente
            {
                Nombre = viewModel.Nombre,
                Apellido = viewModel.Apellido,
                Dni = viewModel.Dni,
                Telefono = viewModel.Telefono,
                Direccion = viewModel.Direccion,
                UsuarioId = usuario.Id
            };

            await _clienteRepository.Agregar(cliente);
            await _clienteRepository.Guardar();

            return cliente;
        }
        #endregion

        public async Task<Cliente> Modificar(ClienteEditarViewModel model)
        {

            var cliente = await _clienteRepository.ObtenerPorId(model.Id);
            if (cliente == null)
            {
                throw new KeyNotFoundException("Cliente no encontrado.");
            }

            cliente.Nombre = model.Nombre;
            cliente.Apellido = model.Apellido;
            cliente.Dni = model.Dni;
            cliente.Direccion = model.Direccion;
            cliente.Telefono = model.Telefono;

            _clienteRepository.Modificar(cliente);
            await _clienteRepository.Guardar();

            return cliente;
        }

        public async Task Eliminar(int id)
        {
            var cliente = await _clienteRepository.ObtenerPorId(id);

            if (cliente == null)
            {
                throw new KeyNotFoundException("Cliente no encontrado.");
            }

            if (cliente.Usuario != null)
            {
                _clienteRepository.Eliminar(cliente);
                await _clienteRepository.Guardar();

                var result = await _userManager.DeleteAsync(cliente.Usuario);
                if (!result.Succeeded)
                {
                    throw new Exception("Error al eliminar el usuario asociado.");
                }
            }

        }

        public async Task<IEnumerable<Cliente>> ListarTodo()
        {
            return await _clienteRepository.ListarTodo();
        }

        public async Task<Cliente> ObtenerPorId(int id)
        {
            return await _clienteRepository.ObtenerPorId(id);
        }
        public async Task<IEnumerable<Cliente>> FiltrarPorBusqueda(string busqueda)
        {
            var clientes = await _clienteRepository.ListarTodo();

            if (!string.IsNullOrEmpty(busqueda))
            {
                return clientes.Where(c => c.Dni.ToString().Contains(busqueda));
            }

            return clientes;
        }

    }
}
