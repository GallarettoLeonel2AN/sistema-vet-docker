namespace SistemaVetIng.Models
{
    public class HistoriaClinica
    {
        public int Id { get; set; }
        public List<AtencionVeterinaria> Atenciones { get; set; }
        public Mascota Mascota { get; set; }
        public int MascotaId { get; set; }
    }
}
