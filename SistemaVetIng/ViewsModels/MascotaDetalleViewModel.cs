namespace SistemaVetIng.ViewsModels
{
    public class MascotaDetalleViewModel
    {
        // Datos de la Mascota
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Especie { get; set; }
        public string Raza { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Sexo { get; set; }
        public bool EsRazaPeligrosa { get; set; }
        public string ChipCodigo { get; set; } // Puede ser null

        // Datos del Propietario 
        public string PropietarioNombreCompleto { get; set; }

        // Historia Clinica
        public int HistoriaClinicaId { get; set; }
        public List<AtencionDetalleViewModel> HistorialClinico { get; set; } = new List<AtencionDetalleViewModel>();
    }
}
