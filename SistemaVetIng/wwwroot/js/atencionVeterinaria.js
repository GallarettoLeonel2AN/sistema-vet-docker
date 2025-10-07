// Script para manejar la adición y eliminación dinámica de vacunas y estudios
document.addEventListener('DOMContentLoaded', function () {
    const vacunasContainer = document.getElementById('vacunas-container');
    const estudiosContainer = document.getElementById('estudios-container');
    const addVacunaBtn = document.getElementById('add-vacuna-btn');
    const addEstudioBtn = document.getElementById('add-estudio-btn');

    let vacunaIndex = 0;
    let estudioIndex = 0;

    /**
     * Función genérica para obtener el siguiente índice de un array de inputs.
     * @param {HTMLElement} container - El contenedor de los elementos (vacunas o estudios).
     * @param {string} baseName - El nombre base del array 
     * @returns {number} El siguiente índice a usar.
     */
    function getNextIndex(container) {
        const count = container.querySelectorAll('.v1-input-group select').length;
        return count;
    }

    /**
     * Agrega un nuevo campo de selección dinámico usando una plantilla.
     * @param {string} templateId - ID de la etiqueta <template> en el CSHTML.
     * @param {HTMLElement} container - Contenedor donde se insertará el nuevo campo.
     * @param {string} namePrefix - Prefijo para el atributo name del select
     */
    function addItem(templateId, container, namePrefix) {
        const template = document.getElementById(templateId);
        if (!template) {
            console.error(`Plantilla no encontrada: ${templateId}`);
            return;
        }

        const clone = template.content.cloneNode(true);
        const selectElement = clone.querySelector('select');

        // Indice basado en los elementos existentes
        const newIndex = getNextIndex(container);

        // Configuramos el nombre para que MVC lo interprete como un array
        selectElement.name = `${namePrefix}[${newIndex}]`;

        container.appendChild(clone);
    }

    // Manejador para agregar Vacunas
    if (addVacunaBtn && vacunasContainer) {
        addVacunaBtn.addEventListener('click', function () {
            addItem('vacuna-template', vacunasContainer, 'VacunasSeleccionadasIds');
        });
    }

    // Manejador para agregar Estudios Complementarios
    if (addEstudioBtn && estudiosContainer) {
        addEstudioBtn.addEventListener('click', function () {
            addItem('estudio-template', estudiosContainer, 'EstudiosSeleccionadosIds');
        });
    }

    // Manejador para eliminar elementos dinámicos
    document.addEventListener('click', function (e) {
        if (e.target.closest('.remove-item-btn')) {
            // Encuentra el contenedor principal del input group y lo elimina
            const inputGroup = e.target.closest('.v1-input-group');
            if (inputGroup) {
                const container = inputGroup.parentNode;
                inputGroup.remove();
                reindexItems(container);
            }
        }
    });

    /**
     * Reindexa los nombres de los campos después de eliminar uno, 
     * para mantener la continuidad del array.
     * @param {HTMLElement} container - Contenedor de los elementos.
     */
    function reindexItems(container) {
        const selectElements = container.querySelectorAll('.v1-input-group select');

        // Determina el prefijo a partir del primer elemento, si existe
        if (selectElements.length > 0) {
            const currentName = selectElements[0].name;
            const namePrefix = currentName.substring(0, currentName.indexOf('['));

            selectElements.forEach((select, index) => {
                select.name = `${namePrefix}[${index}]`;
            });
        }
    }
});