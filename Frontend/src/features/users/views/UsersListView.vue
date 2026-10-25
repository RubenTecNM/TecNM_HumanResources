<template>
  <div>
    <AppPageHeader title="Gestión de Usuarios" subtitle="Administra los colaboradores, roles y departamentos" icon="fa-users">
      <template #actions>
        <AppButton variant="primary" icon="fa-plus" @click="$router.push({ name: 'users.create' })">
          Nuevo Usuario
        </AppButton>
      </template>
    </AppPageHeader>

    <!-- Panel de búsqueda estilo TecNM -->
    <section class="tecnm-card tecnm-mb-4">
      <div class="tecnm-card-header tecnm-card-header--premium">
        <h4 class="filter-title">
          <i class="fa fa-filter" aria-hidden="true"></i> Panel de Búsqueda
        </h4>
      </div>
      <div class="tecnm-card-body">
        <form @submit.prevent="applyFilters" class="filter-grid">
          <div class="tecnm-form-group">
            <label class="tecnm-label" for="userName">Nombre / Apellido</label>
            <input id="userName" v-model="filters.name" type="text" class="tecnm-form-control" placeholder="Buscar por nombre..." />
          </div>

          <div class="tecnm-form-group">
            <label class="tecnm-label" for="userEmail">Correo Electrónico</label>
            <input id="userEmail" v-model="filters.email" type="text" class="tecnm-form-control" placeholder="Buscar por correo..." />
          </div>

          <div class="tecnm-form-group">
            <label class="tecnm-label" for="departmentFilter">Departamento</label>
            <select id="departmentFilter" v-model="filters.departmentId" class="tecnm-form-control">
              <option value="">Todos</option>
              <option v-for="dept in departments" :key="dept.id" :value="dept.id">{{ dept.description }}</option>
            </select>
          </div>

          <div class="tecnm-form-group">
            <label class="tecnm-label" for="statusFilter">Estado</label>
            <select id="statusFilter" v-model="filters.status" class="tecnm-form-control">
              <option value="">Cualquiera</option>
              <option value="true">Activo</option>
              <option value="false">Inactivo</option>
            </select>
          </div>

          <div class="tecnm-form-group filter-actions">
            <AppButton type="submit" variant="primary" icon="fa-search">Buscar</AppButton>
            <AppButton type="button" variant="secondary" icon="fa-undo" @click="clearFilters">Limpiar</AppButton>
          </div>
        </form>
      </div>
    </section>

    <AppSpinner v-if="loading" />
    <template v-else>
      <section class="tecnm-card">
        <div class="tecnm-card-header tecnm-card-header--premium">
          <h4 class="filter-title">Listado de Usuarios</h4>
          <span class="count-badge">{{ users.length }} registro(s)</span>
        </div>

        <div class="tecnm-table-responsive">
          <table class="tecnm-table tecnm-table-striped">
            <thead>
              <tr>
                <th class="text-center">ID</th>
                <th class="text-center">Colaborador</th>
                <th class="text-center">Correo</th>
                <th class="text-center">Departamento</th>
                <th class="text-center">Rol / Tipo</th>
                <th class="text-center">Estado</th>
                <th class="text-center">Creación</th>
                <th class="text-center">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="u in users" :key="u.id">
                <td class="text-center"><strong>#{{ u.id }}</strong></td>
                <td>
                  <strong>{{ u.name }}</strong>
                  <div class="small-text">{{ u.position || 'Sin puesto asignado' }}</div>
                </td>
                <td class="text-center">{{ u.email }}</td>
                <td class="text-center">{{ u.department || 'N/A' }}</td>
                <td class="text-center">
                  <span class="role-badge" :class="`role-badge--${roleVariant(u.userTypeId)}`">{{ u.userType || 'Estándar' }}</span>
                </td>
                <td class="text-center">
                  <StatusBadge :variant="u.status ? 'approved' : 'rejected'">
                    {{ u.status ? 'Activo' : 'Inactivo' }}
                  </StatusBadge>
                </td>
                <td class="text-center">
                  <div class="datetime-cell">
                    <span class="datetime-date">{{ formatDateCell(u.createdAt) }}</span>
                    <span class="datetime-time">{{ formatTimeCell(u.createdAt) }}</span>
                  </div>
                </td>
                <td class="text-center">
                  <div class="tecnm-table-actions">
                    <button
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--edit"
                      title="Editar usuario"
                      @click="$router.push({ name: 'users.edit', params: { id: u.id } })"
                    >
                      <i class="fa fa-edit" aria-hidden="true"></i>
                    </button>
                    <button
                      v-if="u.status"
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--delete"
                      title="Desactivar usuario"
                      @click="toggleDeactivate(u)"
                    >
                      <i class="fa fa-ban" aria-hidden="true"></i>
                    </button>
                    <button
                      v-else
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--approve"
                      title="Reactivar usuario"
                      @click="toggleReactivate(u)"
                    >
                      <i class="fa fa-check" aria-hidden="true"></i>
                    </button>
                    <button
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--view"
                      title="Ver detalles del usuario"
                      @click="openDetail(u.id)"
                    >
                      <i class="fa fa-eye" aria-hidden="true"></i>
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="!users.length">
                <td colspan="8"><EmptyState message="No hay usuarios registrados"><span>Intenta ajustar los filtros de búsqueda</span></EmptyState></td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="users.length" class="tecnm-card-footer">
          <div class="pagination-wrap">
            <span class="pagination-info">
              <i class="fa fa-list-ol" aria-hidden="true"></i>
              Mostrando página <strong>{{ page }}</strong> de <strong>{{ totalPages }}</strong>
            </span>
            <PaginationBar
              :page="page"
              :total-pages="totalPages"
              @update:page="(v: number) => { page = v; fetchUsers() }"
            />
          </div>
        </div>
      </section>
    </template>
    <!-- Modal de detalle de usuario -->
    <AppModal
      :show="detailModal.open"
      :title="`Ficha de Usuario #${detailModal.id}`"
      @close="detailModal.open = false"
    >
      <template #default>
        <AppSpinner v-if="detailModal.loading" />
        <div v-else-if="detailModal.error" class="alert alert-danger">
          <i class="fa fa-exclamation-triangle me-2"></i>{{ detailModal.error }}
        </div>
        <div v-else class="user-detail-grid">
          <div class="detail-row"><span class="detail-label">Nombre</span><span class="detail-value">{{ detailModal.data?.name }}</span></div>
          <div class="detail-row"><span class="detail-label">Correo</span><span class="detail-value">{{ detailModal.data?.email }}</span></div>
          <div class="detail-row"><span class="detail-label">Departamento</span><span class="detail-value">{{ detailModal.data?.department || 'N/A' }}</span></div>
          <div class="detail-row"><span class="detail-label">Área</span><span class="detail-value">{{ detailModal.data?.area || 'N/A' }}</span></div>
          <div class="detail-row"><span class="detail-label">Puesto</span><span class="detail-value">{{ detailModal.data?.position || 'N/A' }}</span></div>
          <div class="detail-row"><span class="detail-label">Tipo de Usuario</span><span class="detail-value">{{ detailModal.data?.userType || 'N/A' }}</span></div>
          <div v-if="detailModal.data?.tempPassword" class="detail-row">
            <span class="detail-label">Clave temporal</span>
            <span class="detail-value temp-password">
              {{ detailModal.data.tempPassword }}
              <button type="button" class="temp-copy" title="Copiar" @click="copyTempPassword(detailModal.data!.tempPassword!)">
                <i class="fa fa-clipboard" aria-hidden="true"></i>
              </button>
              <small>(visible hasta que el usuario ingrese)</small>
            </span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Estado</span>
            <StatusBadge :variant="detailModal.data?.status ? 'approved' : 'rejected'">
              {{ detailModal.data?.status ? 'Activo' : 'Inactivo' }}
            </StatusBadge>
          </div>
          <div class="detail-row"><span class="detail-label">Alta en sistema</span><span class="detail-value">{{ formatDateCell(detailModal.data?.createdAt) }}</span></div>
          <div class="detail-row" v-if="detailModal.data?.updatedAt"><span class="detail-label">Última modificación</span><span class="detail-value">{{ formatDateCell(detailModal.data?.updatedAt) }}</span></div>
          <div class="detail-row" v-if="detailModal.data?.totalPermissions != null">
            <span class="detail-label">Total de permisos</span>
            <span class="detail-value">{{ detailModal.data.totalPermissions }}</span>
          </div>
        </div>
      </template>
      <template #footer>
        <AppButton variant="secondary" @click="detailModal.open = false">Cerrar</AppButton>
        <AppButton variant="primary" icon="fa-edit" @click="$router.push({ name: 'users.edit', params: { id: detailModal.id } })">Editar</AppButton>
      </template>
    </AppModal>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { http } from "@/shared/api/client"
