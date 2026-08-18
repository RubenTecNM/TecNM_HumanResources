<template>
  <div class="auth-container">
    <div class="auth-card">
      <div class="auth-header">
        <h1 class="auth-title">Recuperación de Contraseña</h1>
        <p class="auth-subtitle">Ingresa tu correo institucional para recibir un enlace de restablecimiento.</p>
      </div>

      <form @submit.prevent="handleSubmit" class="auth-form">
        <div class="form-group">
          <label for="email" class="form-label">Correo Electrónico Institucional</label>
          <div class="input-with-icon">
            <i class="fas fa-envelope input-icon"></i>
            <input
              id="email"
              v-model="email"
              type="email"
              class="form-control"
              placeholder="ejemplo@monclova.tecnm.mx"
              required
              :disabled="loading || submitted"
            />
          </div>
        </div>

        <div v-if="errorMessage" class="alert alert-danger">
          <i class="fas fa-exclamation-circle me-2"></i> {{ errorMessage }}
        </div>

        <div v-if="successMessage" class="alert alert-success">
          <i class="fas fa-check-circle me-2"></i> {{ successMessage }}
        </div>

        <button type="submit" class="btn btn-primary w-100 mt-3" :disabled="loading || submitted">
          <i v-if="loading" class="fas fa-spinner fa-spin me-2"></i>
          <i v-else class="fas fa-paper-plane me-2"></i>
          Enviar Enlace
        </button>

        <div class="auth-footer mt-4 text-center">
          <router-link to="/login" class="back-link">
            <i class="fas fa-arrow-left me-1"></i> Volver a Iniciar Sesión
          </router-link>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const email = ref('')
const loading = ref(false)
const submitted = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

async function handleSubmit() {
  if (!email.value) return

  loading.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    const res = await fetch('/api/PasswordResetApi/request-token', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: email.value })
    })

    const data = await res.json()
    if (res.ok && data.success) {
      submitted.value = true
      successMessage.value = data.message || 'Si el correo está registrado, recibirás un enlace de restablecimiento.'
    } else {
      errorMessage.value = data.message || 'Ocurrió un error al procesar la solicitud.'
    }
  } catch (err) {
    errorMessage.value = 'No se pudo conectar con el servidor. Intenta de nuevo más tarde.'
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
  color: #6c757d;
  line-height: 1.4;
}

.form-group {
  margin-bottom: 1.25rem;
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
  margin-top: 1rem;
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
