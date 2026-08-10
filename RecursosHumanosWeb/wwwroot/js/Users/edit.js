(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        // Initialize Select2 for all form-select elements
        if (typeof $ !== 'undefined' && $.fn.select2) {
            $('.form-select').select2({
                width: '100%',
                language: {
                    noResults: function () { return "No se encontraron resultados"; }
                }
            });
        }

        // Elements
        const toggleBtn = document.getElementById('togglePassword');
        const passwordInput = document.getElementById('passwordInput');
        const passwordStrength = document.getElementById('password-strength');

        // Toggle password visibility
        if (toggleBtn && passwordInput) {
            toggleBtn.addEventListener('click', function () {
                const icon = this.querySelector('i');

                if (passwordInput.type === 'password') {
                    passwordInput.type = 'text';
                    if (icon) {
                        icon.classList.remove('fa-eye');
                        icon.classList.add('fa-eye-slash');
                    }
                } else {
                    passwordInput.type = 'password';
                    if (icon) {
                        icon.classList.remove('fa-eye-slash');
                        icon.classList.add('fa-eye');
                    }
                }
            });
        }

        // Password Strength Indicator
        if (passwordInput && passwordStrength) {
            passwordInput.addEventListener('input', function () {
                const val = this.value;

                // If empty (and optional in edit), clear it
                if (!val) {
                    passwordStrength.style.width = '0%';
                    return;
                }

                // Show bar (implied visible)

                let strength = 0;
                // Length check
                if (val.length >= 6) strength += 33;
                if (val.length >= 9) strength += 33;

                // Complexity check (Number + Uppercase)
                if (/[A-Z]/.test(val) && /[0-9]/.test(val)) strength += 34;

                passwordStrength.style.width = strength + '%';

                // Colors (Strictly inline to override default classes)
                if (strength < 40) {
                    passwordStrength.style.backgroundColor = '#ef4444'; // Red
                } else if (strength < 80) {
                    passwordStrength.style.backgroundColor = '#f59e0b'; // Yellow
                } else {
                    passwordStrength.style.backgroundColor = '#10b981'; // Green
                }
            });
        }

        // Form submission loading state
        const form = document.querySelector('form');
        const submitBtn = document.querySelector('button[type="submit"]');

        if (form && submitBtn) {
            form.addEventListener('submit', function (e) {
                if (form.checkValidity()) {
                    submitBtn.disabled = true;
                    // Store original content if not already stored
                    if (!submitBtn.getAttribute('data-original-html')) {
                        submitBtn.setAttribute('data-original-html', submitBtn.innerHTML);
                    }
                    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Guardando...';

                    // Restore if it takes too long (optional safety)
                    setTimeout(() => {
                        // Check if form was actually submitted (browser usually navigates away, but if prevented or ajax...)
                        if (!form.submitted) { // 'submitted' prop is not standard, but just a safety timeout
                            submitBtn.disabled = false;
                            submitBtn.innerHTML = submitBtn.getAttribute('data-original-html');
                        }
                    }, 8000);
                }
            });
        }
    });
})();