import AppPageHeader from '@/shared/components/AppPageHeader.vue'
import AppButton from '@/shared/components/AppButton.vue'
import AppModal from '@/shared/components/AppModal.vue'
import AppSpinner from '@/shared/components/AppSpinner.vue'
import EmptyState from '@/shared/components/EmptyState.vue'
import StatusBadge from '@/shared/components/StatusBadge.vue'
import PaginationBar from '@/shared/components/PaginationBar.vue'
import { formatTecNMDate } from '@/shared/composables/useDates'
import { askConfirmation, showToast } from '@/shared/composables/useAlerts'
import { fetchDepartments, type CatalogItem } from '@/features/departments/api/catalogs'

interface UserItem {
  id: number
  name: string
  email: string
  status: boolean
  department: string | null
  area: string | null
  position: string | null
  userType: string | null
  userTypeId?: number
  createdAt?: string
}

interface UserDetail {
  id: number
  name: string
  email: string
  status: boolean
  department?: string | null
  area?: string | null
  position?: string | null
  userType?: string | null
  createdAt?: string | null
  updatedAt?: string | null
  totalPermissions?: number
  tempPassword?: string | null
}

async function copyTempPassword(pwd: string) {
  try { await navigator.clipboard.writeText(pwd) } catch { /* sin permisos de portapapeles */ }
}

