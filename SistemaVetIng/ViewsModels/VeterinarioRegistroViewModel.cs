
using System.ComponentModel.DataAnnotations;

namespace SistemaVetIng.ViewsModels
{
    public class VeterinarioRegistroViewModel
    {
        
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y un máximo de {1} caracteres.", MinimumLength = 6)]
        public string Password { get; set; }

        
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100)]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        
        public long Dni { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(200)]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
       
        public long Telefono { get; set; }

        
        [Required(ErrorMessage = "La matrícula es obligatoria.")]
        [StringLength(50)]
        public string Matricula { get; set; }
    }
}