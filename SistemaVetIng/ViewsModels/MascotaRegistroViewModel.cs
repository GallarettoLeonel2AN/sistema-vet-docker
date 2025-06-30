// Archivo: ViewsModels/MascotaRegistroViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SistemaVetIng.ViewsModels
{
    public class MascotaRegistroViewModel
    {
        // Propiedades de la Mascota
        [Required(ErrorMessage = "El nombre de la mascota es obligatorio.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La raza es obligatoria.")]
        public string Raza { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El sexo es obligatorio.")]
        public string Sexo { get; set; }

        [Display(Name = "Raza Peligrosa")]
        public bool RazaPeligrosa { get; set; }

        // Propiedad para el Id del cliente seleccionado
        [Required(ErrorMessage = "Debes seleccionar un cliente propietario.")]
        [Display(Name = "Propietario")]
        public int ClienteId { get; set; }

        // Esta propiedad se usará en la vista para crear la lista desplegable de clientes.
        public IEnumerable<SelectListItem>? Clientes { get; set; }
    }
}