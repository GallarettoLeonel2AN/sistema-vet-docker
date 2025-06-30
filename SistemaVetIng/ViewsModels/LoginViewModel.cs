// Models/ViewModels/LoginViewModel.cs (o AuthViewModels/LoginViewModel.cs)
using System.ComponentModel.DataAnnotations;

namespace SistemaVetIng.ViewModels // Ajusta el namespace según tu proyecto
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [Display(Name = "Usuario")]
        public string UserName { get; set; } // O Email, si usas email para login

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Display(Name = "¿Recordarme?")]
        public bool RememberMe { get; set; }
    }
}