// site.js

// ----------------------------------------------------------------------
// 1. FUNCIONES AUXILIARES PARA MANEJO DE ALERTA Y VALIDACIÓN
// ----------------------------------------------------------------------

/**
 * Limpia los mensajes de error de validación previos en el formulario.
 */
function clearValidationErrors() {
    // Elimina todos los mensajes de error dinámicos
    $(".validation-error-message").remove();
    // Elimina la clase de error de Bootstrap de todos los controles
    $(".form-control").removeClass("is-invalid");
    $(".form-select").removeClass("is-invalid");
    $("[data-valmsg-for]").empty(); // Limpia mensajes de error del helper MVC
}

/**
 * Combina fecha (YYYY-MM-DD) y hora (HH:mm) en una cadena LOCAL sin zona horaria,
 * de modo que ASP.NET la interprete como hora local (Unspecified) en el servidor.
 * @param {string|null} dateOnly - Valor del input date ("YYYY-MM-DD").
 * @param {string|null} timeOnly - Valor del input time ("HH:mm") o null.
 * @param {boolean} [endOfDay=false] - Si true y no hay hora, usa 23:59:59.
 * @returns {string|null} Cadena local "YYYY-MM-DDTHH:mm:ss" o null.
 */
function combineDateTimeLocal(dateOnly, timeOnly, endOfDay = false) {
    if (!dateOnly) return null;
    let time = timeOnly && timeOnly.trim() ? timeOnly.trim() : (endOfDay ? '23:59:59' : '00:00:00');
    if (time.split(':').length === 2) time += ':00';
    return `${dateOnly}T${time}`;
}

// Helper global para mostrar toasts rápidos usando SweetAlert2.
// Uso: showToast('Mensaje', 'success'|'info'|'warning'|'error')
function showToast(message, type = 'info') {
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    });
    Toast.fire({ icon: type, title: message });
}

// Helper global para alternar visibilidad de contraseña (FR4).
// Uso: <button type="button" data-toggle-password><i class="fas fa-eye"></i></button>
// junto a un input[type="password"] dentro del mismo .input-wrapper.
function togglePassword(btn) {
    const wrapper = btn.closest('.input-wrapper');
    const input = wrapper ? wrapper.querySelector('input[type="password"], input[type="text"]') : null;
    if (!input) return;

    const isPassword = input.type === 'password';
    input.type = isPassword ? 'text' : 'password';
    const icon = btn.querySelector('i');
    if (icon) {
        icon.classList.toggle('fa-eye');
        icon.classList.toggle('fa-eye-slash');
    }
    btn.setAttribute('aria-label', isPassword ? 'Ocultar contraseña' : 'Mostrar contraseña');
}

/**
 * Procesa la respuesta del servidor (AlertResponseDTO) y maneja las alertas y la validación.
 * @param {object} response - El objeto de respuesta JSON del servidor.
 * @returns {Promise<any>} Una promesa que resuelve después de mostrar la alerta.
 */
