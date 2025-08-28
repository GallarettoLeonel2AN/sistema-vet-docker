using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SistemaVetIng.Models.Indentity
{
    public class Usuario : IdentityUser<int>
    {
        [Required]
        public string NombreUsuario { get; set; }
        
    }
}
