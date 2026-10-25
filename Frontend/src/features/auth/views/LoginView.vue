<template>
  <div class="login-page" :class="{ 'login-page--gap': !currentTheme.animatedBg }">
    <AnimatedBackground v-if="currentTheme.animatedBg" :enabled="currentTheme.animatedBg" />

    <div class="tecnm-header login-header">
      <div>
        <p class="tecnm-brand-title">Tecnológico Nacional de México</p>
        <p class="tecnm-brand-subtitle">Control de Acceso — Sistema de Permisos</p>
      </div>
    </div>

    <div class="login-container">
      <section class="tecnm-card login-card">
        <div class="tecnm-card-header tecnm-card-header--premium">
          <div class="login-title">
            <i class="fa fa-shield" aria-hidden="true"></i>
            <h2>Iniciar Sesión</h2>
          </div>
        </div>

        <div class="tecnm-card-body">
          <form @submit.prevent="submit">
            <div class="tecnm-form-group">
              <label class="tecnm-label" for="email">
                <i class="fa fa-at" aria-hidden="true"></i> Correo Electrónico
              </label>
              <input
                id="email"
                v-model="form.Email"
                type="email"
                class="tecnm-form-control"
                placeholder="usuario@monclova.tecnm.mx"
                required
                autocomplete="username"
              />
            </div>

            <div class="tecnm-form-group">
              <label class="tecnm-label" for="password">
                <i class="fa fa-lock" aria-hidden="true"></i> Contraseña
              </label>
              <div class="password-wrapper">
                <input
                  id="password"
                  v-model="form.Password"
                  :type="showPassword ? 'text' : 'password'"
                  class="tecnm-form-control"
                  placeholder="••••••••"
                  required
                  autocomplete="current-password"
                />
                <button type="button" class="password-toggle" :aria-label="showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'" @click="showPassword = !showPassword">
                  <i :class="showPassword ? 'fa fa-eye-slash' : 'fa fa-eye'" aria-hidden="true"></i>
                </button>
              </div>
            </div>

            <label class="remember-row">
              <input v-model="form.RememberMe" type="checkbox" />
              Recordarme
            </label>

            <div v-if="errorMessage" class="tecnm-alert tecnm-alert-danger tecnm-mb-2">
              <i class="fa fa-times-circle" aria-hidden="true"></i>
              <span>{{ errorMessage }}</span>
            </div>

            <button type="submit" class="tecnm-btn tecnm-btn-primary login-btn" :disabled="authStore.loading">
              <i class="fa fa-sign-in" aria-hidden="true"></i>
              {{ authStore.loading ? 'Verificando...' : 'Acceder al Sistema' }}
            </button>

            <div class="text-center tecnm-mt-2">
              <RouterLink to="/forgot-password" class="forgot-link">¿Olvidaste tu contraseña?</RouterLink>
            </div>
          </form>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from "@/features/auth/stores/auth"'
import { useTheme } from '@/shared/composables/useTheme'
import { showToast } from '@/shared/composables/useAlerts'
import AnimatedBackground from '@/shared/components/AnimatedBackground.vue'

const authStore = useAuthStore()
const route = useRoute()
const router = useRouter()
const { currentTheme } = useTheme()

const form = reactive({ Email: '', Password: '', RememberMe: true })
const showPassword = ref(false)
const errorMessage = ref('')

async function submit() {
  errorMessage.value = ''
  const result = await authStore.login({ ...form, ReturnUrl: (route.query.redirect as string) || '' })
  if (result.success) {
    showToast('Sesión iniciada correctamente.', 'success')
    router.push((route.query.redirect as string) || '/permissions')
  } else {
    errorMessage.value = result.message || 'No se pudo iniciar sesión.'
  }
}
</script>

<style scoped>
.login-page {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: var(--tecnm-bg-main);
}

.login-page--gap {
  background:
    radial-gradient(ellipse at 15% 20%, rgba(27, 57, 106, 0.1), transparent 55%),
    radial-gradient(ellipse at 85% 85%, rgba(197, 160, 89, 0.16), transparent 50%),
    var(--tecnm-bg-main);
}

.login-container {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem 1rem;
}

.login-card {
  width: 100%;
  max-width: 460px;
}

.login-title {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.login-title i {
  font-size: 1.5rem;
  color: var(--tecnm-gold-accent);
}

.password-wrapper {
  position: relative;
}

.password-wrapper input {
  padding-right: 3rem;
}

.password-toggle {
  position: absolute;
  right: 0.75rem;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: var(--tecnm-text-secondary);
  cursor: pointer;
}

.remember-row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 1rem;
  font-size: 0.9rem;
  color: var(--tecnm-text-primary);
  cursor: pointer;
}

.login-btn {
  width: 100%;
  height: 3rem;
  font-size: 1rem;
}

.forgot-link {
  color: var(--tecnm-accent-text);
  font-weight: 600;
  text-decoration: none;
  font-size: 0.9rem;
}

.forgot-link:hover {
  text-decoration: underline;
}

.text-center {
  text-align: center;
}
</style>