function handleAlertResponse(response) {
    if (!response || typeof response !== 'object') {
        Swal.fire({
            title: 'Error Desconocido',
            text: 'El servidor devolvió un formato de respuesta inesperado.',
            icon: 'error'
        });
        return Promise.resolve();
    }

    clearValidationErrors(); // Siempre limpiar antes de procesar

    const isSuccess = (response.success === true) || (response.Success === true);
    const alertIcon = response.icon || response.Icon || (isSuccess ? 'success' : 'error');
    const alertTitle = response.title || response.Title || (isSuccess ? '¡Éxito!' : 'Error');
    const alertText = response.message || response.Message || (isSuccess ? 'Operación completada exitosamente.' : 'Ha ocurrido un error.');
    const redirectUrl = response.redirectUrl || response.RedirectUrl;
    const errors = response.errors || response.Errors;

    // ----------------------------------------------------------------------
    // MANEJO DE ERRORES DE VALIDACIÓN ESPECÍFICOS (La nueva propiedad)
    // ----------------------------------------------------------------------
    if (!isSuccess && errors && Object.keys(errors).length > 0) {
        // La validación del modelo falló. Inyectamos los errores en el formulario.
        for (const fieldName in errors) {
            if (errors.hasOwnProperty(fieldName)) {
                const errorMessages = errors[fieldName];

                // Buscar por atributo name (más robusto para diferentes tipos de input)
                const $input = $(`[name="${fieldName}"]`);

                if ($input.length) {
                    $input.addClass("is-invalid");

                    // Buscar el span de validación de MVC si existe (data-valmsg-for)
                    const $valMsg = $(`[data-valmsg-for="${fieldName}"]`);

                    if ($valMsg.length) {
                        // Usar el contenedor MVC si está presente
                        $valMsg.addClass("text-danger validation-error-message");
                        $valMsg.html(errorMessages.join('<br>'));
                    } else {
                        // Si no hay span MVC, insertamos un div invalid-feedback estándar de Bootstrap
                        const $feedback = $(`<div class="invalid-feedback validation-error-message">${errorMessages.join('<br>')}</div>`);
                        // Manejo especial para inputs que requieren la clase en el contenedor (como radios/checkboxes)
                        if ($input.is(':checkbox') || $input.is(':radio')) {
                            // Buscar el contenedor padre o agruparlo
                            $input.closest('.form-check').append($feedback);
                        } else {
                            $feedback.insertAfter($input);
                        }
                    }
                }
            }
        }

        // Si hay errores de campo, mostramos la alerta general, pero solo como un aviso.
        return Swal.fire({
            title: alertTitle,
            text: alertText,
            icon: alertIcon
        });
    }

    // ----------------------------------------------------------------------
    // MANEJO DE ALERTA GENERAL Y REDIRECCIÓN (para éxito o error general)
    // ----------------------------------------------------------------------
    return Swal.fire({
        title: alertTitle,
        text: alertText,
        icon: alertIcon,
        confirmButtonText: 'Aceptar'
    }).then(() => {
        // REDIRECCIÓN (solo si es exitoso o el servidor lo requiere)
        if (isSuccess && redirectUrl) {
            console.log(`Redireccionando a: ${redirectUrl}`);
            window.location.href = redirectUrl;
        } else if (isSuccess && !redirectUrl) {
            // Si fue exitoso y no hay redirección específica, recargar la página.
            // Esto es útil para acciones de "Eliminar" o "Estado".
            location.reload();
        }
    });
}


// ----------------------------------------------------------------------
// 2. FUNCIÓN PRINCIPAL PARA ACCIONES CON CONFIRMACIÓN (Usando Fetch API)
// ----------------------------------------------------------------------

/**
 * Función genérica para manejar acciones con confirmación (por ejemplo, Eliminar, Activar/Desactivar).
 * Utiliza Fetch API y se basa en el flujo showConfirmation del DTO.
 */
