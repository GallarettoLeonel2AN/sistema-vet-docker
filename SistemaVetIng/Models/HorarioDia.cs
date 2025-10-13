namespace SistemaVetIng.Models
{
    public class HorarioDia
    {
        public int Id { get; set; }
        public DayOfWeek DiaSemana { get; set; } 
        public bool EstaActivo { get; set; } 
        public DateTime? HoraInicio { get; set; } 
        public DateTime? HoraFin { get; set; }

        public int ConfiguracionVeterinariaId { get; set; }
        public virtual ConfiguracionVeterinaria ConfiguracionVeterinaria { get; set; }
    }
}

