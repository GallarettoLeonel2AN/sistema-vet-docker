using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class VeterinariaService : IVeterinariaService
    {
        private readonly IVeterinarioService _veterinarioService;
        private readonly IClienteService _clienteService;
        private readonly IMascotaService _mascotaService;
        private readonly IVeterinariaConfigService _veterinariaConfigService;
    
        public VeterinariaService(
            IVeterinarioService veterinarioService,
            IClienteService clienteService,
            IMascotaService mascotaService)
        {
            _veterinarioService = veterinarioService;
            _clienteService = clienteService;
            _mascotaService = mascotaService;
        }

        public async Task<VeterinariaPaginaPrincipalViewModel> PaginaPrincipalAsync(
            string busquedaVeterinario = null,
            string busquedaCliente = null,
            string busquedaMascota = null)
        {
            var viewModel = new VeterinariaPaginaPrincipalViewModel();

        
            var configuracionDb = (await _veterinariaConfigService.ListarTodo()).FirstOrDefault();
            if (configuracionDb != null)
            {
                viewModel.ConfiguracionTurnos = new ConfiguracionVeterinariaViewModel
                {
                    HoraInicio = configuracionDb.HoraInicio,
                    HoraFin = configuracionDb.HoraFin,
                    DuracionMinutosPorConsulta = configuracionDb.DuracionMinutosPorConsulta,
                    TrabajaLunes = configuracionDb.TrabajaLunes,
                    TrabajaMartes = configuracionDb.TrabajaMartes,
                    TrabajaMiercoles = configuracionDb.TrabajaMiercoles,
                    TrabajaJueves = configuracionDb.TrabajaJueves,
                    TrabajaViernes = configuracionDb.TrabajaViernes,
                    TrabajaSabado = configuracionDb.TrabajaSabado,
                    TrabajaDomingo = configuracionDb.TrabajaDomingo
                };
            }
            else
            {
                viewModel.ConfiguracionTurnos = null;
            }

            var veterinarios = await _veterinarioService.FiltrarPorBusqueda(busquedaVeterinario);
            viewModel.Veterinarios = veterinarios.Select(p => new VeterinarioViewModel
            {
                Id = p.Id,
                NombreCompleto = $"{p.Nombre} {p.Apellido}",
                Telefono = p.Telefono,
                NombreUsuario = p.Usuario?.Email,
                Direccion = p.Direccion,
                Matricula = p.Matricula,
            }).ToList();

            var clientes = await _clienteService.FiltrarPorBusqueda(busquedaCliente);
            viewModel.Clientes = clientes.Select(c => new ClienteViewModel
            {
                Id = c.Id,
                NombreCompleto = $"{c.Nombre} {c.Apellido}",
                Telefono = c.Telefono,
                NombreUsuario = c.Usuario?.Email
            }).ToList();

            var mascotas = await _mascotaService.FiltrarPorBusqueda(busquedaMascota);
            viewModel.Mascotas = mascotas.Select(m => new MascotaListViewModel
            {
                Id = m.Id,
                NombreMascota = m.Nombre,
                Especie = m.Especie,
                Sexo = m.Sexo,
                Raza = m.Raza,
                EdadAnios = DateTime.Today.Year - m.FechaNacimiento.Year - (DateTime.Today.Month < m.FechaNacimiento.Month || (DateTime.Today.Month == m.FechaNacimiento.Month && DateTime.Today.Day < m.FechaNacimiento.Day) ? 1 : 0),
                NombreDueno = $"{m.Propietario?.Nombre} {m.Propietario?.Apellido}",
                ClienteId = m.Propietario?.Id ?? 0
            }).ToList();


            viewModel.CantidadPerrosPeligrosos = 5;

            var mostRequestedBreed = mascotas
                 .GroupBy(m => m.Raza)
                 .OrderByDescending(g => g.Count())
                 .Select(g => g.Key)
                 .FirstOrDefault();

            viewModel.IngresosMensualesEstimados = 1500.00m;
            viewModel.IngresosDiariosEstimados = 5000.00m;

            return viewModel;
        }  
    }
}
