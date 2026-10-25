<template>
  <div class="tecnm-app" :class="{ 'is-sidebar-expanded': expanded, 'is-sidebar-open': mobileOpen }">
    <AnimatedBackground :enabled="currentTheme.animatedBg" />

    <!-- Persistent Topbar Header -->
    <header class="tecnm-topbar">
      <button class="tecnm-topbar__menu-btn" aria-label="Abrir menú" @click="mobileOpen = true">
        <i class="fa fa-bars" aria-hidden="true"></i>
      </button>
      <button
        class="tecnm-sidebar__toggle"
        aria-label="Expandir o contraer menú lateral"
        :title="expanded ? 'Contraer menú lateral' : 'Expandir menú lateral'"
        @click="toggleSidebar"
      >
        <i class="fa fa-layer-group" aria-hidden="true"></i>
      </button>
      <div class="tecnm-topbar__brand">
        <b class="tecnm-brand-title">TECNOLÓGICO NACIONAL DE MÉXICO</b>
        <span class="tecnm-brand-subtitle">Sistema de Permisos</span>
      </div>
    </header>

    <!-- Mobile overlay -->
    <div v-if="mobileOpen" class="tecnm-sidebar-overlay" @click="mobileOpen = false"></div>

    <!-- Sidebar -->
    <aside class="tecnm-sidebar" aria-label="Menú principal">
      <!-- Live Search in sidebar (shown when expanded) -->
      <div v-if="expanded" class="sidebar-search">
        <svg viewBox="0 0 24 24" fill="none" stroke-width="2" stroke-linecap="round" aria-hidden="true">
          <circle cx="11" cy="11" r="7" />
          <path d="m20 20-3.5-3.5" />
        </svg>
        <input
          ref="searchInput"
          v-model="searchQuery"
          type="search"
          placeholder="Buscar en el menú…"
          autocomplete="off"
          aria-label="Buscar en el menú"
          @keydown="handleSearchKeydown"
        />
        <kbd>/</kbd>
      </div>

      <!-- Nav with sections -->
      <nav ref="navEl" class="sidebar-nav" @keydown="handleNavKeydown">
        <!-- Section: NAVEGACIÓN -->
        <div v-if="hasVisibleItems('navegacion')" class="sidebar-sec">
          <h6 v-if="expanded">NAVEGACIÓN</h6>
          <RouterLink
            v-for="item in sectionItems('navegacion')"
            :key="item.id"
            :to="item.to"
            class="sidebar-item"
            :class="{ active: isActive(item.match), hide: !itemMatches(item) }"
            :title="!expanded ? item.label : undefined"
            @click="mobileOpen = false"
          >
            <component :is="item.icon" />
            <span v-if="expanded" v-html="highlight(item.label, searchQuery)"></span>
            <em v-if="expanded && item.badge" class="sidebar-badge">{{ item.badge }}</em>
          </RouterLink>
        </div>

        <!-- Section: GESTIÓN -->
        <div v-if="authStore.isRHOrAdmin && hasVisibleItems('gestion')" class="sidebar-sec">
          <h6 v-if="expanded">GESTIÓN</h6>
          <RouterLink
            v-for="item in sectionItems('gestion')"
            :key="item.id"
            :to="item.to"
            class="sidebar-item"
            :class="{ active: isActive(item.match), hide: !itemMatches(item) }"
            :title="!expanded ? item.label : undefined"
            @click="mobileOpen = false"
          >
            <component :is="item.icon" />
            <span v-if="expanded" v-html="highlight(item.label, searchQuery)"></span>
            <em v-if="expanded && item.badge" class="sidebar-badge">{{ item.badge }}</em>
          </RouterLink>
        </div>

        <!-- Section: ADMINISTRACIÓN -->
        <div v-if="authStore.isAdmin && hasVisibleItems('administracion')" class="sidebar-sec">
          <h6 v-if="expanded">ADMINISTRACIÓN</h6>
          <RouterLink
            v-for="item in sectionItems('administracion')"
            :key="item.id"
            :to="item.to"
            class="sidebar-item"
            :class="{ active: isActive(item.match), hide: !itemMatches(item) }"
            :title="!expanded ? item.label : undefined"
            @click="mobileOpen = false"
          >
            <component :is="item.icon" />
            <span v-if="expanded" v-html="highlight(item.label, searchQuery)"></span>
            <em v-if="expanded && item.badge" class="sidebar-badge">{{ item.badge }}</em>
          </RouterLink>
        </div>

        <!-- Empty search results state -->
        <div v-if="expanded && totalVisibleItems === 0" class="sidebar-empty">
          Sin resultados para “<b>{{ searchQuery }}</b>”
        </div>
      </nav>

      <!-- Sidebar Footer -->
      <div class="sidebar-foot">
        <div class="sidebar-profile">
          <button
            type="button"
            class="sidebar-user-btn"
            @click="showProfile = true"
            :title="authStore.user?.name ? `Perfil: ${authStore.user.name}` : 'Mi perfil'"
          >
            <div class="sidebar-av">{{ authStore.initial }}</div>
            <div v-if="expanded" class="sidebar-user-info">
              <b>{{ authStore.user?.name || 'Usuario' }}</b>
              <small>{{ roleLabel }}</small>
            </div>
          </button>
          <div class="sidebar-foot-actions">
            <ThemeSwitcher :collapsed="!expanded" />
            <button
              type="button"
              class="sidebar-out-btn"
              @click="logout"
              title="Cerrar sesión"
              aria-label="Cerrar sesión"
            >
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
                <path d="M12 3v8M6.3 6.5a8 8 0 1 0 11.4 0" />
              </svg>
            </button>
          </div>
        </div>
      </div>

      <!-- User Profile Modal -->
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
          <div v-if="authStore.user.position" class="detail-row"><span class="detail-label">Puesto</span><span class="detail-value">{{ authStore.user.position }}</span></div>
          <div v-if="authStore.user.department" class="detail-row"><span class="detail-label">Departamento</span><span class="detail-value">{{ authStore.user.department }}</span></div>
          <div v-if="authStore.user.area" class="detail-row"><span class="detail-label">Área</span><span class="detail-value">{{ authStore.user.area }}</span></div>
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
import { ref, computed, onMounted, onUnmounted, h, defineComponent } from 'vue'
import { useRoute, useRouter, RouterLink, RouterView } from 'vue-router'
import { useAuthStore, USER_TYPE } from '@/features/auth/stores/auth'
import { useTheme } from '@/shared/composables/useTheme'
import { askConfirmation } from '@/shared/composables/useAlerts'
import AnimatedBackground from '@/shared/components/AnimatedBackground.vue'
import ThemeSwitcher from '@/shared/components/ThemeSwitcher.vue'
import AppModal from '@/shared/components/AppModal.vue'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const { currentTheme } = useTheme()

