namespace SistemaVetIng.Models
{
    public class Disponibilidad
    {
        public int Id { get; set; }
        public Veterinario Veterinario { get; set; }
        public int VeterinarioId { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public int DuracionMinutosPorConsulta { get; set; }
        public bool TrabajaLunes { get; set; } 
        public bool TrabajaMartes { get; set; } 
        public bool TrabajaMiercoles { get; set; } 
        public bool TrabajaJueves { get; set; } 
        public bool TrabajaViernes { get; set; } 
        public bool TrabajaSabado { get; set; } 
        public bool TrabajaDomingo { get; set; } 
    }
}
