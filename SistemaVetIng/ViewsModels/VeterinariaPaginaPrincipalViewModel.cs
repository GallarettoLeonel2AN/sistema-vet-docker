namespace SistemaVetIng.ViewsModels
{
    public class VeterinariaPaginaPrincipalViewModel
    {
        public List<VeterinarioViewModel> Veterinarios { get; set; }
        public DisponibilidadViewModel ConfiguracionTurnos { get; set; }
        public List<ClienteViewModel> Clientes { get; set; } 
        public List<MascotaListViewModel> Mascotas { get; set; } 

        // Propiedades para los reportes analíticos !!Simulado!!
        public int CantidadPerrosPeligrosos { get; set; }
        public string RazaMayorDemanda { get; set; } 
        public decimal IngresosMensualesEstimados { get; set; } 
        public decimal IngresosDiariosEstimados { get; set; } 

        public VeterinariaPaginaPrincipalViewModel()
        {
            Veterinarios = new List<VeterinarioViewModel>();
            Clientes = new List<ClienteViewModel>();
            Mascotas = new List<MascotaListViewModel>();
            ConfiguracionTurnos = new DisponibilidadViewModel(); // Inicializar para evitar NullReferenceException
        }
    }
}
