<template>
  <div>
    <AppPageHeader title="Gestión de Permisos" subtitle="Administra los permisos solicitados" icon="fa-file-text-o">
      <template #actions>
        <AppButton variant="primary" icon="fa-plus" @click="$router.push({ name: 'permissions.create' })">
          Nuevo Permiso
        </AppButton>
      </template>
    </AppPageHeader>

    <!-- Filter panel (Panel de Búsqueda con footer interno estandarizado) -->
    <section class="tecnm-card tecnm-mb-4 filter-card">
      <div class="tecnm-card-header tecnm-card-header--premium">
        <h4 class="filter-title">
          <i class="fa fa-filter" aria-hidden="true"></i> Panel de Búsqueda
        </h4>
      </div>
      <form @submit.prevent="applyFilters" class="filter-form">
        <div class="tecnm-card-body">
          <div class="filter-grid">
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
                <option value="5">Cancelado por el Solicitante</option>
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
          </div>
        </div>

        <div class="filter-card-footer">
          <AppButton type="button" variant="secondary" icon="fa-undo" @click="clearFilters">Limpiar Filtros</AppButton>
          <AppButton type="submit" variant="primary" icon="fa-search">Buscar</AppButton>
        </div>
      </form>
    </section>

    <!-- Leyenda de estados -->
    <details class="tecnm-card legend-box">
      <summary>
        <i class="fa fa-question-circle" aria-hidden="true"></i>
        ¿Qué significa cada estado?
      </summary>
      <p><StatusBadge variant="pending">Pendiente por Revisar</StatusBadge> — el permiso fue solicitado y espera revisión del autorizador o Recursos Humanos. Mientras esté así, se puede editar o cancelar.</p>
      <p><StatusBadge variant="approved">Revisado / Aprobado</StatusBadge> — autorizado. Puede ser <em>con goce</em> (se paga) o <em>sin goce</em>, según lo acordado en la revisión.</p>
      <p><StatusBadge variant="rejected">Revisado / Rechazado</StatusBadge> — no autorizado; queda registrado como historial pero sin efecto de pago.</p>
      <p><StatusBadge variant="info">Cancelado por el Solicitante</StatusBadge> — el permiso fue cancelado por quien lo solicitó estando pendiente y dentro de un periodo vigente. Solo es visible para su solicitante, Recursos Humanos y el administrador; no para autorizadores ni terceros.</p>
      <p class="legend-note"><i class="fa fa-clock-o" aria-hidden="true"></i> Al cerrarse un corte, sus permisos pendientes se resuelven automáticamente y pasan a Rechazado. Un permiso revisado ya no se puede modificar, y un permiso solo se cancela estando pendiente dentro de un periodo vigente.</p>
    </details>

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
                <th class="text-center">Tipo</th>
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
                  <span class="type-cell">
                    {{ p.type }}
                    <i v-if="p.permissionTypeId === 5" class="fa fa-chalkboard-teacher type-badge" :title="p.shiftDescription || 'Falta docente'" aria-hidden="true"></i>
                  </span>
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

                      <!-- Cancelar: solo el solicitante o RH/Admin, no autorizadores -->
                      <button
                        v-if="isOwnerOf(p) || authStore.isRHOrAdmin"
                        type="button"
                        class="tecnm-action-btn tecnm-action-btn--delete"
                        :class="{ 'tecnm-action-btn--locked': p.permissionStatusTypeId !== PERMISSION_STATUS.PENDING }"
                        :title="p.permissionStatusTypeId === PERMISSION_STATUS.PENDING ? 'Cancelar permiso' : 'Solo se cancela un permiso pendiente dentro de un periodo vigente'"
                        :disabled="p.permissionStatusTypeId !== PERMISSION_STATUS.PENDING"
                        @click="onCancel(p)"
                      >
                        <i :class="p.permissionStatusTypeId === PERMISSION_STATUS.PENDING ? 'fa fa-ban' : 'fa fa-lock'" aria-hidden="true"></i>
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
                <td colspan="7"><EmptyState message="No hay permisos registrados"><span>Intenta ajustar los filtros de búsqueda</span></EmptyState></td>
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
import AppPageHeader from '@/shared/components/AppPageHeader.vue'
import AppButton from '@/shared/components/AppButton.vue'
import AppSpinner from '@/shared/components/AppSpinner.vue'
import EmptyState from '@/shared/components/EmptyState.vue'
import StatusBadge from '@/shared/components/StatusBadge.vue'
import PaginationBar from '@/shared/components/PaginationBar.vue'
import {
  fetchPermissions,
  cancelPermission,
  statusLabel,
  PERMISSION_STATUS,
  type PermissionListPage,
  type PermissionListItem
} from "@/features/permissions/api/permissions"
import { formatTecNMDate } from '@/shared/composables/useDates'
import { showToast } from '@/shared/composables/useAlerts'
import { askConfirmation } from '@/shared/composables/useAlerts'
import { useAuthStore } from '@/features/auth/stores/auth'
import { fetchAreas, type AreaItem } from '@/features/departments/api/catalogs'

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

async function onCancel(p: PermissionListItem) {
  const ok = await askConfirmation({
    title: '¿Cancelar permiso?',
    text: `Se cancelará el permiso #${p.id} de ${p.requester}. Esta acción no puede revertirse.`,
    confirmText: 'Sí, cancelar',
    danger: true
  })
  if (!ok) return
  try {
    await cancelPermission(p.id)
    showToast('Permiso cancelado correctamente', 'success')
    if (items.value.length === 1 && page.value > 1) page.value -= 1
    load()
  } catch (err) {
    console.error('Error al cancelar permiso:', err)
    showToast('No se pudo cancelar el permiso.', 'error')
  }
}

function isOwnerOf(p: PermissionListItem): boolean {
  return !!p.requestingUserId && p.requestingUserId === authStore.user?.id
}

function badgeVariant(id: number): 'approved' | 'pending' | 'rejected' | 'info' {
  switch (id) {
    case PERMISSION_STATUS.PENDING:    return 'pending'
    case PERMISSION_STATUS.APPROVED:   return 'approved'
    case PERMISSION_STATUS.REJECTED:   return 'rejected'
    case PERMISSION_STATUS.CANCELLED:  return 'info'
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
.legend-box {
  margin-bottom: 1.5rem;
  padding: 0.75rem 1rem;
}

.legend-box summary {
  cursor: pointer;
  font-weight: 600;
  color: var(--tecnm-primary, #0d6efd);
  display: flex;
  align-items: center;
  gap: 0.5rem;
  user-select: none;
}

.legend-box p {
  margin: 0.6rem 0 0;
  font-size: 0.9rem;
}

.legend-note {
  background: var(--tecnm-soft-hover);
  padding: 0.55rem 0.75rem;
  border-radius: 0.375rem;
}

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

.type-cell {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
}

.type-badge {
  color: var(--tecnm-accent-text);
  font-size: 0.85rem;
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