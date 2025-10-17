using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;

namespace SistemaVetIng.Repository.Implementacion
{
    public class AtencionVeterinariaRepository : IAtencionVeterinariaRepository
    {
        private readonly ApplicationDbContext _context;

        public AtencionVeterinariaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HistoriaClinica> GetHistoriaClinicaConMascotayPropietario(int historiaClinicaId)
        {
            return await _context.HistoriasClinicas
                                 .Include(hc => hc.Mascota)
                                     .ThenInclude(m => m.Propietario)
                                 .FirstOrDefaultAsync(hc => hc.Id == historiaClinicaId);
        }

        public async Task<AtencionVeterinaria> ObtenerPorId(int id)
            => await _context.AtencionesVeterinarias.FirstOrDefaultAsync(x => x.Id == id);

        public async Task<AtencionVeterinaria> ObtenerAtencionConCliente(int idAtencion)
        {
            // Navegamos por la cadena: Atencion -> HistoriaClinica -> Mascota -> Cliente
            return await _context.AtencionesVeterinarias
                .Include(a => a.HistoriaClinica)
                    .ThenInclude(hc => hc.Mascota)
                        .ThenInclude(m => m.Propietario) // Asumo que el cliente se llama Propietario en la entidad Mascota
                        .ThenInclude(p => p.Usuario)
                .FirstOrDefaultAsync(a => a.Id == idAtencion);
        }
        public async Task<List<AtencionVeterinaria>> ObtenerAtencionesPendientesPorCliente(int clienteId)
        {
            // NOTA: Asumo que en tu modelo de AtencionVeterinaria tienes la propiedad EstadoDePago.
            // Si no la tienes, DEBES agregarla a la entidad AtencionVeterinaria en tu modelo.
            const string ESTADO_PENDIENTE = "Pendiente";

            // 1. Buscamos las Historias Clínicas de las Mascotas que pertenecen a este Cliente.
            // 2. Buscamos las Atenciones asociadas a esas Historias Clínicas.
            // 3. Filtramos por el estado de pago "Pendiente".

            return await _context.AtencionesVeterinarias
                .Include(a => a.HistoriaClinica) // Necesario para acceder a la Mascota
                    .ThenInclude(hc => hc.Mascota) // Necesario para acceder al Nombre de la Mascota
                                                   // La Mascota del cliente debe coincidir con la Mascota de la Historia Clínica
                .Where(a => a.HistoriaClinica.Mascota.ClienteId == clienteId)
                // Filtramos por el estado que indica que falta el pago
                //.Where(a => a.EstadoDePago == ESTADO_PENDIENTE)
                .ToListAsync();
        }
        public async Task<List<Vacuna>> GetVacunas()
        {
            return await _context.Vacunas.ToListAsync();
        }

        public async Task<List<Estudio>> GetEstudios()
        {
            return await _context.Estudios.ToListAsync();
        }

        public async Task<Veterinario> GetVeterinarioPorId(int usuarioId)
        {
            return await _context.Veterinarios
                                 .FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);
        }

        public async Task<List<Vacuna>> GetVacunaSeleccionada(IEnumerable<int> ids)
        {
            return await _context.Vacunas
                                 .Where(v => ids.Contains(v.Id))
                                 .ToListAsync();
        }

        public async Task<List<Estudio>> GetEstudioSeleccionado(IEnumerable<int> ids)
        {
            return await _context.Estudios
                                 .Where(e => ids.Contains(e.Id))
                                 .ToListAsync();
        }

        public async Task AgregarAtencionVeterinaria(AtencionVeterinaria atencion)
        {
            await _context.AtencionesVeterinarias.AddAsync(atencion);
        }

        public async Task AgregarTratamiento(Tratamiento tratamiento)
        {
            await _context.Tratamientos.AddAsync(tratamiento);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<HistoriaClinica> GetHistoriaClinicaPorId(int id)
        {
            return await _context.HistoriasClinicas.FindAsync(id);
        }
    }
}
