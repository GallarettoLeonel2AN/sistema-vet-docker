using SistemaVetIng.Models.Indentity;

namespace SistemaVetIng.Models
{
    public class Veterinario : Persona
    {
       
        public string Matricula { get; set; }
        public string Direccion { get; set; }
        public List<Turno> Turnos { get; set; }
        public List<Disponibilidad> Disponibilidades { get; set; }
    }
}
