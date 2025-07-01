namespace SistemaVetIng.Models
{
    public class Mascota
    {
        public int Id { get; set; }
        public string Especie { get; set; }
        public string Nombre { get; set; }
        public string Raza { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Sexo { get; set; }
        public bool RazaPeligrosa { get; set; }
        public Cliente Propietario { get; set; }
        public int ClienteId { get; set; }
        public Chip? Chip { get; set; }
        public HistoriaClinica HistoriaClinica { get; set; }
    }
}
