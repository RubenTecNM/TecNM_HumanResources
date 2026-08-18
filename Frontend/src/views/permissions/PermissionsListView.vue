<template>
  <div>
    <AppPageHeader title="Gestión de Permisos" subtitle="Administra los permisos solicitados" icon="fa-file-text-o">
      <template #actions>
        <AppButton variant="primary" icon="fa-plus" @click="$router.push({ name: 'permissions.create' })">
          Nuevo Permiso
        </AppButton>
      </template>
    </AppPageHeader>

    <!-- Filter panel (igual que el original: Panel de Búsqueda) -->
    <section class="tecnm-card tecnm-mb-4">
      <div class="tecnm-card-header tecnm-card-header--premium">
        <h4 class="filter-title">
          <i class="fa fa-filter" aria-hidden="true"></i> Panel de Búsqueda
        </h4>
      </div>
      <div class="tecnm-card-body">
        <form @submit.prevent="applyFilters" class="filter-grid">
          <div class="tecnm-form-group">
            <label class="tecnm-label" for="dateFrom">Vigencia Desde</label>
            <input id="dateFrom" v-model="filters.fromDay" type="date" class="tecnm-form-control" />
          </div>

          <div class="tecnm-form-group">
            <label class="tecnm-label" for="dateTo">Vigencia Hasta</label>
            <input id="dateTo" v-model="filters.toDay" type="date" class="tecnm-form-control" />
          </div>

          <div class="tecnm-form-group">
            <label class="tecnm-label" for="statusFilter">Estado</label>
            <select id="statusFilter" v-model="filters.statusId" class="tecnm-form-control">
              <option value="">Cualquiera</option>
              <option value="1">Pendiente por Revisar</option>
              <option value="2">Revisado / Aprobado</option>
              <option value="4">Revisado / Rechazado</option>
              <option value="3">Invalidado por Periodo</option>
            </select>
          </div>

          <div class="tecnm-form-group">
            <label class="tecnm-label" for="userFilter">Creado por</label>
            <input id="userFilter" v-model="filters.userName" type="text" class="tecnm-form-control" placeholder="Nombre del usuario" />
          </div>

          <div v-if="authStore.canManageUsers" class="tecnm-form-group">
            <label class="tecnm-label" for="areaFilter">Área</label>
            <select id="areaFilter" v-model="filters.areaId" class="tecnm-form-control">
              <option value="">Todas</option>
              <option v-for="area in areas" :key="area.id" :value="area.id">{{ area.description }}</option>
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
          <h4 class="filter-title">Listado de Permisos</h4>
          <span class="count-badge">{{ pageData.totalItems }} registro(s)</span>
        </div>

        <div class="tecnm-table-responsive">
          <table class="tecnm-table tecnm-table-striped">
            <thead>
              <tr>
                <th class="text-center">ID</th>
                <th class="text-center">Solicitante</th>
                <th class="text-center">Estado</th>
                <th class="text-center">Creación</th>
                <th class="text-center">Modificación</th>
                <th class="text-center">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="p in items" :key="p.id">
                <td class="text-center">{{ p.id }}</td>
                <td>
                  <strong>{{ p.requester || '-' }}</strong>
                </td>
                <td class="text-center">
                  <StatusBadge :variant="badgeVariant(p.permissionStatusTypeId)">
                    {{ statusLabel(p.permissionStatusTypeId) }}
                  </StatusBadge>
                </td>
                <td class="text-center">
                  <div class="datetime-cell">
                    <span class="datetime-date">{{ formatDateCell(p.createdAt) }}</span>
                    <span class="datetime-time">{{ formatTimeCell(p.createdAt) }}</span>
                  </div>
                </td>
                <td class="text-center">
                  <div class="datetime-cell">
                    <span class="datetime-date">{{ formatDateCell(p.updatedAt) }}</span>
                    <span class="datetime-time">{{ formatTimeCell(p.updatedAt) }}</span>
                  </div>
                </td>
                <td class="text-center">
                  <div class="tecnm-table-actions">
                    <!-- Revisar/Autorizar: solo autorizadores, solo si está pendiente -->
                    <button
                      v-if="authStore.isAuthorizer"
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--approve"
                      title="Revisar y autorizar permiso"
                      :disabled="p.permissionStatusTypeId !== PERMISSION_STATUS.PENDING"
                      @click="$router.push({ name: 'permissions.authorize', params: { id: String(p.id) } })"
                    >
                      <i class="fa fa-user-check" aria-hidden="true"></i>
                    </button>

                    <!-- Editar: admin/RH, solo pendiente, no autorizadores -->
                    <template v-if="!authStore.isAuthorizer">
                      <button
                        type="button"
                        class="tecnm-action-btn tecnm-action-btn--edit"
                        :class="{ 'tecnm-action-btn--locked': p.permissionStatusTypeId !== PERMISSION_STATUS.PENDING }"
                        :title="p.permissionStatusTypeId === PERMISSION_STATUS.PENDING ? 'Editar permiso' : 'No se puede editar fuera del periodo vigente'"
                        :disabled="p.permissionStatusTypeId !== PERMISSION_STATUS.PENDING"
                        @click="goEdit(p.id)"
                      >
                        <i :class="p.permissionStatusTypeId === PERMISSION_STATUS.PENDING ? 'fa fa-edit' : 'fa fa-lock'" aria-hidden="true"></i>
                      </button>

                      <!-- Eliminar: admin/RH, no autorizadores -->
                      <button
                        type="button"
                        class="tecnm-action-btn tecnm-action-btn--delete"
                        :class="{ 'tecnm-action-btn--locked': p.permissionStatusTypeId !== PERMISSION_STATUS.PENDING }"
                        :title="p.permissionStatusTypeId === PERMISSION_STATUS.PENDING ? 'Eliminar permiso' : 'No se puede eliminar un permiso revisado de un periodo cerrado'"
                        :disabled="p.permissionStatusTypeId !== PERMISSION_STATUS.PENDING"
                        @click="onDelete(p)"
                      >
                        <i class="fa fa-trash" aria-hidden="true"></i>
                      </button>
                    </template>

                    <!-- Ver detalles: siempre visible -->
                    <button
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--view"
                      title="Ver detalles"
                      @click="goDetails(p.id)"
                    >
                      <i class="fa fa-eye" aria-hidden="true"></i>
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="!items.length">
                <td colspan="6"><EmptyState message="No hay permisos registrados"><span>Intenta ajustar los filtros de búsqueda</span></EmptyState></td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="items.length" class="tecnm-card-footer">
          <div class="pagination-wrap">
            <span class="pagination-info">
              <i class="fa fa-list-ol" aria-hidden="true"></i>
              Mostrando <strong>{{ pageData.firstItemOnPage }}</strong> - <strong>{{ pageData.lastItemOnPage }}</strong> de
              <strong>{{ pageData.totalItems }}</strong> permisos
            </span>
            <PaginationBar
              :page="pageData.page"
              :total-pages="pageData.totalPages"
              @update:page="(v: number) => { page = v; load() }"
            />
          </div>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import AppPageHeader from '@/components/AppPageHeader.vue'
