
namespace SistemaVetIng.ViewsModels
{
    public class HistoriaClinicaViewModel
    {
        public int Id { get; set; }
        public string MascotaNombre { get; set; }
        public List<AtencionVeterinariaViewModel> Atenciones { get; set; } = new List<AtencionVeterinariaViewModel>();
    }
}
