namespace SistemaVetIng.ViewsModels
{
    public class VeterinarioPaginaPrincipalViewModel
    {
        public List<ClienteViewModel> Clientes { get; set; }
        public List<MascotaListViewModel> Mascotas { get; set; }

        public VeterinarioPaginaPrincipalViewModel()
        {
            Clientes = new List<ClienteViewModel>();
            Mascotas = new List<MascotaListViewModel>();
        }
    }
}