const expanded = ref(localStorage.getItem('sidebar-expanded') !== 'false')
const mobileOpen = ref(false)
const showProfile = ref(false)

const searchQuery = ref('')
const searchInput = ref<HTMLInputElement | null>(null)
const navEl = ref<HTMLElement | null>(null)

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

function isActive(match: string): boolean {
  const path = route.path
  return path.startsWith(`/${match}`) || path.startsWith(`/${match}/`)
}

async function logout() {
  const ok = await askConfirmation({ title: '¿Cerrar sesión?', confirmText: 'Sí, salir' })
  if (!ok) return
  await authStore.logout()
  router.push({ name: 'login' })
}

// Inline SVGs from Sidebar TecNM design
const createSvg = (svgContent: string) =>
  defineComponent({
    name: 'SidebarIcon',
    render() {
      return h('svg', {
        viewBox: '0 0 24 24',
        fill: 'none',
        stroke: 'currentColor',
        strokeWidth: '1.8',
        strokeLinecap: 'round',
        strokeLinejoin: 'round',
        'aria-hidden': 'true',
        innerHTML: svgContent,
      })
    },
  })

const IconPermisos = createSvg('<path d="M14 3H7a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V8z"/><path d="M14 3v5h5M9 13h6M9 17h6"/>')
const IconCortes = createSvg('<rect x="3" y="5" width="18" height="16" rx="2"/><path d="M8 3v4M16 3v4M3 10h18m-11 5 2 2 3-4"/>')
const IconUsuarios = createSvg('<circle cx="9" cy="8" r="3"/><circle cx="17" cy="9" r="2.5"/><path d="M3 20c0-3.5 2.7-6 6-6s6 2.5 6 6M16 14c3 0 5 2 5 5"/>')
const IconDepartamentos = createSvg('<rect x="9" y="3" width="6" height="5" rx="1"/><rect x="2" y="16" width="6" height="5" rx="1"/><rect x="16" y="16" width="6" height="5" rx="1"/><path d="M12 8v4M5 16v-4h14v4"/>')
const IconAreas = createSvg('<path d="M3 6a2 2 0 0 1 2-2h4l2 2h8a2 2 0 0 1 2 2v10a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/>')
const IconAuditoria = createSvg('<path d="M3 12a9 9 0 1 0 3-6.7L3 8"/><path d="M3 3v5h5M12 8v4l3 2"/>')
const IconApiKeys = createSvg('<circle cx="8" cy="8" r="4"/><path d="m11 11 9 9m-3-3 2-2m-5-1 2-2"/>')
const IconSync = createSvg('<path d="M12 14V4m-4 4 4-4 4 4M4 15v4h16v-4"/>')

interface NavItem {
  id: string
  label: string
  to: string
  icon: any
  section: 'navegacion' | 'gestion' | 'administracion'
  visible: () => boolean
  badge?: string | number
  match: string
}