function formatDateCell(iso?: string | null): string {
  if (!iso) return '—'
  return formatTecNMDate(iso)
}

function formatTimeCell(iso?: string | null): string {
  if (!iso) return ''
  const d = new Date(iso)
  if (isNaN(d.getTime())) return ''
  return d.toLocaleTimeString('es-MX', { hour: '2-digit', minute: '2-digit' })
}

const users = ref<UserItem[]>([])
const departments = ref<CatalogItem[]>([])
const loading = ref(true)
const page = ref(1)
const totalPages = ref(1)

const filters = reactive({
  name: '',
  email: '',
  departmentId: '',
  status: ''
})

const detailModal = reactive({
  open: false,
  loading: false,
  error: '',
  id: 0,
  data: null as UserDetail | null
})

async function openDetail(id: number) {
  detailModal.id = id
  detailModal.data = null
  detailModal.error = ''
  detailModal.loading = true
  detailModal.open = true
  try {
    const res = await http.get(`/api/Users/${id}`)
    detailModal.data = res.data as UserDetail
  } catch (e: any) {
    detailModal.error = e.response?.data?.message || 'No se pudo cargar el usuario.'
  } finally {
    detailModal.loading = false
  }
}

async function fetchUsers() {
  loading.value = true
  try {
    const params = new URLSearchParams()
    params.append('page', page.value.toString())
    params.append('pageSize', '10')

    if (filters.name) params.append('name', filters.name)
    if (filters.email) params.append('email', filters.email)
    if (filters.departmentId) params.append('departmentId', filters.departmentId)
    if (filters.status) params.append('status', filters.status)

    const res = await http.get(`/api/Users?${params.toString()}`)
    const data = res.data

    if (res.status === 200) {
      users.value = data.items || []
      totalPages.value = data.totalPages || 1
    }
  } catch (err) {
    showToast('Error cargando usuarios.', 'error')
  } finally {
    loading.value = false
  }
}

