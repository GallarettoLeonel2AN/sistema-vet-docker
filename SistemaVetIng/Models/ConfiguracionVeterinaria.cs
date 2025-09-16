namespace SistemaVetIng.Models
{
    public class ConfiguracionVeterinaria
    {
        public int Id { get; set; }
        public DateTime HoraInicio { get; set; }
        public DateTime HoraFin { get; set; }
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
