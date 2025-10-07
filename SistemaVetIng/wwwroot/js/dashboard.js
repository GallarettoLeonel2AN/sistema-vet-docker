$(document).ready(function () {

    // Almacenamos los objetos de Chart.js en variables globales para poder destruirlos
    // y redibujarlos (necesario para el drill-down y el cambio de filtros).
    let rendimientoChart;
    let serviciosChart;
    let especiesChart;
    let ingresosChart;

    // --- Variables de Estado para el DRILL-DOWN de INGRESOS (3 NIVELES) ---
    let currentIngresosLevel = 1; // 1: Anual, 2: Mensual, 3: Semanal
    let selectedAnio = null;
    let selectedMes = null;
    // ----------------------------------------------------------------------

    // Estado para el Drill Down de Rendimiento: Nivel 1 = Vets; Nivel 2 = Turnos del Vet seleccionado
    let currentDrillDownLevel = 1;
    let selectedVeterinarioId = null;

    // -----------------------------------------------------------------
    // 1. DIBUJO DE GRÁFICOS (Funciones de renderizado)
    // -----------------------------------------------------------------

    // ************* TUS FUNCIONES EXISTENTES (renderRendimientoChart, renderServiciosChart, renderEspeciesChart) *************

    /**
     * Dibuja el gráfico de Rendimiento de Turnos por Veterinario (Drill Down Nivel 1).
     * @param {Array} data - Lista de objetos TurnosPorVeterinarioData.
     */
    function renderRendimientoChart(data) {
        const ctx = document.getElementById('chartRendimiento').getContext('2d');

        if (rendimientoChart) {
            rendimientoChart.destroy();
        }

        const labels = data.map(item => item.nombreVeterinario);
        const finalizados = data.map(item => item.finalizados);
        const cancelados = data.map(item => item.cancelados);
        const pendientes = data.map(item => item.pendientes);

        rendimientoChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Finalizados',
                        data: finalizados,
                        backgroundColor: '#4CAF50', // Verde
                        stack: 'stack1'
                    },
                    {
                        label: 'Pendientes',
                        data: pendientes,
                        backgroundColor: '#FFC107', // Amarillo
                        stack: 'stack1'
                    },
                    {
                        label: 'Cancelados',
                        data: cancelados,
                        backgroundColor: '#F44336', // Rojo
                        stack: 'stack1'
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        stacked: true,
                    },
                    y: {
                        stacked: true,
                        beginAtZero: true
                    }
                },
                plugins: {
                    title: {
                        display: true,
                        text: 'Turnos Totales por Veterinario'
                    }
                },
                // Lógica de CLICK para el Drill Down
                onClick: (e) => {
                    const points = rendimientoChart.getElementsAtEventForMode(e, 'nearest', { intersect: true }, true);
                    if (points.length) {
                        const firstPoint = points[0];
                        const index = firstPoint.index;
                        const vetData = data[index];

                        // Si estamos en el Nivel 1 (Vets), hacemos Drill Down
                        if (currentDrillDownLevel === 1) {
                            // Simulamos el paso a Nivel 2 . LUEGO LLAMAREMOS A AJAX
                            handleDrillDownToLevel2(vetData.veterinarioId, vetData.nombreVeterinario);
                        }
                    }
                }
            }
        });

        // Ocultar el botón "Volver" en el Nivel 1
        $('#drillUpButton').hide();
        $('#drillDownTitle').text('Rendimiento de Turnos por Veterinario (Nivel 1)');
    }


    /**
     * Dibuja el gráfico de Servicios Más Solicitados.
     * @param {Array} data - Lista de objetos ServicioCountData.
     */
    function renderServiciosChart(data) {
        const ctx = document.getElementById('chartServicios').getContext('2d');

        if (serviciosChart) {
            serviciosChart.destroy();
        }

        serviciosChart = new Chart(ctx, {
            type: 'pie',
            data: {
                labels: data.map(item => item.nombreServicio),
                datasets: [{
                    label: 'Cantidad de Solicitudes',
                    data: data.map(item => item.cantidadSolicitudes),
                    backgroundColor: ['#008CBA', '#4CAF50', '#F44336', '#FFC107', '#9C27B0'],
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: {
                        display: true,
                        text: 'Distribución de Servicios (Vacunas y Estudios)'
                    }
                }
            }
        });
    }

    /**
     * Dibuja el gráfico de Distribución por Especie (Gráfico faltante).
     * @param {Array} data - Lista de objetos EspecieCountData.
     */
    function renderEspeciesChart(data) {
        const ctx = document.getElementById('chartEspecies').getContext('2d');

        if (especiesChart) {
            especiesChart.destroy();
        }

        // Colores base para las especies
        const colors = [
            'rgba(75, 192, 192, 0.8)',
            'rgba(255, 159, 64, 0.8)',
            'rgba(153, 102, 255, 0.8)',
            'rgba(255, 99, 132, 0.8)'
        ];

        especiesChart = new Chart(ctx, {
            type: 'doughnut', // Gráfico de Dona para distribución
            data: {
                labels: data.map(item => item.especie),
                datasets: [{
                    label: 'Mascotas por Especie',
                    data: data.map(item => item.cantidad),
                    backgroundColor: colors.slice(0, data.length),
                    hoverOffset: 8
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: {
                        display: true,
                        text: 'Distribución por Especie de Mascotas'
                    }
                },
                // Lógica de CLICK para el Drill Down de Razas
                onClick: (e) => {
                    const points = especiesChart.getElementsAtEventForMode(e, 'nearest', { intersect: true }, true);
                    if (points.length) {
                        const firstPoint = points[0];
                        const index = firstPoint.index;
                        const especieSeleccionada = data[index].especie;

                        // Aquí vamos a hacer la llamada AJAX para obtener las razas de esa especie
                        updateRazasList(especieSeleccionada);
                    }
                }
            }
        });
    }

    // ***************************************************************************************************
    // 🚀 LÓGICA DE INGRESO: Nivel 1 (Anual) -> Nivel 2 (Mensual) -> Nivel 3 (Semanal)
    // ***************************************************************************************************

    /**
     * Dibuja el gráfico de Ingresos: Nivel 1 (Anual).
     * @param {Array} data - Lista de objetos IngresosAnualesData.
     */
    function renderIngresosNivel1Chart(data) {
        const ctx = document.getElementById('chartIngresosMensuales').getContext('2d');

        if (ingresosChart) {
            ingresosChart.destroy();
        }

        currentIngresosLevel = 1;

        const labels = data.map(d => d.anio);
        const ingresosReales = data.map(d => d.ingresoRealAnual);
        const metas = data.map(d => d.metaAnual);

        const backgroundColors = data.map(d => {
            if (d.estadoSemaforo === 'verde') return 'rgba(76, 175, 80, 0.8)'; // Verde
            if (d.estadoSemaforo === 'amarillo') return 'rgba(255, 193, 7, 0.8)'; // Amarillo
            return 'rgba(244, 67, 54, 0.8)'; // Rojo
        });

        ingresosChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Ingreso Real',
                        data: ingresosReales,
                        backgroundColor: backgroundColors,
                        yAxisID: 'y'
                    },
                    {
                        type: 'line',
                        label: 'Meta Anual',
                        data: metas,
                        borderColor: 'rgb(255, 99, 132)',
                        borderWidth: 3,
                        fill: false,
                        pointRadius: 5,
                        yAxisID: 'y'
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function (value) {
                                return '$' + value.toLocaleString();
                            }
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: true,
                    },
                    title: {
                        display: true,
                        text: 'Rendimiento de Ingresos: Visión Anual'
                    }
                },
                onClick: (e) => {
                    const points = ingresosChart.getElementsAtEventForMode(e, 'nearest', { intersect: true }, true);
                    if (points.length) {
                        const index = points[0].index;
                        selectedAnio = data[index].anio;

                        // PASO AL NIVEL 2: MESES
                        handleIngresosDrillDownToLevel2(data[index].ingresosMensuales);
                    }
                }
            }
        });
        $('#ingresosDrillUpButton').hide();
        $('#ingresosDrillDownTitle').text('Rendimiento de Ingresos: Visión Anual');
    }

    /**
     * Dibuja el gráfico de Ingresos: Nivel 2 (Mensual).
     * @param {Array} data - Lista de objetos IngresosMensualesData.
     */
    function renderIngresosNivel2Chart(data) {
        const ctx = document.getElementById('chartIngresosMensuales').getContext('2d');
        if (ingresosChart) { ingresosChart.destroy(); }

        currentIngresosLevel = 2;

        const labels = data.map(d => d.mes);
        const ingresosReales = data.map(d => d.ingresoRealMensual);
        const metas = data.map(d => d.metaMensual);

        // Mapeo para colores de barras según el estado del semáforo
        const backgroundColors = data.map(d => {
            if (d.estadoSemaforo === 'verde') return 'rgba(76, 175, 80, 0.8)'; // Verde
            if (d.estadoSemaforo === 'amarillo') return 'rgba(255, 193, 7, 0.8)'; // Amarillo
            return 'rgba(244, 67, 54, 0.8)'; // Rojo
        });

        ingresosChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Ingreso Real',
                        data: ingresosReales,
                        backgroundColor: backgroundColors,
                        yAxisID: 'y'
                    },
                    {
                        type: 'line',
                        label: 'Meta Mensual',
                        data: metas,
                        borderColor: 'rgb(0, 140, 186)',
                        borderWidth: 3,
                        fill: false,
                        pointRadius: 0,
                        yAxisID: 'y'
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function (value) { return '$' + value.toLocaleString(); }
                        }
                    }
                },
                plugins: {
                    title: {
                        display: true,
                        text: `Ingresos Mensuales de ${selectedAnio}`
                    }
                },
                onClick: (e) => {
                    const points = ingresosChart.getElementsAtEventForMode(e, 'nearest', { intersect: true }, true);
                    if (points.length) {
                        const index = points[0].index;
                        selectedMes = data[index].mes; // Guardamos el mes

                        // PASO AL NIVEL 3: SEMANAS
                        handleIngresosDrillDownToLevel3(data[index].ingresosSemanales);
                    }
                }
            }
        });
        $('#ingresosDrillUpButton').show();
        $('#ingresosDrillDownTitle').text(`Ingresos Mensuales de ${selectedAnio}`);
    }

    /**
     * Dibuja el gráfico de Ingresos: Nivel 3 (Semanal).
     * @param {Array} data - Lista de objetos IngresosSemanalesData.
     */
    function renderIngresosNivel3Chart(data) {
        const ctx = document.getElementById('chartIngresosMensuales').getContext('2d');
        if (ingresosChart) { ingresosChart.destroy(); }

        currentIngresosLevel = 3;

        const labels = data.map(item => item.semana);
        const ingresos = data.map(item => item.ingresoRealSemanal);
        const metas = data.map(item => item.metaSemanal);

        const backgroundColors = data.map(d => {
            if (d.estadoSemaforo === 'verde') return 'rgba(76, 175, 80, 0.8)'; // Verde
            if (d.estadoSemaforo === 'amarillo') return 'rgba(255, 193, 7, 0.8)'; // Amarillo
            return 'rgba(244, 67, 54, 0.8)'; // Rojo
        });


        ingresosChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Ingresos Reales',
                        data: ingresos,
                        backgroundColor: backgroundColors,
                        yAxisID: 'y'
                    },
                    {
                        type: 'line',
                        label: 'Meta Semanal',
                        data: metas,
                        borderColor: 'rgb(255, 99, 132)',
                        borderWidth: 2,
                        fill: false,
                        pointRadius: 0,
                        yAxisID: 'y'
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function (value) { return '$' + value.toLocaleString(); }
                        }
                    }
                },
                plugins: {
                    title: {
                        display: true,
                        text: `Ingresos Semanales de ${selectedMes} (${selectedAnio})`
                    }
                }
            }
        });
        $('#ingresosDrillDownTitle').text(`Ingresos Semanales de ${selectedMes} (${selectedAnio})`);
    }

    // ************* FIN DE FUNCIONES DE RENDERIZADO DE INGRESOS *************

    // -----------------------------------------------------------------
    // 2. LÓGICA DE INTERACCIÓN (Filtros y Drill Down)
    // -----------------------------------------------------------------

    // ************* Lógica de Rendimiento de Vets (handleDrillDownToLevel2, renderTurnosDetalleChart, handleDrillUp) *************

    /**
     * Simula (o manejará) la transición al Drill Down Nivel 2: Turnos de un Vet específico.
     */
    function handleDrillDownToLevel2(vetId, vetName) {
        selectedVeterinarioId = vetId;
        currentDrillDownLevel = 2;

        // --- SIMULACIÓN DE DATOS (AQUÍ hariamos la llamada AJAX) ---
        const level2Data = [
            { nombreVeterinario: 'Semana 1', finalizados: 30, cancelados: 5, pendientes: 1 },
            { nombreVeterinario: 'Semana 2', finalizados: 35, cancelados: 2, pendientes: 2 },
            { nombreVeterinario: 'Semana 3', finalizados: 25, cancelados: 7, pendientes: 0 },
            { nombreVeterinario: 'Semana 4', finalizados: 40, cancelados: 1, pendientes: 0 }
        ];

        // Redibujamos el gráfico
        renderTurnosDetalleChart(level2Data, vetName);

        // Mostrar el botón "Volver"
        $('#drillUpButton').show();
    }

    /**
     * Dibuja el gráfico de detalle (Nivel 2) para un veterinario.
     */
    function renderTurnosDetalleChart(data, vetName) {
        const ctx = document.getElementById('chartRendimiento').getContext('2d');

        if (rendimientoChart) {
            rendimientoChart.destroy();
        }

        const labels = data.map(item => item.nombreVeterinario);
        const finalizados = data.map(item => item.finalizados);

        rendimientoChart = new Chart(ctx, {
            type: 'line', // Usamos línea para ver la tendencia de las semanas
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Turnos Finalizados',
                        data: finalizados,
                        borderColor: '#008CBA',
                        backgroundColor: 'rgba(0, 140, 186, 0.2)',
                        fill: true,
                        tension: 0.4
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true
                    }
                },
                plugins: {
                    title: {
                        display: true,
                        text: `Detalle de Turnos Finalizados para: ${vetName}`
                    }
                }
            }
        });

        $('#drillDownTitle').text(`Detalle Semanal - ${vetName} (Nivel 2)`);
    }

    /**
     * Simula (o maneja) la transición de vuelta al Drill Down Nivel 1 (Vets).
     */
    function handleDrillUp() {
        if (currentDrillDownLevel > 1) {
            currentDrillDownLevel = 1;
            selectedVeterinarioId = null;
            // Recargamos los datos base. Usamos initialRendimientoData que viene del ViewModel.
            renderRendimientoChart(initialRendimientoData);
        }
    }

    // Asignar evento al botón "Volver (Drill Up)"
    $('#drillUpButton').on('click', handleDrillUp);

    // ************* FIN Lógica de Rendimiento de Vets *************

    // --- Lógica de Navegación de Ingresos (Drill Up) ---

    /**
     * Simula la transición al Drill Down Nivel 2: Meses.
     * @param {Array} monthlyData - Lista de objetos IngresosMensualesData.
     */
    function handleIngresosDrillDownToLevel2(monthlyData) {
        renderIngresosNivel2Chart(monthlyData);
    }

    /**
     * Simula la transición al Drill Down Nivel 3: Semanas.
     * @param {Array} weeklyData - Lista de objetos IngresosSemanalesData.
     */
    function handleIngresosDrillDownToLevel3(weeklyData) {
        renderIngresosNivel3Chart(weeklyData);
    }

    /**
     * Maneja la transición de vuelta a los niveles superiores.
     */
    function handleIngresosDrillUp() {
        // En el Nivel 3 (Semanal), volvemos al Nivel 2 (Mensual)
        if (currentIngresosLevel === 3) {
            const anioData = initialIngresosAnualesData.find(d => d.anio === selectedAnio);
            if (anioData) {
                renderIngresosNivel2Chart(anioData.ingresosMensuales);
            }
            // En el Nivel 2 (Mensual), volvemos al Nivel 1 (Anual)
        } else if (currentIngresosLevel === 2) {
            selectedAnio = null;
            selectedMes = null;
            renderIngresosNivel1Chart(initialIngresosAnualesData);
        }
        // Si es Nivel 1, el botón está oculto por renderIngresosNivel1Chart
    }

    // Asignar evento al botón "Volver (Drill Up)" específico de Ingresos
    $('#ingresosDrillUpButton').on('click', handleIngresosDrillUp);

    // ************* Simulación de Razas *************

    /**
     * Simula la actualización de la lista de razas basada en la especie seleccionada
     */
    function updateRazasList(especie) {
        let html = '';

        // Simulación de datos de Razas (Aca luego hacemos llamada a ajax)
        const mockRazas = {
            'Canino': [
                { raza: 'Ovejero Alemán', count: 45 },
                { raza: 'Labrador', count: 30 },
                { raza: 'Mestizo', count: 20 }
            ],
            'Felino': [
                { raza: 'Siamés', count: 15 },
                { raza: 'Persa', count: 10 },
                { raza: 'Común Europeo', count: 50 }
            ],
            'Exótico': [
                { raza: 'Conejo', count: 10 },
                { raza: 'Hámster', count: 5 }
            ]
        };

        const razasData = mockRazas[especie] || [{ raza: 'No hay datos de razas', count: 0 }];

        razasData.forEach(item => {
            html += `<div class="v1-list-item">
                        <span>${item.raza}</span>
                        <span class="v1-badge">${item.count}</span>
                    </div>`;
        });

        $('#drillDownRazas').html(html);
    }

    // ************* Fin Simulación de Razas *************


    // Asignar evento al botón de "Aplicar Filtros" (Aquí es donde se haría la llamada AJAX general)
    $('#applyFilters').on('click', function () {
        console.log("Aplicando filtros y recargando dashboard...");
        handleDrillUp(); // Para asegurar que volvemos al Nivel 1 de Rendimiento con los nuevos filtros
        handleIngresosDrillUp(); // Para asegurar que volvemos al Nivel 1 de Ingresos
        loadDashboardData();
    });


    // -----------------------------------------------------------------
    // 3. CARGA DE DATOS INICIAL
    // -----------------------------------------------------------------

    /**
     * Función que simula la carga inicial de todos los datos del ViewModel.
     * En el siguiente paso, esta función contendrá la llamada AJAX a la Controller.
     */
    function loadDashboardData() {
        console.log("Cargando datos del Dashboard desde el ViewModel...");

        // 1. Rendimiento de Turnos (Drill Down Nivel 1)
        renderRendimientoChart(initialRendimientoData);

        // 2. Servicios Más Solicitados
        renderServiciosChart(initialServiciosData);

        // 3. Gráfico de Especies
        renderEspeciesChart(initialEspeciesData);

        // 4. Actualizamos el KPI de Ausencia para el semáforo
        updateKpiSemaforo(initialTasaAusencia);

        // 5. Gráfico de Ingresos: ¡AHORA LLAMAMOS AL NIVEL 1 ANUAL!
        renderIngresosNivel1Chart(initialIngresosAnualesData);
    }

    // Implementación simple de la semaforización de KPI
    function updateKpiSemaforo(tasa) {
        const card = $('#kpi-ausencia-card');
        card.removeClass('indicator-rojo indicator-amarillo indicator-verde');

        // Se usa la lógica de tu semáforización: Rojo (>15%) | Amarillo (5-15%) | Verde (<5%)
        if (tasa > 0.15) {
            card.addClass('indicator-rojo');
        } else if (tasa >= 0.05) {
            card.addClass('indicator-amarillo');
        } else {
            card.addClass('indicator-verde');
        }
    }

    // Iniciar la carga de datos al cargar la página
    loadDashboardData();

});