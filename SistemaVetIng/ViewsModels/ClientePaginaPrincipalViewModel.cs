namespace SistemaVetIng.ViewsModels
{
    public class ClientePaginaPrincipalViewModel
    {
        public string NombreCompleto { get; set; }
        public List<MascotaListViewModel> Mascotas { get; set; }
        public ClienteViewModel Cliente { get; set; }
        public List<TurnoViewModel> Turnos { get; set; }

        // Reportes simulados / indicadores
        public int CantidadTurnosReservados { get; set; }
        public int CantidadTurnosCancelados { get; set; }
        public string VeterinarioMasFrecuente { get; set; }

        public HistoriaClinicaViewModel HistoriaClinicas { get; set; } = new HistoriaClinicaViewModel();

        public ClientePaginaPrincipalViewModel()
        {
            Mascotas = new List<MascotaListViewModel>();
            Cliente = new ClienteViewModel();
            Turnos = new List<TurnoViewModel>();
        }
    }
}
