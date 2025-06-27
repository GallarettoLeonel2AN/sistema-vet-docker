namespace SistemaVetIng.Models
{
    public class Estudio
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Informe { get; set; }
        public decimal Precio { get; set; }

        public int AtencionVeterinariaId { get; set; }
        public AtencionVeterinaria AtencionVeterinaria { get; set; }
    }
}
