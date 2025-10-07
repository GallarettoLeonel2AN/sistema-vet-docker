namespace SistemaVetIng.ViewsModels
{
    public class DashboardViewModel
    {
  
        public List<VeterinarioItem> VeterinariosDisponibles { get; set; } = new List<VeterinarioItem>();

        // Tasa de Ausencia (Necesario para la Semaforización)
        public decimal TasaAusencia { get; set; } // Valor entre 0 y 1.0 (Ej: 0.08 para 8%)
        public string EstadoSemaforoAusencia { get; set; } // "Verde", "Amarillo", "Rojo"

        // Ingresos
        public decimal IngresoPromedioPorTurno { get; set; }
        public decimal TotalIngresosPeriodo { get; set; }

        // Crecimiento y Clientes
        public int NuevosClientesPeriodo { get; set; }

        // Mascota
        public int TotalMascotas { get; set; }
        public int MascotasConChipCount { get; set; }


        // Grafico 1: Rendimiento de Turnos por Veterinario (Punto de inicio del Drill Down)
        public List<TurnosPorVeterinarioData> RendimientoVeterinarios { get; set; } = new List<TurnosPorVeterinarioData>();

        // Grafico 2: Distribución por Especie (Punto de inicio del Drill Down Secundario)
        public List<EspecieCountData> DistribucionEspecies { get; set; } = new List<EspecieCountData>();

        // Grafico 3: Servicios mas Solicitados
        public List<ServicioCountData> TopServicios { get; set; } = new List<ServicioCountData>();

        // Detalle Mascotas
        public int PeligrososCount { get; set; }
        public int NoPeligrososCount { get; set; }
        public string RazaMayorDemanda { get; set; }



        // Usada en RendimientoVeterinarios (Grafico 1)
        public class TurnosPorVeterinarioData
        {
            public int VeterinarioId { get; set; }
            public string NombreVeterinario { get; set; }
            public int Finalizados { get; set; }
            public int Cancelados { get; set; }
            public int Pendientes { get; set; }
        }

        // Usada en DistribucionEspecies (Grafico 2)
        public class EspecieCountData
        {
            public string Especie { get; set; } 
            public int Cantidad { get; set; }
        }

        // Usada en TopServicios (Grafico 3)
        public class ServicioCountData
        {
            public string NombreServicio { get; set; } 
            public int CantidadSolicitudes { get; set; }
        }

        // Usada para precargar el Dropdown de Filtros
        public class VeterinarioItem
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
        }

        // Ingresos

        public List<IngresosAnualesData> IngresosAnuales { get; set; } = new List<IngresosAnualesData>();

        // Nivel 1: Datos Anuales
        public class IngresosAnualesData
        {
            public string Anio { get; set; } 
            public decimal IngresoRealAnual { get; set; }
            public decimal MetaAnual { get; set; }
            public string EstadoSemaforo { get; set; }
            public List<IngresosMensualesData> IngresosMensuales { get; set; } = new List<IngresosMensualesData>();
        }

        // Nivel 2: Datos Mensuales
        public class IngresosMensualesData
        {
            public string Mes { get; set; } 
            public decimal IngresoRealMensual { get; set; }
            public decimal MetaMensual { get; set; }
            public string EstadoSemaforo { get; set; } 
            public List<IngresosSemanalesData> IngresosSemanales { get; set; } = new List<IngresosSemanalesData>();
        }

        // Nivel 3: Datos Semanales
        public class IngresosSemanalesData
        {
            public string Semana { get; set; } 
            public decimal IngresoRealSemanal { get; set; }
            public decimal MetaSemanal { get; set; }
            public string EstadoSemaforo { get; set; }
        }

    }
}