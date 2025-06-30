using System.ComponentModel.DataAnnotations;

namespace SistemaVetIng.ViewsModels
{
    public class RecuperarContraseñaViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico no válido.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }
    }
}
