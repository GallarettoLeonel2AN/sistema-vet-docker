using SistemaVetIng.Models.Indentity;

namespace SistemaVetIng.Models
{
    public abstract class Persona
    {
        public int Id { get; set; }
        public Usuario Usuario { get; set; }
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public long Telefono { get; set; }
        public long Dni { get; set; }
    }
}
