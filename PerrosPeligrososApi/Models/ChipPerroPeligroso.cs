using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PerrosPeligrososApi.Models
{
    public class ChipPerroPeligroso
    {
        // La clave primaria  es la FK a PerroPeligroso
        [Key]
        [ForeignKey("PerroPeligroso")]
        public int PerroPeligrosoId { get; set; } 

        [Required]
        [StringLength(50)]
        public string Codigo { get; set; } 

        public PerroPeligroso PerroPeligroso { get; set; }
    }
}