import AppButton from '@/components/AppButton.vue'
import AppSpinner from '@/components/AppSpinner.vue'
import EmptyState from '@/components/EmptyState.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import PaginationBar from '@/components/PaginationBar.vue'
import {
  fetchPermissions,
  deactivatePermission,
  statusLabel,
  PERMISSION_STATUS,
  type PermissionListPage,
  type PermissionListItem
} from '@/api/permissions'
import { formatTecNMDate } from '@/composables/useDates'
import { showToast } from '@/composables/useAlerts'
import { askConfirmation } from '@/composables/useAlerts'
import { useAuthStore } from '@/stores/auth'
import { fetchAreas, type AreaItem } from '@/api/catalogs'

const router = useRouter()
const authStore = useAuthStore()
const items = ref<PermissionListItem[]>([])
const areas = ref<AreaItem[]>([])
const loading = ref(false)
const page = ref(1)
const pageSize = 10

const pageData = reactive<PermissionListPage>({
  items: [],
  page: 1,
  pageSize,
  totalItems: 0,
  totalPages: 1,
  firstItemOnPage: 0,
  lastItemOnPage: 0
})

const filters = reactive({
  statusId: '' as string | number,
  fromDay: '',
  toDay: '',
  userName: '',
  areaId: '' as string | number
})

