namespace SistemaVetIng.Models
{
    public class Tratamiento
    {
        public int Id { get; set; }
        public string Indicaciones { get; set; }
        public decimal Precio { get; set; }
        public string Observaciones { get; set; }

        public string Medicamento { get; set; }

        public string Dosis { get; set; }

        public string Frecuencia { get; set; }

        public string Duracion { get; set; }
        
    }
}