function handleActionWithConfirmation(url, data = {}, options = {}) {
    const token = $('input[name="__RequestVerificationToken"]').val();

    if (!token) {
        console.error('⚠️ Token antiforgery no encontrado');
        handleAlertResponse({ success: false, title: 'Error', message: 'Token de seguridad no encontrado. Recarga la página.', icon: 'error' });
        return Promise.reject('No token found');
    }

    const defaults = {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify(data)
    };

    return fetch(url, { ...defaults, ...options })
        .then(response => {
            if (!response.ok) throw new Error(`Error HTTP: ${response.status}`);
            return response.json();
        })
        .then(result => {
            // Si requiere confirmación, mostrar SweetAlert (Manejar PascalCase y camelCase)
            if (result.showConfirmation || result.ShowConfirmation) {
                // Convertir markdown simple a HTML
                const message = result.message || result.Message || '';
                const messageHtml = message.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');

                return Swal.fire({
                    title: result.title || result.Title || '¿Estás seguro?',
                    html: messageHtml,
                    icon: result.icon || result.Icon || 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#3085d6',
                    cancelButtonColor: '#d33',
                    confirmButtonText: result.confirmButtonText || result.ConfirmButtonText || 'Sí, continuar',
                    cancelButtonText: result.cancelButtonText || result.CancelButtonText || 'Cancelar'
                }).then(swalResult => {
                    if (swalResult.isConfirmed) {
                        // Reenviar con confirmación
                        data.Confirmed = true;
                        return fetch(url, {
                            ...defaults,
                            body: JSON.stringify(data)
                        })
                            .then(response => response.json())
                            .then(finalResult => {
                                // Mostrar el resultado final (éxito/error)
                                return handleAlertResponse(finalResult);
                            });
                    }
                    console.log('❌ Usuario canceló la acción');
                    return { success: false, cancelled: true };
                });
            } else {
                // Si no requiere confirmación, solo mostrar el resultado.
                return handleAlertResponse(result);
            }
        })
        .catch(error => {
            console.error('❌ Error en handleActionWithConfirmation:', error);

            let detailedMessage = 'Ocurrió un error al procesar la solicitud.';
            if (error.message && error.message.includes('HTTP:')) {
                detailedMessage = `Error de servidor: ${error.message}. Por favor, contacte a soporte técnico.`;
            } else if (error.message) {
                detailedMessage = `Error: ${error.message}`;
            }

            handleAlertResponse({
                success: false,
                title: 'Error de Comunicación',
                message: detailedMessage,
                icon: 'error'
            });
            return { success: false, error: error.message };
        });
}


// ----------------------------------------------------------------------
// 3. INICIALIZACIÓN Y EVENTOS
// ----------------------------------------------------------------------

