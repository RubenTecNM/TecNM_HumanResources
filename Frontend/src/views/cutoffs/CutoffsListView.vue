<template>
  <div>
    <AppPageHeader title="Gestión de Cortes" subtitle="Administra los periodos de cortes de nómina" icon="fa-calendar">
      <template #actions>
        <AppButton variant="primary" icon="fa-plus" @click="$router.push({ name: 'cutoffs.create' })">
          Nuevo Periodo
        </AppButton>
      </template>
    </AppPageHeader>

    <!-- Panel de búsqueda estilo TecNM (idéntico a Permisos) -->
    <section class="tecnm-card tecnm-mb-4">
      <div class="tecnm-card-header tecnm-card-header--premium">
        <h4 class="filter-title">
          <i class="fa fa-filter" aria-hidden="true"></i> Panel de Búsqueda
        </h4>
      </div>
<div class="tecnm-card-body">
          <form @submit.prevent="applyFilters" class="filter-grid">
          <div class="tecnm-form-group">
            <label class="tecnm-label" for="dateFrom">Fecha Inicio Desde</label>
            <input id="dateFrom" v-model="filters.dateFrom" type="date" class="tecnm-form-control" />
          </div>

          <div class="tecnm-form-group">
            <label class="tecnm-label" for="dateTo">Fecha Fin Hasta</label>
            <input id="dateTo" v-model="filters.dateTo" type="date" class="tecnm-form-control" />
          </div>

          <div class="tecnm-form-group">
            <label class="tecnm-label" for="statusFilter">Estado</label>
            <select id="statusFilter" v-model="filters.statusId" class="tecnm-form-control">
              <option value="">Cualquiera</option>
              <option value="1">Abierto</option>
              <option value="2">Cerrado</option>
            </select>
          </div>

          <div class="tecnm-form-group filter-actions">
            <AppButton type="submit" variant="primary" icon="fa-search">Buscar</AppButton>
            <AppButton type="button" variant="secondary" icon="fa-undo" @click="clearFilters">Limpiar</AppButton>
          </div>
        </form>
      </div>
    </section>

    <details class="tecnm-card legend-box">
      <summary>
        <i class="fa fa-question-circle" aria-hidden="true"></i>
        ¿Cerrar un periodo?
      </summary>
      <p><strong><i class="fa fa-lock" aria-hidden="true"></i> Cerrar periodo</strong> — finaliza de forma normal un corte Abierto (también ocurre automáticamente al vencer). Sus permisos <em>pendientes</em> pasan a Rechazado sin goce, con fecha y responsable del cierre. Los permisos ya revisados no se tocan y todo queda visible como historial de nómina. Solo el recurso humano y el administrador pueden cerrar un periodo.</p>
    </details>

    <AppSpinner v-if="loading" />
    <template v-else>
      <section class="tecnm-card">
        <div class="tecnm-card-header tecnm-card-header--premium">
          <h4 class="filter-title">Listado de Cortes Quincenales</h4>
          <span class="count-badge">{{ totalItems }} registro(s)</span>
        </div>

        <div class="tecnm-table-responsive">
          <table class="tecnm-table tecnm-table-striped">
            <thead>
              <tr>
                <th class="text-center">ID</th>
                <th class="text-center">Inicia</th>
                <th class="text-center">Termina</th>
                <th class="text-center">Creación</th>
                <th class="text-center">Modificación</th>
                <th class="text-center">Estatus</th>
                <th class="text-center">Permisos</th>
                <th class="text-center">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="c in cutoffs" :key="c.id">
                <td class="text-center"><strong>#{{ c.id }}</strong></td>
                <td class="text-center"><strong>{{ formatTecNMDate(c.startDate) }}</strong></td>
                <td class="text-center"><strong>{{ formatTecNMDate(c.endDate) }}</strong></td>
                <td class="text-center">
                  <div class="datetime-cell">
                    <span class="datetime-date">{{ formatDateCell(c.createdAt) }}</span>
                    <span class="datetime-time">{{ formatTimeCell(c.createdAt) }}</span>
                  </div>
                </td>
                <td class="text-center">
                  <div v-if="c.updatedAt" class="datetime-cell">
                    <span class="datetime-date">{{ formatDateCell(c.updatedAt) }}</span>
                    <span class="datetime-time">{{ formatTimeCell(c.updatedAt) }}</span>
                  </div>
                  <span v-else class="text-muted small">Sin modificar</span>
                </td>
                <td class="text-center">
                  <StatusBadge :variant="badgeVariant(c.cutoffStatusTypeId)">
                    {{ c.statusDescription }}
                  </StatusBadge>
                </td>
                <td class="text-center">
                  <span class="badge-count"><i class="fa fa-file-text-o" aria-hidden="true"></i> {{ c.permissionsCount }}</span>
                </td>
                <td class="text-center">
                  <div class="tecnm-table-actions">
                    <button
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--view"
                      title="Ver Dashboard Analítico"
                      @click="$router.push({ name: 'cutoffs.details', params: { id: c.id } })"
                    >
                      <i class="fa fa-bar-chart" aria-hidden="true"></i>
                    </button>

                    <button
                      v-if="c.cutoffStatusTypeId === CUTOFF_STATUS.OPEN && authStore.canManageUsers"
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--edit"
                      title="Editar Periodo"
                      @click="$router.push({ name: 'cutoffs.edit', params: { id: c.id } })"
                    >
                      <i class="fa fa-pencil" aria-hidden="true"></i>
                    </button>

                    <button
                      v-if="c.cutoffStatusTypeId === CUTOFF_STATUS.OPEN && authStore.canManageUsers"
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--approve"
                      title="Cerrar Periodo Quincenal"
                      @click="confirmClose(c)"
                    >
                      <i class="fa fa-lock" aria-hidden="true"></i>
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="!cutoffs.length">
                <td colspan="8"><EmptyState message="No hay cortes de nómina registrados"><span>Intenta ajustar los filtros de búsqueda</span></EmptyState></td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="cutoffs.length" class="tecnm-card-footer">
          <div class="pagination-wrap">
            <span class="pagination-info">
              <i class="fa fa-list-ol" aria-hidden="true"></i>
              Mostrando <strong>{{ firstItemOnPage }}</strong> - <strong>{{ lastItemOnPage }}</strong> de
              <strong>{{ totalItems }}</strong> cortes
            </span>
            <PaginationBar
              :page="page"
              :total-pages="totalPages"
              @update:page="(v: number) => { page = v; fetchCutoffs() }"
            />
          </div>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { http } from '@/api/client'
