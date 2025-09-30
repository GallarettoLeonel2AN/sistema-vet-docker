using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PerrosPeligrososApi.Models
{
    public class PerroPeligroso
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        public string Raza { get; set; }
        public int MascotaIdOriginal { get; set; }

        // Información del cliente que registró esta mascota 
        public long ClienteDni { get; set; }
        [StringLength(100)]
        public string ClienteNombre { get; set; }
        [StringLength(100)]
        public string ClienteApellido { get; set; }

        public DateTime FechaRegistroApi { get; set; } = DateTime.Now; // Fecha en que se registró 

        public ChipPerroPeligroso? Chip { get; set; } // Puede ser nulo si no tiene chip
    }
}