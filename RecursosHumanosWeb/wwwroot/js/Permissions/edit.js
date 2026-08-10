// edit.js - Script para el formulario de Editar Permiso

(function () {
    'use strict';

    console.log('🚀 Edit Permiso script loaded');

    // --- ELEMENTOS DEL DOM ---
    const form = document.getElementById('editPermissionForm');
    if (!form) {
        console.error('❌ Formulario no encontrado');
        return;
    }

    // Get initial tipo from data attribute (pre-filled value)
    let currentPermissionType = parseInt(form.dataset.permissionType);
    console.log('📋 Tipo de Permiso Inicial:', currentPermissionType);

    const elements = {
        form: form,
        permissionTypeRadios: document.querySelectorAll('input[name="PermissionTypeId"]'),
        date1: document.getElementById('StartDateOnly'),
        time1: document.getElementById('StartTime'),
        date2: document.getElementById('EndDateOnly'),
        time2: document.getElementById('EndTime'),
        date1Hidden: document.querySelector('input[name="StartDate"]'),
        date2Hidden: document.querySelector('input[name="EndDate"]'),
        reason: document.getElementById('Reason'),
        charCount: document.getElementById('char-count'),
        submitBtn: document.getElementById('submitBtn'),
        // File upload elements
        evidenceFile: document.getElementById('evidenceFile'),
        fileUploadArea: document.getElementById('fileUploadArea'),
        fileUploadContent: document.getElementById('fileUploadContent'),
        filePreview: document.getElementById('filePreview'),
        fileName: document.getElementById('fileName'),
        removeFile: document.getElementById('removeFile'),
        evidenceHidden: document.querySelector('input[name="Evidence"]')
    };

    // --- CONFIGURACIÓN DE CAMPOS SEGÚN TIPO DE PERMISO ---
    function configureFieldsByType(permissionType) {
        const config = {
            1: { // Retardo
                date1Label: 'Fecha del retardo',
                date2Label: 'Fecha de fin',
                time1Visible: true,
                time2Visible: false,
                date2Required: false
            },
            2: { // Turno por turno
                date1Label: 'Fecha de inicio',
                date2Label: 'Fecha de fin',
                time1Visible: false,
                time2Visible: false,
                date2Required: true
            },
            3: { // Falta
                date1Label: 'Fecha de inicio',
                date2Label: 'Fecha de fin',
                time1Visible: false,
                time2Visible: false,
                date2Required: true
            },
            4: { // Cambio de horario
                date1Label: 'Fecha de inicio',
                date2Label: 'Fecha de fin',
                time1Visible: true,
                time2Visible: true,
                date2Required: true
            }
        };

        const currentConfig = config[permissionType];
        if (!currentConfig) return;

        // Actualizar labels
        document.getElementById('date1-label').textContent = currentConfig.date1Label;
        document.getElementById('date2-label').textContent = currentConfig.date2Label;

        // Mostrar/ocultar campos de hora
        const time1Group = document.getElementById('time1-group');
        const time2Group = document.getElementById('time2-group');

        time1Group.style.display = currentConfig.time1Visible ? 'block' : 'none';
        time2Group.style.display = currentConfig.time2Visible ? 'block' : 'none';

        // Limpiar valores de horas si se ocultan
        if (!currentConfig.time1Visible && elements.time1) {
            elements.time1.value = '';
        }
        if (!currentConfig.time2Visible && elements.time2) {
            elements.time2.value = '';
        }

        // Configurar required en Fecha2
        const date2Group = document.getElementById('date2-group');
        date2Group.style.display = (permissionType === 1) ? 'none' : 'block'; // Ocultar Fecha2 solo para Retardo (ID 1)

        if (elements.date2) {
            elements.date2.required = currentConfig.date2Required;
            if (!currentConfig.date2Required) {
                elements.date2.value = '';
            }
        }

        console.log(`✅ Campos configurados para tipo ${permissionType}`);
    }

    // --- MANEJAR CAMBIO DE TIPO DE PERMISO ---
    function handlePermissionTypeChange(e) {
        const newType = parseInt(e.target.value);
        console.log(`🔄 Tipo de permiso cambiado: ${currentPermissionType} → ${newType}`);

        currentPermissionType = newType;
        form.dataset.permissionType = newType;

        // Reconfigurar campos
        configureFieldsByType(newType);

        // Agregar clase activa a la tarjeta seleccionada
        document.querySelectorAll('.permission-type-card').forEach(card => {
            card.classList.remove('selected', 'active');
        });
        e.target.closest('.permission-type-card').classList.add('selected', 'active');
    }

    // --- COMBINAR FECHA Y HORA EN CAMPOS HIDDEN ---
    function combineDateTime() {
        if (elements.date1.value) {
            const date1 = new Date(elements.date1.value + 'T00:00:00');
            if (elements.time1 && elements.time1.value && elements.time1.offsetParent !== null) {
                const [hours, minutes] = elements.time1.value.split(':');
                date1.setHours(parseInt(hours), parseInt(minutes), 0);
            }
            elements.date1Hidden.value = date1.toISOString();
        }

        if (currentPermissionType !== 1 && elements.date2 && elements.date2.value) { // Cambiado 3 por 1 (Retardo)
            const date2 = new Date(elements.date2.value + 'T00:00:00');
            if (elements.time2 && elements.time2.value && elements.time2.offsetParent !== null) {
                const [hours, minutes] = elements.time2.value.split(':');
                date2.setHours(parseInt(hours), parseInt(minutes), 0);
            } else {
                date2.setHours(23, 59, 59);
            }
            elements.date2Hidden.value = date2.toISOString();
        }
    }

    // --- CONTADOR DE CARACTERES ---
    function updateCharCount() {
        const count = elements.reason.value.length;
        elements.charCount.textContent = count;

        if (count > 255) {
            elements.charCount.style.color = '#ef4444';
        } else if (count > 200) {
            elements.charCount.style.color = '#f59e0b';
        } else {
            elements.charCount.style.color = '#6b7280';
        }
    }

    // --- MANEJO DE ARCHIVO ---
    function handleFileSelect(e) {
        const file = e.target.files[0];
        if (!file) return;

        // Validar tamaño (5MB)
        if (file.size > 5 * 1024 * 1024) {
            Swal.fire({
                icon: 'error',
                title: 'Archivo muy grande',
                text: 'El archivo no debe superar los 5MB'
            });
            e.target.value = '';
            return;
        }

        // Validar tipo
        const allowedTypes = ['application/pdf', 'image/jpeg', 'image/jpg', 'image/png'];
        if (!allowedTypes.includes(file.type)) {
            Swal.fire({
                icon: 'error',
                title: 'Tipo de archivo no válido',
                text: 'Solo se permiten archivos PDF, JPG y PNG'
            });
            e.target.value = '';
            return;
        }

        // Mostrar preview
        elements.fileName.textContent = file.name;
        elements.fileUploadContent.style.display = 'none';
        elements.filePreview.style.display = 'block';
    }

    function removeFileHandler() {
        elements.evidenceFile.value = '';
        elements.evidenceHidden.value = '';
        elements.fileName.textContent = '';
        elements.filePreview.style.display = 'none';
        elements.fileUploadContent.style.display = 'block';
    }

    // --- ENVÍO DEL FORMULARIO ---
    function handleFormSubmit(e) {
        // Combinar fechas y horas antes de validar/enviar
        combineDateTime();

        // Validar que haya un tipo seleccionado
        if (!currentPermissionType) {
            e.preventDefault();
            e.stopImmediatePropagation();
            Swal.fire({
                icon: 'warning',
                title: 'Tipo requerido',
                text: 'Debe seleccionar un tipo de permiso'
            });
            return;
        }

        // Validar campos requeridos
        if (!elements.date1.value) {
            e.preventDefault();
            e.stopImmediatePropagation();
            Swal.fire({
                icon: 'warning',
                title: 'Campo requerido',
                text: 'La fecha de inicio es obligatoria'
            });
            return;
        }

        if (currentPermissionType !== 1 && !elements.date2.value) {
            e.preventDefault();
            e.stopImmediatePropagation();
            Swal.fire({
                icon: 'warning',
                title: 'Campo requerido',
                text: 'La fecha de fin es obligatoria para este tipo de permiso'
            });
            return;
        }

        if (!elements.reason.value.trim()) {
            e.preventDefault();
            e.stopImmediatePropagation();
            Swal.fire({
                icon: 'warning',
                title: 'Campo requerido',
                text: 'El motivo es obligatorio'
            });
            return;
        }

        // Si llegó aquí, es válido. No llamamos e.preventDefault()
        // para permitir que site.js (ajax-form) maneje el envío por AJAX.
        console.log('✅ Validación exitosa, delegando envío a site.js');
    }

    // --- INICIALIZACIÓN ---
    function init() {
        console.log('🔧 Inicializando formulario de edición');

        // Configurar campos según tipo inicial
        configureFieldsByType(currentPermissionType);

        // Marcar la tarjeta activa inicial
        const radioChecked = document.querySelector(`input[name="PermissionTypeId"]:checked`);
        if (radioChecked) {
            radioChecked.closest('.permission-type-card').classList.add('selected', 'active');
        }

        // Event listeners para cambio de tipo
        elements.permissionTypeRadios.forEach(radio => {
            radio.addEventListener('change', handlePermissionTypeChange);
        });

        // Event listeners
        elements.reason.addEventListener('input', updateCharCount);
        if (elements.evidenceFile) elements.evidenceFile.addEventListener('change', handleFileSelect);
        if (elements.removeFile) elements.removeFile.addEventListener('click', removeFileHandler);
        elements.form.addEventListener('submit', handleFormSubmit);

        // Event listeners para fechas/horas
        [elements.date1, elements.time1, elements.date2, elements.time2].forEach(el => {
            if (el) {
                el.addEventListener('change', combineDateTime);
            }
        });

        // Inicializar contador de caracteres
        updateCharCount();

        // Drag & Drop para archivo
        if (elements.fileUploadArea) {
            elements.fileUploadArea.addEventListener('dragover', (e) => {
                e.preventDefault();
                elements.fileUploadArea.style.borderColor = '#3b82f6';
            });

            elements.fileUploadArea.addEventListener('dragleave', () => {
                elements.fileUploadArea.style.borderColor = '#d1d5db';
            });

            elements.fileUploadArea.addEventListener('drop', (e) => {
                e.preventDefault();
                elements.fileUploadArea.style.borderColor = '#d1d5db';
                const files = e.dataTransfer.files;
                if (files.length > 0) {
                    elements.evidenceFile.files = files;
                    handleFileSelect({ target: elements.evidenceFile });
                }
            });
        }

        console.log('✅ Formulario inicializado correctamente');
    }

    // Ejecutar cuando el DOM esté listo
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();