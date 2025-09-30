using System.ComponentModel.DataAnnotations;

namespace SistemaVetIng.ViewsModels
{
    public class ClienteRegistroViewModel
    {

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }


        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio")]
        public long Dni { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        public long Telefono { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