import AppPageHeader from '@/components/AppPageHeader.vue'
import AppButton from '@/components/AppButton.vue'
import AppSpinner from '@/components/AppSpinner.vue'
import EmptyState from '@/components/EmptyState.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import PaginationBar from '@/components/PaginationBar.vue'
import { formatTecNMDate } from '@/composables/useDates'
import { askConfirmation, showToast } from '@/composables/useAlerts'
import { useAuthStore } from '@/stores/auth'

interface CutoffItem {
  id: number
  startDate: string
  endDate: string
  createdAt?: string
  updatedAt?: string
  cutoffStatusTypeId: number
  statusDescription: string
  statusColor: string
  createdBy: string
  permissionsCount: number
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

const authStore = useAuthStore()

const cutoffs = ref<CutoffItem[]>([])
const loading = ref(true)
const page = ref(1)
const totalPages = ref(1)
const totalItems = ref(0)

const CUTOFF_STATUS = {
  OPEN: 1,
  CLOSED: 2
} as const

const filters = reactive({
  dateFrom: '',
  dateTo: '',
  statusId: ''
})

function badgeVariant(statusId: number): 'pending' | 'approved' | 'rejected' | 'info' {
  switch (statusId) {
    case CUTOFF_STATUS.OPEN: return 'approved'  // Abierto / Activo (verde)
    case CUTOFF_STATUS.CLOSED: return 'rejected'  // Cerrado (rojo)
    default: return 'info'
  }
}

const firstItemOnPage = computed(() => (totalItems.value === 0 ? 0 : (page.value - 1) * 10 + 1))
const lastItemOnPage = computed(() => Math.min(page.value * 10, totalItems.value))

async function fetchCutoffs() {
  loading.value = true
  try {
    const params = new URLSearchParams()
    params.append('page', page.value.toString())
    params.append('pageSize', '10')

    if (filters.dateFrom) params.append('dateFrom', filters.dateFrom)
    if (filters.dateTo) params.append('dateTo', filters.dateTo)
    if (filters.statusId) params.append('statusId', filters.statusId)

    const res = await http.get(`/api/Cutoffs?${params.toString()}`)
    const data = res.data

    if (res.status === 200) {
      cutoffs.value = data.items || []
      totalPages.value = data.totalPages || 1
      totalItems.value = data.totalItems || 0
    }
  } catch (err) {
    showToast('Error cargando los cortes de nómina.', 'error')
  } finally {
    loading.value = false
  }
}

function applyFilters() {
  page.value = 1
  fetchCutoffs()
}

function clearFilters() {
  filters.dateFrom = ''
  filters.dateTo = ''
  filters.statusId = ''
  applyFilters()
}

async function confirmClose(cutoff: CutoffItem) {
  const ok = await askConfirmation({
    title: '¿Cerrar Periodo de Nómina?',
    text: `¿Estás seguro de finalizar el periodo #${cutoff.id}? Los permisos pendientes serán procesados/rechazados automáticamente.`,
    confirmText: 'Sí, cerrar periodo'
  })

  if (ok) {
    try {
      const res = await http.put(`/api/Cutoffs/${cutoff.id}/close`)
      showToast(res.data.message || 'Periodo cerrado correctamente.', 'success')
      fetchCutoffs()
    } catch (e: any) {
      showToast(e.response?.data?.message || 'No se pudo cerrar el periodo.', 'error')
    }
  }
}

onMounted(() => {
  fetchCutoffs()
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

.count-badge {
  font-size: 0.82rem;
  font-weight: 600;
  color: var(--tecnm-accent-text);
  background: var(--tecnm-soft-hover);
  padding: 0.2rem 0.6rem;
  border-radius: 9999px;
}

.badge-count {
  font-size: 0.85rem;
  color: var(--tecnm-navy);
  font-weight: 600;
  background: var(--tecnm-bg-light);
  padding: 0.2rem 0.6rem;
  border-radius: 4px;
}

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
</style>