$(document).ready(function () {
    console.log('🚀 Sistema de alertas y manejo de acciones cargado');

    // ----------------------------------------------------------------------
    // MANEJO DE ALERTAS DESDE TEMPDATA (desde el servidor en redirecciones)
    // ----------------------------------------------------------------------
    const errorAlertJson = document.querySelector('[data-error-alert]')?.getAttribute('data-error-alert');
    const successAlertJson = document.querySelector('[data-success-alert]')?.getAttribute('data-success-alert');

    if (errorAlertJson) {
        try {
            const alertData = JSON.parse(errorAlertJson);
            Swal.fire({
                icon: alertData.icon || 'error',
                title: alertData.title || 'Error',
                text: alertData.text || 'Ha ocurrido un error'
            });
        } catch (e) {
            console.error('Error parsing error alert JSON:', e);
        }
    }

    if (successAlertJson) {
        try {
            const alertData = JSON.parse(successAlertJson);
            Swal.fire({
                icon: alertData.icon || 'success',
                title: alertData.title || '¡Éxito!',
                text: alertData.text || 'Operación completada exitosamente'
            });
        } catch (e) {
            console.error('Error parsing success alert JSON:', e);
        }
    }

    // ----------------------------------------------------------------------
    // MANEJO DE ERRORES DE VALIDACIÓN (Asp.NET ModelSummary)
    // ----------------------------------------------------------------------
    function checkAndShowValidationErrors() {
        const $summary = $(".validation-summary-errors");
        if ($summary.length > 0) {
            // Extraer los textos de los errores
            let errorText = "";
            $summary.find("ul li").each(function () {
                errorText += `${$(this).text()}<br>`;
            });

            if (errorText) {
                Swal.fire({
                    icon: 'error',
                    title: 'Se encontraron errores',
                    html: `<div style="text-align: center; font-weight: 500;">${errorText}</div>`,
                    confirmButtonText: 'Entendido',
                    confirmButtonColor: '#3085d6',
                    customClass: {
                        popup: 'rounded-4 shadow-lg'
                    }
                });
            }
            // Ocultar el sumario original para evitar duplicación visual
            $summary.hide();
        }
    }

    // Ejecutar inmediatamente
    checkAndShowValidationErrors();

    // Event listener para botones con data-action
    $(document).on('click', '[data-action]', function (e) {
        e.preventDefault();
        console.log('🖱️ Clic detectado en botón con data-action');

        const $btn = $(this);
        const action = $btn.data('action');
        const controller = $btn.data('controller') || 'Permission';
        const id = $btn.data('id');

        // Soporte para URL dinámica (Url.Action) o construcción manual (compatibilidad)
        const url = $btn.data('url') || `/${controller}/${action}`;
        const data = { id: id };

        console.log(`📡 Ejecutando acción: ${url}`, data);

        // Deshabilitar botón durante la petición
        $btn.prop('disabled', true);
        $btn.css('opacity', '0.5');

        handleActionWithConfirmation(url, data)
            .finally(() => {
                // Habilitar botón al finalizar (ya sea éxito, error o cancelación)
                $btn.prop('disabled', false);
                $btn.css('opacity', '1');
            });
    });

    // Toggle de contraseña (FR4): auto-enlaza botones [data-toggle-password]
    $(document).on('click', '[data-toggle-password]', function (e) {
        togglePassword(this);
    });

    // Add AJAX form handler: intercept forms with class 'ajax-form' and submit as form data
    (function () {
        document.addEventListener('submit', function (e) {
            var form = e.target;
            if (!form.classList || !form.classList.contains('ajax-form')) return;
            e.preventDefault();

            // Build FormData from the form
            var formData = new FormData(form);

            // Read antiforgery token if present
            var token = form.querySelector('input[name="__RequestVerificationToken"]')?.value || '';

            // Prepare headers; do not set Content-Type so browser sets multipart/form-data boundary
            var headers = {};
            if (token) headers['RequestVerificationToken'] = token;
            headers['Accept'] = 'application/json';

            var submitBtn = form.querySelector('[type="submit"]');
            var originalBtnHtml = submitBtn ? submitBtn.innerHTML : '';
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Procesando...';
            }

            fetch(form.action, {
                method: (form.method || 'POST').toUpperCase(),
                headers: headers,
                body: formData
            })
                .then(function (response) {
                    if (!response.ok) throw new Error('Error HTTP: ' + response.status);
                    return response.json();
                })
                .then(function (json) {
                    // If server asks for confirmation, show SweetAlert and optionally resubmit
                    if (json && json.showConfirmation) {
                        return Swal.fire({
                            title: json.title || '¿Confirmar?',
                            text: json.message || '',
                            icon: json.icon || 'warning',
                            showCancelButton: true,
                            confirmButtonText: json.confirmButtonText || 'Sí, continuar',
                            cancelButtonText: json.cancelButtonText || 'Cancelar'
                        }).then(function (swalResult) {
                            if (swalResult.isConfirmed) {
                                // Mark confirmed and resend the same FormData
                                formData.set('confirmed', 'true');
                                return fetch(form.action, {
                                    method: (form.method || 'POST').toUpperCase(),
                                    headers: headers,
                                    body: formData
                                })
                                    .then(function (response) {
                                        if (!response.ok) throw new Error('Error HTTP: ' + response.status);
                                        return response.json();
                                    })
                                    .then(function (finalJson) {
                                        return handleAlertResponse(finalJson);
                                    });
                            }
                            // If cancelled, show an info toast and keep the form intact for editing
                            showToast('Acción cancelada', 'info');
                            if (submitBtn) {
                                submitBtn.disabled = false;
                                submitBtn.innerHTML = originalBtnHtml;
                            }
                            return Promise.resolve();
                        });
                    }

                    // Delegate to central handler for normal responses
                    return handleAlertResponse(json);
                })
                .catch(function (err) {
                    console.error('Error submitting ajax form:', err);
                    handleAlertResponse({ success: false, title: 'Error', message: err.message || 'Error en la petición' });
                    if (submitBtn) {
                        submitBtn.disabled = false;
                        submitBtn.innerHTML = originalBtnHtml;
                    }
                });
        });
    })();
});

