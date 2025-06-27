namespace SistemaVetIng.Models
{
    public class Chip
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public Mascota Mascota { get; set; }
        public int MascotaId { get; set; }
    }
}
