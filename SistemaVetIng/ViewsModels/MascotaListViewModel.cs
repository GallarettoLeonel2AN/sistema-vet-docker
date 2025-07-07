namespace SistemaVetIng.ViewsModels
{
    public class MascotaListViewModel
    {
        public int Id { get; set; } 
        public string NombreMascota { get; set; }
        public string Especie { get; set; }
        public string Raza { get; set; }
        public string Sexo { get; set; }
        public int EdadAnios { get; set; }
        public bool RazaPeligrosa { get; set; }
        public string NombreDueno { get; set; } 
        public int ClienteId { get; set; } 
    }
}