// ----------------------------------------------------------------------
// 7. HEADER ORNAMENT INJECTION (PREMIUM V2)
// ----------------------------------------------------------------------
document.addEventListener('DOMContentLoaded', function () {
    const headers = document.querySelectorAll('.page-header-v2');
    headers.forEach(header => {
        if (!header.querySelector('.header-ornament-2')) {
            const ornament = document.createElement('div');
            ornament.className = 'header-ornament-2';
            header.prepend(ornament);
        }
    });
});


// ----------------------------------------------------------------------
// 4. HEADER DINÁMICO Y MENÚ (DETECCIÓN DE OVERFLOW Y TRIGGERS)
// ----------------------------------------------------------------------
document.addEventListener('DOMContentLoaded', function () {
    const navbarCollapse = document.getElementById('navbarNav');
    const brandTrigger = document.getElementById('brandMenuTrigger');
    const mainNavLinks = document.getElementById('mainNavLinks');
    const userMenu = document.querySelector('.user-menu-container');
    const header = document.querySelector('header');

    // 1. Trigger de la Marca
    if (brandTrigger) {
        brandTrigger.addEventListener('click', function (e) {
            e.preventDefault();
            if (navbarCollapse) {
                const bsCollapse = bootstrap.Collapse.getOrCreateInstance(navbarCollapse);
                bsCollapse.toggle();
            }
        });
    }

    // 2. Detección de Desbordamiento (Overflow)
    function checkHeaderOverflow() {
        if (!mainNavLinks || !header || !userMenu || !brandTrigger) return;

        // Reset temporal para medir
        header.classList.remove('force-drawer-mode');

        const container = header.querySelector('.container-fluid');
        if (!container) return;

        const containerWidth = container.offsetWidth;
        const brandWidth = brandTrigger.offsetWidth;
        // Obtenemos el contenedor de la derecha que agrupa menú de usuario y hamburguesa
        const rightContainer = userMenu.closest('.d-flex.align-items-center.ms-auto');
        const userMenuWidth = rightContainer ? rightContainer.offsetWidth : userMenu.offsetWidth;

        // Medir el ancho real de los links
        const navLinksWidth = mainNavLinks.scrollWidth;

        // Espacio disponible entre logo y menú de usuario
        const availableSpace = containerWidth - brandWidth - userMenuWidth - 60; // 60px de buffer

        if (navLinksWidth > availableSpace || window.innerWidth < 992) {
            header.classList.add('force-drawer-mode');
        } else {
            header.classList.remove('force-drawer-mode');
        }
    }

    // Ejecutar al cargar y al redimensionar
    checkHeaderOverflow();
    window.addEventListener('resize', checkHeaderOverflow);

    // 3. Manejo de cierre y UX del Drawer
    if (navbarCollapse) {
        // Cerrar menú al hacer clic en el overlay (pseudo-elemento)
        navbarCollapse.addEventListener('click', function (e) {
            if (e.target === this && this.classList.contains('show')) {
                const bsCollapse = bootstrap.Collapse.getInstance(navbarCollapse);
                if (bsCollapse) bsCollapse.hide();
            }
        });

        // Cerrar menú al hacer clic en cualquier enlace
        const navLinks = navbarCollapse.querySelectorAll('.nav-link-premium');
        navLinks.forEach(link => {
            link.addEventListener('click', function () {
                if (navbarCollapse.classList.contains('show')) {
                    const bsCollapse = bootstrap.Collapse.getInstance(navbarCollapse);
                    if (bsCollapse) bsCollapse.hide();
                }
            });
        });
    }
});

// ----------------------------------------------------------------------
// 5. FUNCIONALIDAD UI GLOBAL (Mobile Cards, etc.)
// ----------------------------------------------------------------------


