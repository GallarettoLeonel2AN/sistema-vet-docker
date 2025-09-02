using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using SistemaVetIng.Models;
using System.Text;
using SistemaVetIng.Data;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class MascotaService : IMascotaService
    {
        private readonly IGeneralRepository<Mascota> _mascotaRepository;
        private readonly IGeneralRepository<Cliente> _clienteRepository;
        private readonly IGeneralRepository<Chip> _chipRepository;
        private readonly ApplicationDbContext _context;


        public MascotaService(IGeneralRepository<Mascota> mascotaRepository, IGeneralRepository<Cliente> clienteRepository, IGeneralRepository<Chip> chipRepository, ApplicationDbContext context)
        {
            _mascotaRepository = mascotaRepository;
            _clienteRepository = clienteRepository;
            _chipRepository = chipRepository;
            _context = context;
        }
        
        private readonly List<string> _razasPeligrosas = new List<string>
        {
            "pitbull", "rottweiler", "dogo argentino", "fila brasileiro",
            "akita inu", "tosa inu", "doberman", "staffordshire bull terrier",
            "american staffordshire terrier", "pastor alemán"
        };
        private bool IsRazaPeligrosa(string especie, string raza)
        {
            if (string.IsNullOrEmpty(especie) || string.IsNullOrEmpty(raza))
            {
                return false;
            }

            var especieLower = especie.ToLower().Trim();
            var razaLower = raza.ToLower().Trim();

            return especieLower == "perro" && _razasPeligrosas.Contains(razaLower);
        }


        public async Task<(bool success, string message)> Registrar(MascotaRegistroViewModel model)
        {
            // Validar la existencia del cliente antes de continuar.
            var clienteExiste = await _clienteRepository.ObtenerPorId(model.ClienteId);
            if (clienteExiste == null)
            {
                return (false, "El cliente asociado no es válido. Intente de nuevo.");
            }

            // Crear una nueva instancia de Mascota a partir del ViewModel.
            var mascota = new Mascota
            {
                Nombre = model.Nombre,
                Especie = model.Especie,
                Raza = model.Raza,
                FechaNacimiento = model.FechaNacimiento,
                Sexo = model.Sexo,
                RazaPeligrosa = IsRazaPeligrosa(model.Especie, model.Raza), // Asume que este método existe en el servicio.
                ClienteId = model.ClienteId,
                HistoriaClinica = new HistoriaClinica() // La HistoriaClinica se crea automáticamente.
            };

            // Lógica para el CHIP y la API de Perros Peligrosos.
            
            string apiMessage = string.Empty;
            bool apiCommunicationSuccess = true;
            if (mascota.RazaPeligrosa)
            {
                Chip chipAsociado = null;

                if (model.Chip)
                {
                    chipAsociado = new Chip
                    {
                        Codigo = Guid.NewGuid().ToString("N").Substring(0, 16),
                        Mascota = mascota
                    };
                    mascota.Chip = chipAsociado;
                }

                var clienteAsociado = await _clienteRepository.ObtenerPorId(model.ClienteId);

                // Llamada a la API externa.
                apiCommunicationSuccess = await EnviarApiPerrosPeligrosos(
                    mascota.Id,
                    mascota.Nombre,
                    mascota.Raza,
                    mascota.RazaPeligrosa,
                    model.Chip,
                    chipAsociado?.Codigo,
                    clienteAsociado.Dni,
                    clienteAsociado.Nombre,
                    clienteAsociado.Apellido
                );

                if (apiCommunicationSuccess)
                {
                    apiMessage = model.Chip
                        ? $"Chip Asociado (Código: {chipAsociado?.Codigo})."
                        : "Mascota peligrosa sin chip registrada en la API.";
                }
                else
                {
                    return (false, "Hubo un problema al comunicar con la API de perros peligrosos.");
                }
            }

            try
            {
                // Registrar la mascota y el chip (si aplica) de forma transaccional.
                await _mascotaRepository.Agregar(mascota);

                // Guardar cambios en el repositorio.
                await _mascotaRepository.Guardar();

                return (true, $"Mascota '{mascota.Nombre}' registrada correctamente. " + apiMessage);
            }
            catch (Exception ex)
            {
                // Devolver un error específico para manejar en la controladora.
                return (false, $"Error al registrar la mascota: {ex.Message}");
            }
        }


        public async Task<(bool success, string message)> Modificar(MascotaEditarViewModel model)
        {
            
            var mascota = await _mascotaRepository.ObtenerPorId(model.Id); 
            if (mascota == null)
            {
                return (false, "La mascota que intenta editar no existe.");
            }

           
            mascota.Nombre = model.Nombre;
            mascota.Especie = model.Especie;
            mascota.Raza = model.Raza;
            mascota.FechaNacimiento = model.FechaNacimiento;
            mascota.Sexo = model.Sexo;
            mascota.RazaPeligrosa = IsRazaPeligrosa(model.Especie, model.Raza); 

            try
            {
                
                if (mascota.RazaPeligrosa && model.Chip && mascota.Chip == null)
                {
                    // Caso 1: Se convirtió en peligrosa y se le agregó un chip.
                    var nuevoChip = new Chip
                    {
                        Codigo = Guid.NewGuid().ToString("N").Substring(0, 16),
                        MascotaId = mascota.Id
                    };
                    mascota.Chip = nuevoChip;
                    await _chipRepository.Agregar(nuevoChip); 
                }
                else if (mascota.RazaPeligrosa && !model.Chip && mascota.Chip != null)
                {
                    // Caso 2: Era peligrosa con chip y ahora ya no tiene chip.
                    _chipRepository.Eliminar(mascota.Chip);
                    mascota.Chip = null;
                }
                else if (!mascota.RazaPeligrosa && mascota.Chip != null)
                {
                    // Si no es peligrosa, aseguramos que el chip se elimine si existía.
                    _chipRepository.Eliminar(mascota.Chip);
                    mascota.Chip = null;
                }

                // 4. Llamar a la API si es una raza peligrosa.
                if (mascota.RazaPeligrosa)
                {
                    var clienteAsociado = await _clienteRepository.ObtenerPorId(model.ClienteId);
                    await EnviarApiPerrosPeligrosos(
                        mascota.Id,
                        mascota.Nombre,
                        mascota.Raza,
                        mascota.RazaPeligrosa,
                        model.Chip,
                        mascota.Chip?.Codigo,
                        clienteAsociado.Dni,
                        clienteAsociado.Nombre,
                        clienteAsociado.Apellido
                    );
                }

                // 5. Guardar los cambios.
                _mascotaRepository.Modificar(mascota);
                await _mascotaRepository.Guardar();

                return (true, $"Mascota '{mascota.Nombre}' actualizada correctamente.");
            }
            catch (Exception ex)
            {
                
                return (false, $"Error al actualizar la mascota: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> Eliminar(int id)
        {
            // Cargar la mascota y todas sus entidades relacionadas en un solo query.
            var mascota = await _context.Mascotas
                .Include(m => m.HistoriaClinica)
                    .ThenInclude(h => h.Atenciones)
                        .ThenInclude(a => a.Tratamiento)
                .Include(m => m.HistoriaClinica)
                    .ThenInclude(h => h.Atenciones)
                        .ThenInclude(a => a.Vacunas)
                .Include(m => m.HistoriaClinica)
                    .ThenInclude(h => h.Atenciones)
                        .ThenInclude(a => a.EstudiosComplementarios)
                .Include(m => m.Chip)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mascota == null)
            {
                return (false, "La mascota que intenta eliminar no existe.");
            }

            try
            {
                // Eliminar entidades relacionadas de las atenciones.
                if (mascota.HistoriaClinica?.Atenciones != null)
                {
                    foreach (var atencion in mascota.HistoriaClinica.Atenciones.ToList())
                    {
                        if (atencion.Tratamiento != null) _context.Tratamientos.Remove(atencion.Tratamiento);
                        if (atencion.Vacunas != null) _context.Vacunas.RemoveRange(atencion.Vacunas);
                        if (atencion.EstudiosComplementarios != null) _context.Estudios.RemoveRange(atencion.EstudiosComplementarios);
                    }
                   
                    _context.AtencionesVeterinarias.RemoveRange(mascota.HistoriaClinica.Atenciones);
                }

              
                if (mascota.HistoriaClinica != null)
                {
                    _context.HistoriasClinicas.Remove(mascota.HistoriaClinica);
                }

                
                if (mascota.Chip != null)
                {
                    _chipRepository.Eliminar(mascota.Chip);
                }
                
                _mascotaRepository.Eliminar(mascota);
 
                await _mascotaRepository.Guardar();

                return (true, "La mascota ha sido eliminada exitosamente.");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error al eliminar la mascota: {ex.Message}");
                return (false, "No se pudo eliminar la mascota. Hay registros asociados o un error de base de datos.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                return (false, "Ocurrió un error inesperado al eliminar la mascota.");
            }
        }

        public async Task<IEnumerable<Mascota>> ListarTodo()
        {
           return await _mascotaRepository.ListarTodo();
        }

        
        public async Task<Mascota> ObtenerPorId(int id)
        {
           return await _mascotaRepository.ObtenerPorId(id);
        }


        #region API PERROSPELIGROSOS
        // Metodo para enviar datos a la API de Perros Peligrosos
        private async Task<bool> EnviarApiPerrosPeligrosos(
            int mascotaId,
            string nombreMascota,
            string razaMascota,
            bool esRazaPeligrosa,
            bool tieneChip, // Si el checkbox fue marcado
            string chipCodigo,
            long clienteDni,
            string clienteNombre,
            string clienteApellido)
        {
            // URL API 
            var apiEndpoint = "http://localhost:5075/api/perros-peligrosos/registrar";

            // Objeto de datos a enviar a API
            var dataToSend = new
            {
                MascotaId = mascotaId,
                NombreMascota = nombreMascota,
                RazaMascota = razaMascota,
                EsRazaPeligrosa = esRazaPeligrosa,
                TieneChip = tieneChip,
                ChipCodigo = chipCodigo, // Será null si no tiene chip
                ClienteDni = clienteDni,
                ClienteNombre = clienteNombre,
                ClienteApellido = clienteApellido,
                FechaRegistro = DateTime.Now
            };

            using (var client = new HttpClient())
            {
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(dataToSend),
                    Encoding.UTF8,
                    "application/json"
                );

                try
                {
                    Console.WriteLine($"Enviando a API de Perros Peligrosos: {jsonContent.ReadAsStringAsync().Result}");
                    var response = await client.PostAsync(apiEndpoint, jsonContent);

                    // Si la API retorna un código de estado de exito
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Respuesta exitosa de la API: {await response.Content.ReadAsStringAsync()}");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Error de API ({response.StatusCode}): {await response.Content.ReadAsStringAsync()}");
                        return false;
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    // Errores de red,DNS,conexión rechazada,etc
                    Console.WriteLine($"Error de conexión HTTP con la API: {httpEx.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    // Otros errores (serialización,etc)
                    Console.WriteLine($"Error general al enviar datos a la API: {ex.Message}");
                    return false;
                }
            }
        }
        #endregion

    }
}
