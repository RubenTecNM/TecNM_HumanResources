<template>
  <div class="tecnm-app" :class="{ 'is-sidebar-expanded': expanded, 'is-sidebar-open': mobileOpen }">
    <AnimatedBackground :enabled="currentTheme.animatedBg" />

    <!-- Persistent top header -->
    <header class="tecnm-topbar">
      <button class="tecnm-topbar__menu-btn" aria-label="Abrir menú" @click="mobileOpen = true">
        <i class="fa fa-bars" aria-hidden="true"></i>
      </button>
      <div class="tecnm-topbar__brand">
        <span class="tecnm-brand-title">Tecnológico Nacional de México</span>
        <span class="tecnm-brand-subtitle">Sistema de Permisos</span>
      </div>
    </header>

    <!-- Mobile overlay -->
    <div v-if="mobileOpen" class="tecnm-sidebar-overlay" @click="mobileOpen = false"></div>

    <!-- Sidebar -->
    <aside class="tecnm-sidebar" aria-label="Menú principal">
      <div class="tecnm-sidebar__brand">
        <button
          class="tecnm-sidebar__toggle"
          aria-label="Expandir menú lateral"
          :title="expanded ? 'Contraer menú lateral' : 'Expandir menú lateral'"
          @click="toggleSidebar"
        >
          <i class="fa fa-layer-group" aria-hidden="true"></i>
        </button>
        <div v-if="expanded" class="tecnm-sidebar__brand-text">
          <span class="tecnm-sidebar__brand-name">TecNM</span>
          <span class="tecnm-sidebar__brand-sub">Sistema de Permisos</span>
        </div>
      </div>

      <ul class="tecnm-sidebar__menu">
        <li class="tecnm-sidebar__label">Navegación</li>
        <li class="tecnm-sidebar__item" :class="{ active: isActive('permissions') }">
          <RouterLink to="/permissions" class="tecnm-sidebar__link">
            <i class="fa fa-file-text-o" aria-hidden="true"></i>
            <span>Permisos</span>
          </RouterLink>
        </li>

        <template v-if="authStore.isRHOrAdmin">
          <li class="tecnm-sidebar__label">Gestión</li>
          <li class="tecnm-sidebar__item" :class="{ active: isActive('cutoffs') }">
            <RouterLink to="/cutoffs" class="tecnm-sidebar__link">
              <i class="fa fa-calendar-check-o" aria-hidden="true"></i>
              <span>Cortes</span>
            </RouterLink>
          </li>
          <li class="tecnm-sidebar__item" :class="{ active: isActive('users') }">
            <RouterLink to="/users" class="tecnm-sidebar__link">
              <i class="fa fa-users" aria-hidden="true"></i>
              <span>Usuarios</span>
            </RouterLink>
          </li>
          <li class="tecnm-sidebar__item" :class="{ active: isActive('departments') }">
            <RouterLink to="/departments" class="tecnm-sidebar__link">
              <i class="fa fa-sitemap" aria-hidden="true"></i>
              <span>Departamentos</span>
            </RouterLink>
          </li>

          <li class="tecnm-sidebar__item" :class="{ active: isActive('areas') }">
            <RouterLink to="/areas" class="tecnm-sidebar__link">
              <i class="fa fa-folder" aria-hidden="true"></i>
              <span>Áreas</span>
            </RouterLink>
          </li>
        </template>

        <template v-if="authStore.isAdmin">
          <li class="tecnm-sidebar__label">Administración</li>
          <li class="tecnm-sidebar__item" :class="{ active: isActive('audit') }">
            <RouterLink to="/audit" class="tecnm-sidebar__link">
              <i class="fa fa-history" aria-hidden="true"></i>
              <span>Auditoría</span>
            </RouterLink>
          </li>
          <li class="tecnm-sidebar__item" :class="{ active: isActive('api-keys') }">
            <RouterLink to="/api-keys" class="tecnm-sidebar__link">
              <i class="fa fa-key" aria-hidden="true"></i>
              <span>API Keys</span>
            </RouterLink>
          </li>
        </template>
      </ul>

      <div class="tecnm-sidebar__footer">
        <div class="tecnm-sidebar__footer-actions">
          <button type="button" class="tecnm-sidebar__link profile-btn" @click="showProfile = true" title="Mi perfil">
            <i class="fa fa-user" aria-hidden="true"></i>
            <span v-if="expanded">{{ authStore.user?.name }}</span>
          </button>
          <ThemeSwitcher :collapsed="!expanded" />
          <button type="button" class="tecnm-logout-btn" @click="logout">
            <i class="fa fa-power-off" aria-hidden="true"></i>
            <span v-if="expanded">Salir</span>
          </button>
        </div>
      </div>

      <AppModal
        v-model:show="showProfile"
        title="Mi perfil"
        icon="fa-user"
        :show-cancel="false"
        confirm-text="Cerrar"
        @confirm="showProfile = false"
      >
        <div class="user-detail-grid" v-if="authStore.user">
          <div class="detail-row"><span class="detail-label">Nombre</span><span class="detail-value">{{ authStore.user.name }}</span></div>
          <div class="detail-row"><span class="detail-label">Correo</span><span class="detail-value">{{ authStore.user.email }}</span></div>
          <div class="detail-row"><span class="detail-label">Tipo de Usuario</span><span class="detail-value">{{ roleLabel }}</span></div>
        </div>
      </AppModal>
    </aside>

    <main class="tecnm-main">
      <div class="tecnm-container">
        <RouterView />
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore, USER_TYPE } from '@/stores/auth'
import { useTheme } from '@/composables/useTheme'
import { askConfirmation } from '@/composables/useAlerts'
import AnimatedBackground from '@/components/AnimatedBackground.vue'
import ThemeSwitcher from '@/components/ThemeSwitcher.vue'
import AppModal from '@/components/AppModal.vue'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const { currentTheme } = useTheme()

const expanded = ref(localStorage.getItem('sidebar-expanded') === 'true')
const mobileOpen = ref(false)
const showProfile = ref(false)

const ROLE_LABELS: Record<number, string> = {
  [USER_TYPE.ADMINISTRADOR]: 'Administrador',
  [USER_TYPE.AUTORIZADOR]: 'Autorizador',
  [USER_TYPE.RECURSOS_HUMANOS]: 'Recursos Humanos',
}
const roleLabel = computed(() => ROLE_LABELS[authStore.userType ?? 0] ?? 'Empleado')

function toggleSidebar() {
  expanded.value = !expanded.value
  localStorage.setItem('sidebar-expanded', String(expanded.value))
}

function isActive(...names: string[]): boolean {
  const path = route.path
  return names.some((n) => path.startsWith(`/${n}`) || path.startsWith('/' + n + '/'))
}

async function logout() {
  const ok = await askConfirmation({ title: '¿Cerrar sesión?', confirmText: 'Sí, salir' })
  if (!ok) return
  await authStore.logout()
  router.push({ name: 'login' })
}
</script>

<style scoped>
.tecnm-sidebar__item .tecnm-logout-btn {
  text-decoration: none;
  border-radius: var(--tecnm-radius-md);
  font-family: inherit;
}

.user-detail-grid {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.detail-row {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.5rem 0;
  border-bottom: 1px solid var(--tecnm-border, #e5e7eb);
}

.detail-row:last-child {
  border-bottom: none;
}

.detail-label {
  min-width: 140px;
  font-size: 0.82rem;
  font-weight: 600;
  color: var(--tecnm-text-secondary, #6b7280);
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.detail-value {
  font-size: 0.9rem;
  color: var(--tecnm-text, #111827);
}
</style>