using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using System.Security.Claims;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class AtencionVeterinariaService : IAtencionVeterinariaService
    {
        private readonly IAtencionVeterinariaRepository _repository;

        public AtencionVeterinariaService(IAtencionVeterinariaRepository repository)
        {
            _repository = repository;
        }

        public async Task<AtencionVeterinariaViewModel> GetAtencionVeterinariaViewModel(int historiaClinicaId)
        {
            var historiaClinica = await _repository.GetHistoriaClinicaConMascotayPropietario(historiaClinicaId);

            if (historiaClinica == null)
            {
                return null;
            }

            var viewModel = new AtencionVeterinariaViewModel
            {
                HistoriaClinicaId = historiaClinicaId
            };

            // Pasar datos para la vista 
            viewModel.MascotaNombre = historiaClinica.Mascota.Nombre;
            viewModel.PropietarioNombre = $"{historiaClinica.Mascota.Propietario?.Nombre} {historiaClinica.Mascota.Propietario?.Apellido}";
            viewModel.MascotaId = historiaClinica.Mascota.Id;

            // Obtener datos para SelectList
            var vacunas = await _repository.GetVacunas();
            var estudios = await _repository.GetEstudios();

            viewModel.VacunasDisponibles = new SelectList(vacunas, "Id", "Nombre");
            viewModel.EstudiosDisponibles = new SelectList(estudios, "Id", "Nombre");
            viewModel.VacunasConPrecio = vacunas.Select(v => new { v.Id, v.Nombre, v.Precio }).ToList();
            viewModel.EstudiosConPrecio = estudios.Select(e => new { e.Id, e.Nombre, e.Precio }).ToList();

            return viewModel;
        }

        public async Task<string> CreateAtencionVeterinaria(AtencionVeterinariaViewModel model, ClaimsPrincipal user)
        {
            
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userIdInt))
            {
                return "Error al obtener el ID del usuario.";
            }

            var veterinario = await _repository.GetVeterinarioPorId(userIdInt);
            if (veterinario == null)
            {
                return "El usuario logueado no está asociado a un perfil de veterinario.";
            }

            model.VeterinarioId = veterinario.Id;

            // Obtener vacunas y estudios y calcular costos
            var vacunasSeleccionadas = await _repository.GetVacunaSeleccionada(model.VacunasSeleccionadasIds);
            var estudiosSeleccionados = await _repository.GetEstudioSeleccionado(model.EstudiosSeleccionadosIds);

            decimal costoVacunas = vacunasSeleccionadas.Sum(v => v.Precio);
            decimal costoEstudios = estudiosSeleccionados.Sum(e => e.Precio);
            decimal costoConsultaBase = 1000;
            decimal costoTotal = costoVacunas + costoConsultaBase + costoEstudios;

            // Crear tratamiento
            Tratamiento? tratamiento = null;
            if (!string.IsNullOrEmpty(model.Medicamento) || !string.IsNullOrEmpty(model.Dosis))
            {
                tratamiento = new Tratamiento
                {
                    Medicamento = model.Medicamento,
                    Dosis = model.Dosis,
                    Frecuencia = model.Frecuencia,
                    Duracion = model.DuracionDias,
                    Observaciones = model.ObservacionesTratamiento
                };
                await _repository.AgregarTratamiento(tratamiento);
            }

            // Crear la atención
            var atencion = new AtencionVeterinaria
            {
                Fecha = DateTime.Now,
                Diagnostico = model.Diagnostico,
                PesoMascota = model.PesoKg,
                HistoriaClinicaId = model.HistoriaClinicaId,
                VeterinarioId = model.VeterinarioId,
                Tratamiento = tratamiento,
                CostoTotal = costoTotal,
                Vacunas = vacunasSeleccionadas,
                EstudiosComplementarios = estudiosSeleccionados
            };

            await _repository.AgregarAtencionVeterinaria(atencion);
            await _repository.SaveChangesAsync();

            return null; 
        }
    }
}