// ----------------------------------------------------------------------
// 6. DATE PICKER PREMIUM (FLATPICKR)
// ----------------------------------------------------------------------
document.addEventListener('DOMContentLoaded', function () {
    // Select all date inputs that should be styled
    // We target inputs with the 'filter-input-pill' class that are dates, 
    // or any generic date input inside our premium cards    // Initialize Flatpickr for Date Inputs
    // Selector targets filters AND any element marked with .datepicker-premium
    const dateInputs = document.querySelectorAll('.filter-input-pill[type="date"], .filter-card-premium input[type="date"], .datepicker-premium, input[type="date"]');
    const timeInputs = document.querySelectorAll('input[type="time"]');
    const dateTimeInputs = document.querySelectorAll('input[type="datetime-local"]');

    if (typeof flatpickr !== 'undefined') {
        const commonOptions = {
            locale: "es",
            altInput: true,
            allowInput: true,
            theme: "light",
            onReady: function (selectedDates, dateStr, instance) {
                // Ensure placeholder is preserved in altInput
                // Try multiple sources for the placeholder
                let placeholder = instance.element.getAttribute('placeholder');

                // If not found in the element, determine based on type
                if (!placeholder || placeholder === "") {
                    if (instance.config.noCalendar) {
                        placeholder = "HH:MM AM/PM";
                    } else if (instance.config.enableTime) {
                        placeholder = "DD/MM/YYYY HH:MM AM/PM";
                    } else {
                        placeholder = "DD/MM/YYYY";
                    }
                }

                // Apply to both the original element (for reference) and the altInput (visible)
                instance.element.setAttribute('placeholder', placeholder);
                if (instance.altInput) {
                    instance.altInput.setAttribute('placeholder', placeholder);
                }

                // Create Footer Container
                const footer = document.createElement("div");
                footer.className = "flatpickr-footer-custom";

                // Button: Clear
                const clearBtn = document.createElement("button");
                clearBtn.type = "button";
                clearBtn.className = "btn-flatpickr-secondary";
                clearBtn.innerText = "Limpiar";
                clearBtn.onclick = function () {
                    instance.clear();
                    instance.close();
                };

                // Button: Today
                const todayBtn = document.createElement("button");
                todayBtn.type = "button";
                todayBtn.className = "btn-flatpickr-primary";
                todayBtn.innerText = "Hoy";
                todayBtn.onclick = function () {
                    instance.setDate(new Date());
                    instance.close();
                };

                footer.appendChild(clearBtn);
                footer.appendChild(todayBtn);
                instance.calendarContainer.appendChild(footer);
            }
        };

        // Date Picker
        if (dateInputs.length > 0) {
            flatpickr(dateInputs, {
                ...commonOptions,
                altFormat: "d/m/Y",
                dateFormat: "Y-m-d",
                disableMobile: "true", // Force custom picker
                onReady: function (selectedDates, dateStr, instance) {
                    commonOptions.onReady(selectedDates, dateStr, instance);
                    if (typeof Cleave !== 'undefined') {
                        new Cleave(instance.altInput, {
                            date: true,
                            datePattern: ['d', 'm', 'Y']
                        });
                    }
                }
            });
        }

        // Time Picker
        if (timeInputs.length > 0) {
            flatpickr(timeInputs, {
                ...commonOptions,
                enableTime: true,
                noCalendar: true,
                altFormat: "h:i K",
                dateFormat: "H:i",
                disableMobile: "true", // Force custom picker
            });
        }

        // DateTime Picker
        if (dateTimeInputs.length > 0) {
            flatpickr(dateTimeInputs, {
                ...commonOptions,
                enableTime: true,
                altFormat: "d/m/Y h:i K",
                dateFormat: "Y-m-dTH:i",
                disableMobile: "true", // Force custom picker
            });
        }
    }
});