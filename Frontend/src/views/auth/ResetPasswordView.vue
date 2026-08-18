<template>
  <div class="auth-container">
    <div class="auth-card">
      <div class="auth-header">
        <h1 class="auth-title">Restablecer Contraseña</h1>
        <p class="auth-subtitle" v-if="userEmail">Cuenta: <strong>{{ userEmail }}</strong></p>
      </div>

      <div v-if="verifying" class="text-center py-4">
        <i class="fas fa-spinner fa-spin fa-2x text-primary"></i>
        <p class="mt-2 text-muted">Validando enlace de recuperación...</p>
      </div>

      <div v-else-if="invalidToken" class="alert alert-danger text-center">
        <i class="fas fa-times-circle fa-2x mb-2"></i>
        <p>{{ tokenError }}</p>
        <router-link to="/forgot-password" class="btn btn-outline-danger btn-sm mt-2">
          Solicitar nuevo enlace
        </router-link>
      </div>

      <form v-else @submit.prevent="handleSubmit" class="auth-form">
        <div class="form-group">
          <label for="password" class="form-label">Nueva Contraseña</label>
          <div class="input-with-icon">
            <i class="fas fa-lock input-icon"></i>
            <input
              id="password"
              v-model="newPassword"
              type="password"
              class="form-control"
              placeholder="••••••••"
              required
              :disabled="loading || success"
            />
          </div>

          <!-- Medidor de fortaleza -->
          <div v-if="newPassword" class="strength-meter mt-2">
            <div class="strength-bar-bg">
              <div
                class="strength-bar-fill"
                :style="{ width: strengthPercentage + '%', backgroundColor: strengthColor }"
              ></div>
            </div>
            <span class="strength-label" :style="{ color: strengthColor }">{{ strengthText }}</span>
          </div>
        </div>

        <div class="form-group mt-3">
          <label for="confirmPassword" class="form-label">Confirmar Contraseña</label>
          <div class="input-with-icon">
            <i class="fas fa-lock input-icon"></i>
            <input
              id="confirmPassword"
              v-model="confirmPassword"
              type="password"
              class="form-control"
              placeholder="••••••••"
              required
              :disabled="loading || success"
            />
          </div>
        </div>

        <div v-if="errorMessage" class="alert alert-danger mt-3">
          <i class="fas fa-exclamation-circle me-2"></i> {{ errorMessage }}
        </div>

        <div v-if="success" class="alert alert-success mt-3 text-center">
          <i class="fas fa-check-circle me-2"></i> ¡Contraseña actualizada exitosamente!
          <div class="mt-2">
            <router-link to="/login" class="btn btn-success btn-sm">
              Ir a Iniciar Sesión
            </router-link>
          </div>
        </div>

        <button v-if="!success" type="submit" class="btn btn-primary w-100 mt-4" :disabled="loading || !isFormValid">
          <i v-if="loading" class="fas fa-spinner fa-spin me-2"></i>
          Actualizar Contraseña
        </button>

        <div class="auth-footer mt-4 text-center">
          <router-link to="/login" class="back-link">
            <i class="fas fa-arrow-left me-1"></i> Volver al Login
          </router-link>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const selector = ref('')
const verifier = ref('')

const userEmail = ref('')
const newPassword = ref('')
const confirmPassword = ref('')

const verifying = ref(true)
const invalidToken = ref(false)
const tokenError = ref('')

const loading = ref(false)
const success = ref(false)
const errorMessage = ref('')

// Medidor de fortaleza
const strengthScore = computed(() => {
  const p = newPassword.value
  if (!p) return 0
  let s = 0
  if (p.length >= 8) s += 1
  if (/[A-Z]/.test(p)) s += 1
  if (/[0-9]/.test(p)) s += 1
  if (/[^A-Za-z0-9]/.test(p)) s += 1
  return s
})

const strengthPercentage = computed(() => (strengthScore.value / 4) * 100)
const strengthColor = computed(() => {
  switch (strengthScore.value) {
    case 1: return '#e53e3e'
    case 2: return '#dd6b20'
    case 3: return '#d69e2e'
    case 4: return '#38a169'
    default: return '#cbd5e0'
  }
})
const strengthText = computed(() => {
  switch (strengthScore.value) {
    case 1: return 'Débil'
    case 2: return 'Aceptable'
    case 3: return 'Buena'
    case 4: return 'Fuerte'
    default: return ''
  }
})

