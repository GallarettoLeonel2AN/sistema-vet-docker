
// Logica principal del formulario de reserva

function initializeReservaTurno(horariosUrl) {
    const primeraCitaCheckbox = document.getElementById('primeraCitaCheckbox');
    const mascotaContainer = document.getElementById('mascotaContainer');
    const mascotaSelect = document.getElementById('mascotaSelect');

    // Elementos de fecha y horario
    const fechaTurnoInput = $('#fechaTurno');
    const horariosDisponiblesContainer = $('#horariosDisponiblesContainer');
    const horariosOpcionesDiv = $('#horariosOpciones');
    const horarioHiddenInput = $('#horarioHiddenInput');

    // Formulario de reserva
    const reservaForm = $('form.main-form');


    // Logica para alternar la seleccion de mascota (Primera Cita)
    function toggleMascotaSelection() {
        if (primeraCitaCheckbox && primeraCitaCheckbox.checked) {
            mascotaContainer.style.display = 'none';
            mascotaSelect.removeAttribute('required');
        } else {
            mascotaContainer.style.display = 'block';
            mascotaSelect.setAttribute('required', 'required');
        }
    }

    if (primeraCitaCheckbox) {
        primeraCitaCheckbox.addEventListener('change', toggleMascotaSelection);
        toggleMascotaSelection();
    }


    // Logica para la restriccion de fecha anteriores a la de hoy
    function getTodayDateString() {
        const today = new Date();
        const year = today.getFullYear();
        const month = String(today.getMonth() + 1).padStart(2, '0');
        const day = String(today.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }

    const minDate = getTodayDateString();
    fechaTurnoInput.attr('min', minDate);

    if (fechaTurnoInput.val() < minDate) {
        fechaTurnoInput.val(minDate);
    }


    // Logica para cargar los horarios disponibles 
    function cargarHorariosDisponibles() {
        const fechaSeleccionada = fechaTurnoInput.val();

        if (fechaSeleccionada) {

            if (fechaSeleccionada < minDate) {
                horariosOpcionesDiv.empty().html('<p class="text-danger validation-error">No se pueden buscar horarios para fechas pasadas.</p>');
                horariosDisponiblesContainer.show();
                horarioHiddenInput.val('');
                return;
            }

            horariosOpcionesDiv.empty().html('<p>Cargando horarios...</p>');
            horariosDisponiblesContainer.show();

            $.ajax({
                // Usamos la URL inyectada
                url: horariosUrl,
                type: 'GET',
                data: { fecha: fechaSeleccionada },
                success: function (horarios) {
                    horariosOpcionesDiv.empty();
                    if (horarios && horarios.length > 0) {
                        horarios.forEach(horario => {
                            const btn = `<button type="button" class="horario-btn" data-horario="${horario}">${horario}</button>`;
                            horariosOpcionesDiv.append(btn);
                        });
                    } else {
                        horariosOpcionesDiv.html('<p class="text-danger validation-error">No hay horarios disponibles para la fecha seleccionada. La veterinaria no trabaja este dia</p>');
                    }
                },
                error: function () {
                    horariosOpcionesDiv.html('<p class="text-danger validation-error">Ocurrió un error al cargar los horarios. Por favor, intente de nuevo.</p>');
                }
            });
        } else {
            horariosDisponiblesContainer.hide();
            horarioHiddenInput.val('');
        }
    }

    // Logica para seleccionar un boton de horario
    horariosOpcionesDiv.on('click', '.horario-btn', function () {
        horariosOpcionesDiv.find('.horario-btn').removeClass('selected');
        $(this).addClass('selected');
        horarioHiddenInput.val($(this).data('horario'));
    });

    // Escucha los cambios en el input de fecha
    fechaTurnoInput.on('change', cargarHorariosDisponibles);

    // Llama a la funcion al cargar la pagina para inicializar
    cargarHorariosDisponibles();

    // Logica para el envio del formulario con Ajax
    reservaForm.on('submit', function (e) {
        e.preventDefault();

        if (fechaTurnoInput.val() < minDate) {
            toastr.error("La fecha seleccionada no es válida. Debe ser hoy o una fecha futura.");
            return;
        }

        const formData = new FormData(this);
        const submitButton = $(this).find('button[type="submit"]');

        submitButton.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Reservando...');

        $.ajax({
            url: this.action,
            type: this.method,
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    toastr.success("¡Turno reservado con éxito!");

                    setTimeout(() => window.location.href = response.redirectUrl, 2000);

                    cargarHorariosDisponibles();

                    reservaForm[0].reset();
                    horariosOpcionesDiv.empty();
                    horariosDisponiblesContainer.hide();
                } else {
                    toastr.error(response.message || "Ocurrió un error al reservar el turno.");
                }
            },
            error: function (xhr, status, error) {
                toastr.error("Ocurrió un error al conectar con el servidor. Por favor, intente de nuevo.");
            },
            complete: function () {
                submitButton.prop('disabled', false).html('<i class="fa-solid fa-calendar-plus"></i> Confirmar Turno');
            }
        });
    });
}

// Inicializa las funciones específicas de la reserva al cargar el DOM
document.addEventListener('DOMContentLoaded', function () {
    // Obtener la URL de la API del atributo de datos
    const formElement = document.querySelector('form.main-form');
    if (formElement) {
        const horariosUrl = formElement.getAttribute('data-horarios-url');
        if (horariosUrl) {
            initializeReservaTurno(horariosUrl);
        }
    }
});