const navItems: NavItem[] = [
  {
    id: 'permisos',
    label: 'Permisos',
    to: '/permissions',
    icon: IconPermisos,
    section: 'navegacion',
    visible: () => true,
    match: 'permissions',
  },
  {
    id: 'cortes',
    label: 'Cortes',
    to: '/cutoffs',
    icon: IconCortes,
    section: 'gestion',
    visible: () => authStore.isRHOrAdmin,
    match: 'cutoffs',
  },
  {
    id: 'usuarios',
    label: 'Usuarios',
    to: '/users',
    icon: IconUsuarios,
    section: 'gestion',
    visible: () => authStore.isRHOrAdmin,
    match: 'users',
  },
  {
    id: 'departamentos',
    label: 'Departamentos',
    to: '/departments',
    icon: IconDepartamentos,
    section: 'gestion',
    visible: () => authStore.isRHOrAdmin,
    match: 'departments',
  },
  {
    id: 'areas',
    label: 'Áreas',
    to: '/areas',
    icon: IconAreas,
    section: 'gestion',
    visible: () => authStore.isRHOrAdmin,
    match: 'areas',
  },
  {
    id: 'auditoria',
    label: 'Auditoría',
    to: '/audit',
    icon: IconAuditoria,
    section: 'administracion',
    visible: () => authStore.isAdmin,
    match: 'audit',
  },
  {
    id: 'api-keys',
    label: 'API Keys',
    to: '/api-keys',
    icon: IconApiKeys,
    section: 'administracion',
    visible: () => authStore.isAdmin,
    match: 'api-keys',
  },
  {
    id: 'sincronizar',
    label: 'Sincronizar',
    to: '/sync',
    icon: IconSync,
    section: 'administracion',
    visible: () => authStore.isRHOrAdmin,
    match: 'sync',
  },
]

const norm = (s: string) =>
  s.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase()

function itemMatches(item: NavItem): boolean {
  if (!item.visible()) return false
  const q = norm(searchQuery.value.trim())
  if (!q) return true
  return norm(item.label).includes(q)
}

function sectionItems(sec: 'navegacion' | 'gestion' | 'administracion'): NavItem[] {
  return navItems.filter((i) => i.section === sec && i.visible())
}

function hasVisibleItems(sec: 'navegacion' | 'gestion' | 'administracion'): boolean {
  return navItems.some((i) => i.section === sec && itemMatches(i))
}

const totalVisibleItems = computed(() => {
  return navItems.filter((i) => itemMatches(i)).length
})

function highlight(label: string, query: string): string {
  const q = norm(query.trim())
  if (!q) return label
  const idx = norm(label).indexOf(q)
  if (idx === -1) return label
  return `${label.slice(0, idx)}<mark>${label.slice(idx, idx + q.length)}</mark>${label.slice(idx + q.length)}`
}

function handleSearchKeydown(e: KeyboardEvent) {
  if (e.key === 'ArrowDown') {
    e.preventDefault()
    const firstLink = navEl.value?.querySelector<HTMLAnchorElement>('.sidebar-item:not(.hide)')
    firstLink?.focus()
  } else if (e.key === 'Enter') {
    const firstLink = navEl.value?.querySelector<HTMLAnchorElement>('.sidebar-item:not(.hide)')
    firstLink?.click()
  } else if (e.key === 'Escape') {
    searchQuery.value = ''
    searchInput.value?.blur()
  }
}

function handleNavKeydown(e: KeyboardEvent) {
  const items = Array.from(navEl.value?.querySelectorAll<HTMLAnchorElement>('.sidebar-item:not(.hide)') || [])
  const currentIndex = items.indexOf(document.activeElement as HTMLAnchorElement)
  if (currentIndex === -1) return

  if (e.key === 'ArrowDown') {
    e.preventDefault()
    const next = items[currentIndex + 1] || items[currentIndex]
    next.focus()
  } else if (e.key === 'ArrowUp') {
    e.preventDefault()
    if (currentIndex === 0) {
      searchInput.value?.focus()
    } else {
      items[currentIndex - 1]?.focus()
    }
  }
}

function onGlobalKeydown(e: KeyboardEvent) {
  const target = e.target as HTMLElement | null
  const isInput =
    target &&
    (target.tagName === 'INPUT' ||
      target.tagName === 'TEXTAREA' ||
      target.isContentEditable)
  if (e.key === '/' && !isInput) {
    e.preventDefault()
    if (!expanded.value) {
      expanded.value = true
      localStorage.setItem('sidebar-expanded', 'true')
    }
    setTimeout(() => {
      searchInput.value?.focus()
    }, 60)
  }
}

onMounted(() => {
  window.addEventListener('keydown', onGlobalKeydown)
})

onUnmounted(() => {
  window.removeEventListener('keydown', onGlobalKeydown)
})
</script>

<style scoped>
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
