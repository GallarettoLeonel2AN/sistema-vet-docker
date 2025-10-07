using Microsoft.AspNetCore.Mvc;
using SistemaVetIng.ViewsModels; 
using System.Collections.Generic;

namespace SistemaVetIng.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Dashboard()
        {
            var viewModel = new DashboardViewModel();
         
            viewModel.VeterinariosDisponibles.Add(new DashboardViewModel.VeterinarioItem { Id = 1, Nombre = "Dr. Sosa (Mock)" });
            viewModel.VeterinariosDisponibles.Add(new DashboardViewModel.VeterinarioItem { Id = 2, Nombre = "Dra. Gallaretto (Mock)" });

            viewModel.TasaAusencia = 0.08m; // 8%
            viewModel.EstadoSemaforoAusencia = "Amarillo";
            viewModel.TotalIngresosPeriodo = 450000.75m;
            viewModel.IngresoPromedioPorTurno = 3500.00m;
            viewModel.NuevosClientesPeriodo = 18;
            viewModel.TotalMascotas = 560;
            viewModel.MascotasConChipCount = 410;


            // GRAFICO 1: Rendimiento de Turnos por Veterinario (chartRendimiento)
            viewModel.RendimientoVeterinarios = new List<DashboardViewModel.TurnosPorVeterinarioData>
            {
                new DashboardViewModel.TurnosPorVeterinarioData
                {
                    VeterinarioId = 1,
                    NombreVeterinario = "Dr. Sosa",
                    Finalizados = 120,
                    Cancelados = 15,
                    Pendientes = 5
                },
                new DashboardViewModel.TurnosPorVeterinarioData
                {
                    VeterinarioId = 2,
                    NombreVeterinario = "Dra. Gallaretto",
                    Finalizados = 85,
                    Cancelados = 8,
                    Pendientes = 10
                },
                new DashboardViewModel.TurnosPorVeterinarioData
                {
                    VeterinarioId = 3,
                    NombreVeterinario = "Dr. Lopez",
                    Finalizados = 150,
                    Cancelados = 25,
                    Pendientes = 2
                }
            };

            // GRAFICO 2: Distribución por Especie
            viewModel.DistribucionEspecies = new List<DashboardViewModel.EspecieCountData>
            {
                new DashboardViewModel.EspecieCountData { Especie = "Canino", Cantidad = 450 },
                new DashboardViewModel.EspecieCountData { Especie = "Felino", Cantidad = 90 },
                new DashboardViewModel.EspecieCountData { Especie = "Exótico", Cantidad = 20 }
            };

            // GRAFICO 3: Servicios Más Solicitados 
            viewModel.TopServicios = new List<DashboardViewModel.ServicioCountData>
            {
                new DashboardViewModel.ServicioCountData { NombreServicio = "Vacuna Quíntuple", CantidadSolicitudes = 45 },
                new DashboardViewModel.ServicioCountData { NombreServicio = "Consulta General", CantidadSolicitudes = 70 },
                new DashboardViewModel.ServicioCountData { NombreServicio = "Estudio Rayos X", CantidadSolicitudes = 25 }
            };


            // GRAFICO 4: Ingresos
            decimal metaAnual = 1800000.00m;
            decimal metaAnual2025 = 2000000.00m;

            // --- Hardcodeo de Datos Anuales (Nivel 1) ---
            var data2023 = new DashboardViewModel.IngresosAnualesData
            {
                Anio = "2023",
                IngresoRealAnual = 1000000m,
                MetaAnual = metaAnual,
                EstadoSemaforo = CalcularEstadoSemaforo(1000000m, metaAnual)
            };


            var data2024 = new DashboardViewModel.IngresosAnualesData
            {
                Anio = "2024",
                IngresoRealAnual = 1600000m,
                MetaAnual = metaAnual,
                EstadoSemaforo = CalcularEstadoSemaforo(1600000m, metaAnual)
            };

            var data2025 = new DashboardViewModel.IngresosAnualesData
            {
                Anio = "2025",
                IngresoRealAnual = 2000000m,
                MetaAnual = metaAnual2025,
                EstadoSemaforo = CalcularEstadoSemaforo(2000000m, metaAnual2025)
            };

            // Llenar Meses (Nivel 2) dentro del Año

            data2023.IngresosMensuales.Add(CrearDatosMensuales("Ene", 140000m, 110000m));
            data2023.IngresosMensuales.Add(CrearDatosMensuales("Feb", 155000m, 120000m));
            data2023.IngresosMensuales.Add(CrearDatosMensuales("Mar", 130000m, 150000m));
            data2023.IngresosMensuales.Add(CrearDatosMensuales("Abr", 110000m, 200000m));
            data2023.IngresosMensuales.Add(CrearDatosMensuales("May", 180000m, 200000m));
            data2023.IngresosMensuales.Add(CrearDatosMensuales("Jun", 90000m, 200000m));


            data2024.IngresosMensuales.Add(CrearDatosMensuales("Ene", 140000m, 110000m));
            data2024.IngresosMensuales.Add(CrearDatosMensuales("Feb", 155000m, 140000m));
            data2024.IngresosMensuales.Add(CrearDatosMensuales("Mar", 130000m, 200000m));
            data2024.IngresosMensuales.Add(CrearDatosMensuales("Abr", 110000m, 200000m));
            data2024.IngresosMensuales.Add(CrearDatosMensuales("May", 180000m, 200000m));
            data2024.IngresosMensuales.Add(CrearDatosMensuales("Jun", 220000m, 200000m));


            data2025.IngresosMensuales.Add(CrearDatosMensuales("Ene", 1140000m, 2000000m));
            data2025.IngresosMensuales.Add(CrearDatosMensuales("Feb", 2155000m, 2500000m));
            data2025.IngresosMensuales.Add(CrearDatosMensuales("Mar", 5130000m, 3000000m));
            data2025.IngresosMensuales.Add(CrearDatosMensuales("Abr", 1210000m, 3000000m));
            data2025.IngresosMensuales.Add(CrearDatosMensuales("May", 1180000m, 3000000m));
            data2025.IngresosMensuales.Add(CrearDatosMensuales("Jun", 9000000m, 3000000m));

            viewModel.IngresosAnuales.Add(data2023);
            viewModel.IngresosAnuales.Add(data2024);
            viewModel.IngresosAnuales.Add(data2025);


            return View(viewModel);
        }

        private DashboardViewModel.IngresosMensualesData CrearDatosMensuales(string mes, decimal real, decimal meta)
        {
            var estado = CalcularEstadoSemaforo(real, meta);

            var mesData = new DashboardViewModel.IngresosMensualesData
            {
                Mes = mes,
                IngresoRealMensual = real,
                MetaMensual = meta,
                EstadoSemaforo = estado,
                IngresosSemanales = new List<DashboardViewModel.IngresosSemanalesData>()
            };

            // --- Hardcodeo de 4 Semanas (Nivel 3) ---
            decimal metaSemanal = meta / 4;
            mesData.IngresosSemanales.Add(new DashboardViewModel.IngresosSemanalesData { Semana = "Semana 1", EstadoSemaforo = estado, MetaSemanal = metaSemanal, IngresoRealSemanal = real * 0.23m });
            mesData.IngresosSemanales.Add(new DashboardViewModel.IngresosSemanalesData { Semana = "Semana 2", EstadoSemaforo = estado, MetaSemanal = metaSemanal, IngresoRealSemanal = real * 0.27m });
            mesData.IngresosSemanales.Add(new DashboardViewModel.IngresosSemanalesData { Semana = "Semana 3", EstadoSemaforo = estado, MetaSemanal = metaSemanal, IngresoRealSemanal = real * 0.25m });
            mesData.IngresosSemanales.Add(new DashboardViewModel.IngresosSemanalesData { Semana = "Semana 4", EstadoSemaforo = estado,MetaSemanal = metaSemanal, IngresoRealSemanal = real * 0.25m });

            return mesData;
        }

        private string CalcularEstadoSemaforo(decimal real, decimal meta)
        {
            // Previene la división por cero
            decimal cumplimiento = (meta > 0) ? real / meta : 0;
            string estado = "rojo";

            // Lógica del Semáforo:
            if (cumplimiento >= 0.95m)
            {
                estado = "verde"; // Más del 95% = Éxito
            }
            else if (cumplimiento >= 0.85m)
            {
                estado = "amarillo"; // Entre 85% y 95% = Advertencia
            }

            return estado;
        }
    }
}