$(document).ready(function () {
    console.log('Iniciando script de permisos...');

    // --- 1. CONFIGURACIÓN INICIAL DE ROLES Y ELEMENTOS ---
    const formElement = $('#permissionForm');
    const isRH = formElement.data('is-rh') === true || formElement.data('is-rh') === 'True';
    const isAdministrator = formElement.data('is-administrator') === true || formElement.data('is-administrator') === 'True';
    // const isEmpleado = formElement.data('is-empleado') === true || formElement.data('is-empleado') === 'True'; // No necesario para esta lógica

    console.log('Roles:', { isRH, isAdministrator });

    // Elemento select (Select2) para usuario solicitante — el select2 se inicializa en la vista Create.cshtml
    const $requestingUserId = $("#requestingUserId");

    // --- 3. CONFIGURACIÓN DE TIPOS DE PERMISO (LÓGICA EXISTENTE) ---
    const typeConfigurations = {
        3: { // Falta
            date1: { show: true, label: "Del día", required: true },
            date2: { show: true, label: "Al día", required: true },
            time1: { show: false, label: "Hora entrada", required: false },
            time2: { show: false, label: "Hora salida", required: false }
        },
        1: { // Retardo
            date1: { show: true, label: "En el día", required: true },
            date2: { show: false, label: "Al día", required: false },
            time1: { show: true, label: "Hora de llegada", required: true },
            time2: { show: false, label: "Hora salida", required: false }
        },
        4: { // Cambio de Horario
            date1: { show: true, label: "Del día", required: true },
            date2: { show: true, label: "Al día", required: true },
            time1: { show: true, label: "Hora entrada", required: true },
            time2: { show: true, label: "Hora salida", required: true }
        },
        2: { // Turno por Turno
            date1: { show: true, label: "Del día", required: true },
            date2: { show: true, label: "Por el día", required: true },
            time1: { show: true, label: "Hora entrada", required: true },
            time2: { show: true, label: "Hora salida", required: true }
        }
    };

    // Event handler para cambio de tipo de permiso
    $('input[name="PermissionTypeId"]').on('change', function () {
        const selectedType = $(this).val();
        console.log('Tipo seleccionado:', selectedType);

        // Actualizar visual de las tarjetas
        $('.permission-type-card').removeClass('selected active');
        $(this).closest('.permission-type-card').addClass('selected active');
        const config = typeConfigurations[selectedType];

        if (config) {
            // Mostrar las secciones siguientes
            $('#datetime-section').slideDown(300);
            $('#reason-section').slideDown(300);
            $('#evidence-section').slideDown(300);

            // Configurar campos según el tipo
            updateFieldVisibility('date1', config.date1);
            updateFieldVisibility('date2', config.date2);
            updateFieldVisibility('time1', config.time1);
            updateFieldVisibility('time2', config.time2);
        }

        // Actualizar estilos visuales
        $('.permission-type-card').removeClass('selected');
        $(this).closest('.permission-type-card').addClass('selected');

        validateForm();
    });

    // Función para mostrar/ocultar campos
    function updateFieldVisibility(fieldName, config) {
        const group = $(`#${fieldName}-group`);
        const label = $(`#${fieldName}-label`);
        const input = group.find('input');

        if (config && config.show) {
            group.slideDown(200);
            label.text(config.label);
            input.prop('required', config.required);

            // Mostrar/ocultar asterisco rojo
            const requiredSpan = group.find('.required');
            if (config.required) {
                requiredSpan.show();
            } else {
                requiredSpan.hide();
            }
        } else {
            group.slideUp(200);
            input.prop('required', false).val('');
        }
    }

    // --- 4. CONTADOR DE CARACTERES Y ARCHIVOS (LÓGICA EXISTENTE) ---
    const reasonTextarea = $('textarea[name="Reason"]');
    const charCount = $('#char-count');

    reasonTextarea.on('input keyup paste', function () {
        const currentLength = $(this).val().length;
        const maxLength = 255;

        charCount.text(currentLength);

        // Cambiar color según la cantidad
        const counter = $('.char-counter');
        counter.removeClass('error warning success');

        if (currentLength > maxLength) {
            counter.addClass('error');
        } else if (currentLength > maxLength * 0.9) {
            counter.addClass('warning');
        } else {
            counter.addClass('success');
        }

        validateForm();
    });

    // Manejo de archivos PDF
    $('#evidenceFile').on('change', function () {
        const file = this.files[0];
        const filePreview = $('#filePreview');
        const fileUploadContent = $('.file-upload-content');

        if (file) {
            // Validar tipo de archivo
            if (file.type !== 'application/pdf') {
                // Usar el manejador centralizado para mostrar alertas
                handleAlertResponse({ success: false, title: 'Error de Archivo', message: 'Solo se permiten archivos PDF.', icon: 'error' });
                this.value = '';
                filePreview.hide();
                fileUploadContent.show();
                return;
            }

            // Validar tamaño (5MB)
            if (file.size > 5 * 1024 * 1024) {
                handleAlertResponse({ success: false, title: 'Error de Archivo', message: 'El archivo no debe superar los 5MB.', icon: 'error' });
                this.value = '';
                filePreview.hide();
                fileUploadContent.show();
                return;
            }

            // Mostrar preview
            $('#fileName').text(file.name);
            filePreview.show();
            fileUploadContent.hide();
        } else {
            filePreview.hide();
            fileUploadContent.show();
        }
        validateForm();
    });

    // Remover archivo
    $('#removeFile').on('click', function () {
        $('#evidenceFile').val('');
        $('#filePreview').hide();
        $('.file-upload-content').show();
        validateForm();
    });

    // --- 5. VALIDACIÓN Y MANEJO DEL FORMULARIO (LÓGICA EXISTENTE) ---

    // Validación del formulario
    function validateForm() {
        let isValid = true;

        // 5.1. Verificar Usuario Solicitante (solo para RH/Admin)
        if (isRH || isAdministrator) {
            if (!$requestingUserId.val() || $requestingUserId.val().length === 0) {
                isValid = false;
            }
        }

        // 5.2. Verificar que se haya seleccionado un tipo
        const selectedType = $('input[name="PermissionTypeId"]:checked').val();
        if (!selectedType) {
            isValid = false;
        }

        // 5.3. Verificar motivo
        const reason = reasonTextarea.val() ? reasonTextarea.val().trim() : '';
        if (!reason || reason.length > 255) {
            isValid = false;
        }

        // 5.4. Verificar campos requeridos según el tipo
        const config = selectedType && typeConfigurations[selectedType] ? typeConfigurations[selectedType] : null;
        if (config) {

            // Validar fechas y horas
            if (config.date1.required && !$('input[name="StartDateOnly"]').val()) { isValid = false; }
            if (config.date2.required && !$('input[name="EndDateOnly"]').val()) { isValid = false; }
            if (config.time1.required && !$('input[name="StartTime"]').val()) { isValid = false; }
            if (config.time2.required && !$('input[name="EndTime"]').val()) { isValid = false; }

            // Validación lógica de Fechas (Fecha1 no debe ser posterior a Fecha2)
            const date1Val = $('input[name="StartDateOnly"]').val();
            const date2Val = $('input[name="EndDateOnly"]').val();
            if (config.date1.show && config.date2.show && date1Val && date2Val) {
                if (new Date(date1Val) > new Date(date2Val)) {
                    // Mostrar error visual aquí (p. ej., span de validación)
                    isValid = false;
                }
            }
        }

        // Habilitar/deshabilitar botón de envío
        $('#submitBtn').prop('disabled', !isValid);
        return isValid;
    }

    // Combinar fecha y hora en campos ocultos (para el Model Binder de ASP.NET)
    function combineDateTime() {
        const selectedType = $('input[name="PermissionTypeId"]:checked').val();
        if (!selectedType) return;

        const config = typeConfigurations[selectedType];

        // Fecha1
        if (config.date1.show) {
            const date1 = $('input[name="StartDateOnly"]').val();
            const time1 = $('input[name="StartTime"]').val() || '00:00'; // Valor por defecto si no se pide hora

            let combined1 = date1;
            // Si la hora es requerida o visible, combinamos fecha y hora
            if (config.time1.show && $('input[name="StartTime"]').val()) {
                combined1 = `${date1}T${time1}:00`;
            } else if (date1) {
                combined1 = `${date1}T00:00:00`; // Solo fecha
            }
            $('input[name="StartDate"]').val(combined1);
        } else {
            $('input[name="StartDate"]').val('');
        }

        // Fecha2
        if (config.date2.show) {
            const date2 = $('input[name="EndDateOnly"]').val();
            const time2 = $('input[name="EndTime"]').val() || '00:00';

            let combined2 = date2;
            if (config.time2.show && $('input[name="EndTime"]').val()) {
                combined2 = `${date2}T${time2}:00`;
            } else if (date2) {
                combined2 = `${date2}T23:59:59`;
            }
            $('input[name="EndDate"]').val(combined2);
        } else {
            $('input[name="EndDate"]').val('');
        }
    }

    // Event listeners para validación en tiempo real
    $('input, textarea, select').on('input change', function () {
        // Usamos un pequeño timeout para asegurar que el DOM se actualice antes de validar
        setTimeout(function () {
            combineDateTime();
            validateForm();
        }, 100);
    });

    // Submit del formulario
    $('#permissionForm').on('submit', function (e) {
        // 1. Asegurar que las fechas estén combinadas
        combineDateTime();

        // 2. Validar final
        if (!validateForm()) {
            e.preventDefault();
            e.stopImmediatePropagation();
            Swal.fire({
                icon: 'warning',
                title: 'Campos incompletos',
                text: 'Por favor complete todos los campos obligatorios y revise las fechas/motivo.'
            });
            return;
        }

        // Dejar que site.js (ajax-form) maneje el envío real
        console.log('✅ Validación exitosa, delegando envío a site.js');
    });

    // Validación inicial al cargar la página (para el modo Edit)
    validateForm();

    // Disparar el cambio inicial si hay un tipo seleccionado (para modo Edit)
    $('input[name="PermissionTypeId"]:checked').trigger('change');

    console.log('Script de permisos inicializado correctamente');
});