async function load() {
  loading.value = true
  try {
    const params: Record<string, unknown> = { page: page.value, pageSize }
    if (filters.statusId !== '') params.statusId = filters.statusId
    if (filters.fromDay) params.fromDay = filters.fromDay
    if (filters.toDay) params.toDay = filters.toDay
    if (filters.userName) params.userName = filters.userName
    if (filters.areaId !== '') params.areaId = filters.areaId
    const res = await fetchPermissions(params)
    items.value = res.items
    Object.assign(pageData, res)
  } catch (err) {
    console.error('Error cargando permisos:', err)
    items.value = []
    Object.assign(pageData, {
      items: [],
      page: page.value,
      pageSize,
      totalItems: 0,
      totalPages: 1,
      firstItemOnPage: 0,
      lastItemOnPage: 0
    })
    showToast('No se pudieron cargar los permisos.', 'error')
  } finally {
    loading.value = false
  }
}

function applyFilters() {
  page.value = 1
  load()
}

function clearFilters() {
  filters.statusId = ''
  filters.fromDay = ''
  filters.toDay = ''
  filters.userName = ''
  filters.areaId = ''
  applyFilters()
}

function goDetails(id: number) {
  router.push({ name: 'permissions.details', params: { id: String(id) } })
}

function goEdit(id: number) {
  router.push({ name: 'permissions.edit', params: { id: String(id) } })
}

async function onDelete(p: PermissionListItem) {
  const ok = await askConfirmation({
    title: '¿Eliminar permiso?',
    text: `Se invalidará el permiso #${p.id} de ${p.requester}. Esta acción no puede revertirse.`,
    confirmText: 'Sí, eliminar',
    danger: true
  })
  if (!ok) return
  try {
    await deactivatePermission(p.id)
    showToast('Permiso eliminado correctamente', 'success')
    if (items.value.length === 1 && page.value > 1) page.value -= 1
    load()
  } catch (err) {
    console.error('Error al eliminar permiso:', err)
    showToast('No se pudo eliminar el permiso.', 'error')
  }
}

function badgeVariant(id: number): 'approved' | 'pending' | 'rejected' | 'info' {
  switch (id) {
    case PERMISSION_STATUS.PENDING:    return 'pending'
    case PERMISSION_STATUS.APPROVED:   return 'approved'
    case PERMISSION_STATUS.REJECTED:   return 'rejected'
    case PERMISSION_STATUS.INVALIDATED: return 'rejected'
    default: return 'info'
  }
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

onMounted(async () => {
  if (authStore.canManageUsers) {
    try {
      areas.value = await fetchAreas()
    } catch (err) {
      console.error('Error cargando áreas:', err)
    }
  }
  load()
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

.text-center {
  text-align: center;
}


/* Fecha + hora en dos líneas (como el original) */
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

/* Acciones (como el original: view/edit/delete) */
.tecnm-table-actions {
  display: flex;
  justify-content: center;
  gap: 0.4rem;
}

.tecnm-action-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
  transform: none !important;
}

.tecnm-action-btn--edit {
  color: var(--tecnm-accent-text);
}

.tecnm-action-btn--edit:hover:not(:disabled) {
  color: var(--tecnm-accent-text);
  border-color: var(--tecnm-blue-hover);
  background: var(--tecnm-soft-hover);
}

.tecnm-action-btn--delete {
  color: var(--tecnm-danger);
}

.tecnm-action-btn--delete:hover:not(:disabled) {
  color: #ffffff;
  border-color: var(--tecnm-danger);
  background: var(--tecnm-danger);
}

.tecnm-action-btn--approve {
  color: #059669;
}

.tecnm-action-btn--approve:hover:not(:disabled) {
  color: #ffffff;
  border-color: #059669;
  background: #059669;
}

.tecnm-action-btn--locked {
  opacity: 0.45;
  cursor: not-allowed !important;
}

/* Paginación con info "Mostrando X - Y de Z" */
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

@media (max-width: 640px) {
  .filter-grid {
    grid-template-columns: 1fr;
  }
}
</style>