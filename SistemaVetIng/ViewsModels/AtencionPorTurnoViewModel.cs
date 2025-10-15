using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SistemaVetIng.ViewsModels
{
    public class AtencionPorTurnoViewModel
    {
        [Required]
        public int TurnoId { get; set; }
        public int MascotaId { get; set; }
        public string NombreMascota { get; set; }
        public string NombreCliente { get; set; }

        [Required(ErrorMessage = "El diagnóstico es obligatorio.")]
        public string Diagnostico { get; set; }
        public decimal? PesoKg { get; set; }
        public string Medicamento { get; set; }
        public string Dosis { get; set; }
        public string Frecuencia { get; set; }
        public int? DuracionDias { get; set; }
        public string ObservacionesTratamiento { get; set; }

        public SelectList? VacunasDisponibles { get; set; }
        public SelectList? EstudiosDisponibles { get; set; }
        public object? VacunasConPrecio { get; set; }
        public object? EstudiosConPrecio { get; set; }
        public List<int> EstudiosSeleccionadosIds { get; set; } = new List<int>();
        public List<int> VacunasSeleccionadasIds { get; set; } = new List<int>();
    }
}