const isFormValid = computed(() => {
  return newPassword.value.length >= 8 && newPassword.value === confirmPassword.value
})

onMounted(async () => {
  selector.value = (route.query.sel as string) || ''
  verifier.value = (route.query.tok as string) || ''

  if (!selector.value || !verifier.value) {
    verifying.value = false
    invalidToken.value = true
    tokenError.value = 'El enlace de restablecimiento es incompleto o no válido.'
    return
  }

  try {
    const res = await fetch('/api/PasswordResetApi/validate-token', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ selector: selector.value, verifier: verifier.value })
    })
    const data = await res.json()

    if (res.ok && data.success) {
      userEmail.value = data.email || ''
    } else {
      invalidToken.value = true
      tokenError.value = data.message || 'El enlace de restablecimiento ha expirado o ya fue utilizado.'
    }
  } catch (e) {
    invalidToken.value = true
    tokenError.value = 'Error al conectar con el servidor para validar el enlace.'
  } finally {
    verifying.value = false
  }
})

async function handleSubmit() {
  if (!isFormValid.value) return

  if (newPassword.value !== confirmPassword.value) {
    errorMessage.value = 'Las contraseñas no coinciden.'
    return
  }

  loading.value = true
  errorMessage.value = ''

  try {
    const res = await fetch('/api/PasswordResetApi/confirm-reset', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        selector: selector.value,
        verifier: verifier.value,
        newPassword: newPassword.value
      })
    })

    const data = await res.json()
    if (res.ok && data.success) {
      success.value = true
    } else {
      errorMessage.value = data.message || 'No se pudo restablecer la contraseña.'
    }
  } catch (err) {
    errorMessage.value = 'Ocurrió un error al actualizar la contraseña.'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.auth-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #f4f6f9;
  padding: 1.5rem;
}

.auth-card {
  background: #ffffff;
  border-radius: 12px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.08);
  width: 100%;
  max-width: 440px;
  padding: 2.5rem 2rem;
}

.auth-header {
  text-align: center;
  margin-bottom: 2rem;
}

.auth-logo {
  height: 64px;
  object-fit: contain;
  margin-bottom: 1rem;
}

.auth-title {
  font-size: 1.4rem;
  font-weight: 700;
  color: #1b396a;
  margin-bottom: 0.5rem;
}

.auth-subtitle {
  font-size: 0.875rem;
  color: #4a5568;
}

.form-group {
  margin-bottom: 1rem;
}

.form-label {
  font-weight: 600;
  font-size: 0.875rem;
  color: #333;
  margin-bottom: 0.5rem;
  display: block;
}

.input-with-icon {
  position: relative;
}

.input-icon {
  position: absolute;
  left: 1rem;
  top: 50%;
  transform: translateY(-50%);
  color: #a0aec0;
}

.form-control {
  width: 100%;
  padding: 0.75rem 1rem 0.75rem 2.5rem;
  border: 1px solid #cbd5e0;
  border-radius: 8px;
  font-size: 0.95rem;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.form-control:focus {
  border-color: #1b396a;
  box-shadow: 0 0 0 3px rgba(27, 57, 106, 0.15);
  outline: none;
}

.strength-meter {
  display: flex;
  align-items: center;
  gap: 10px;
}

.strength-bar-bg {
  flex-grow: 1;
  height: 6px;
  background-color: #edf2f7;
  border-radius: 3px;
  overflow: hidden;
}

.strength-bar-fill {
  height: 100%;
  transition: width 0.3s ease, background-color 0.3s ease;
}

.strength-label {
  font-size: 0.75rem;
  font-weight: 700;
  min-width: 60px;
  text-align: right;
}

.btn-primary {
  background-color: #1b396a;
  border-color: #1b396a;
  color: #ffffff;
  padding: 0.75rem 1.5rem;
  font-weight: 600;
  border-radius: 8px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.btn-primary:hover:not(:disabled) {
  background-color: #142a4f;
}

.btn-primary:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.back-link {
  color: #1b396a;
  text-decoration: none;
  font-size: 0.875rem;
  font-weight: 600;
}

.back-link:hover {
  text-decoration: underline;
}

.alert {
  padding: 0.75rem 1rem;
  border-radius: 8px;
  font-size: 0.875rem;
}

.alert-danger {
  background-color: #fff5f5;
  color: #c53030;
  border: 1px solid #feb2b2;
}

.alert-success {
  background-color: #f0fff4;
  color: #276749;
  border: 1px solid #9ae6b4;
}
</style>
