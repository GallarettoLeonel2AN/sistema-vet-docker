namespace SistemaVetIng.ViewsModels
{
    public class AtencionDetalleViewModel : AtencionVeterinariaViewModel
    {
        public int AtencionId { get; set; }
        public string? VeterinarioNombreCompleto { get; set; }
        public string EstadoDePago { get; set; } = "Pendiente";
        public decimal CostoTotal { get; set; }
        public List<string> NombresVacunasConLote { get; set; } = new List<string>();
        public List<string> NombresEstudios { get; set; } = new List<string>();
    }
}
