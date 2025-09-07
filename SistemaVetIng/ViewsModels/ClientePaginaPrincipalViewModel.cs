using SistemaVetIng.Models;

namespace SistemaVetIng.ViewsModels
{
    public class ClientePaginaPrincipalViewModel
    {
        public List<MascotaListViewModel> Mascotas { get; set; }
        public ClienteViewModel Cliente { get; set; }

        // Reportes simulados / indicadores
        public int CantidadTurnosReservados { get; set; }
        public int CantidadTurnosCancelados { get; set; }
        public string VeterinarioMasFrecuente { get; set; }

        public HistoriaClinicaViewModel HistoriaClinicas { get; set; } = new HistoriaClinicaViewModel();

        public ClientePaginaPrincipalViewModel()
        {
            Mascotas = new List<MascotaListViewModel>();
            Cliente = new ClienteViewModel();
        }
    }
}