function applyFilters() {
  page.value = 1
  fetchUsers()
}

function clearFilters() {
  filters.name = ''
  filters.email = ''
  filters.departmentId = ''
  filters.status = ''
  applyFilters()
}

async function toggleDeactivate(user: UserItem) {
  const ok = await askConfirmation({
    title: '¿Desactivar Colaborador?',
    text: `Se suspenderá el acceso a ${user.name}.`,
    confirmText: 'Sí, desactivar'
  })

  if (ok) {
    try {
      await http.put(`/api/Users/${user.id}/deactivate`)
      showToast('El usuario ha sido desactivado.', 'success')
      fetchUsers()
    } catch (e: any) {
      showToast(e.response?.data?.message || 'No se pudo desactivar el usuario.', 'error')
    }
  }
}

async function toggleReactivate(user: UserItem) {
  const ok = await askConfirmation({
    title: '¿Reactivar Colaborador?',
    text: `Se reactivará el acceso de ${user.name}.`,
    confirmText: 'Sí, reactivar'
  })

  if (ok) {
    try {
      await http.put(`/api/Users/${user.id}/activate`)
      showToast('El usuario ha sido reactivado.', 'success')
      fetchUsers()
    } catch (e: any) {
      showToast(e.response?.data?.message || 'No se pudo reactivar el usuario.', 'error')
    }
  }
}

function roleVariant(userTypeId?: number): string {
  switch (userTypeId) {
    case 1: return 'admin'
    case 2: return 'authorizer'
    case 3: return 'rh'
    default: return 'employee'
  }
}

onMounted(() => {
  fetchDepartments()
    .then((res) => { departments.value = res })
    .catch(() => { departments.value = [] })
  fetchUsers()
})
</script>

<style scoped>
.filter-title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin: 0;
}

.filter-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 0 1.25rem;
}

.filter-actions {
  display: flex;
  align-items: flex-end;
  gap: 0.5rem;
}

.count-badge {
  font-size: 0.82rem;
  font-weight: 600;
  color: var(--tecnm-accent-text);
  background: var(--tecnm-soft-hover);
  padding: 0.2rem 0.6rem;
  border-radius: 9999px;
}

.small-text {
  font-size: 0.8rem;
  color: var(--tecnm-text-secondary);
}

.role-badge {
  font-size: 0.82rem;
  background: var(--tecnm-bg-light);
  color: var(--tecnm-text);
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
  border: 1px solid var(--tecnm-border);
}

.role-badge--admin { background: #eef2ff; color: #4338ca; border-color: #c7d2fe; }
.role-badge--authorizer { background: #fef3c7; color: #92400e; border-color: #fde68a; }
.role-badge--rh { background: #ecfdf5; color: #065f46; border-color: #a7f3d0; }
.role-badge--employee { background: #f3f4f6; color: #374151; border-color: #d1d5db; }

.pagination-wrap {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
}

.pagination-info {
  display: flex;
  align-items: center;
  gap: 0.3rem;
  font-size: 0.85rem;
  color: var(--tecnm-text-secondary);
}

.datetime-cell {
  display: flex;
  flex-direction: column;
  line-height: 1.3;
}

.datetime-date {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--tecnm-text-primary);
}

.datetime-time {
  font-size: 0.75rem;
  color: var(--tecnm-text-secondary);
  margin-top: 0.125rem;
}

.text-center {
  text-align: center;
}

@media (max-width: 640px) {
  .filter-grid {
    grid-template-columns: 1fr;
  }
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
  min-width: 160px;
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

.temp-password { display: inline-flex; align-items: center; gap: 0.4rem; flex-wrap: wrap; }
.temp-password small { color: var(--tecnm-text-secondary); font-weight: 400; }
.temp-copy {
  border: none;
  background: transparent;
  cursor: pointer;
  color: var(--tecnm-blue-primary);
  font-size: 0.85rem;
}
</style>
