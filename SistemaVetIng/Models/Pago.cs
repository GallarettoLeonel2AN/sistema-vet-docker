namespace SistemaVetIng.Models
{
    public class Pago
    {
        public int Id { get; set; }
        public AtencionVeterinaria AtencionVeterinaria { get; set; }
        public Cliente Cliente { get; set; }
        public DateTime Fecha { get; set; }
        public MetodoPago MetodoPago { get; set; }
    }
}
