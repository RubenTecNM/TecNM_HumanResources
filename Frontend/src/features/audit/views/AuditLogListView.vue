<template>
  <div>
    <AppPageHeader title="Bitácora de Auditoría" subtitle="Monitoreo de operaciones en el sistema" icon="fa-history" />

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
            <label class="tecnm-label" for="dateFrom">Fecha Desde</label>
            <input id="dateFrom" v-model="filters.dateFrom" type="date" class="tecnm-form-control" />
          </div>

          <div class="tecnm-form-group">
            <label class="tecnm-label" for="dateTo">Fecha Hasta</label>
            <input id="dateTo" v-model="filters.dateTo" type="date" class="tecnm-form-control" />
          </div>

          <div class="tecnm-form-group">
            <label class="tecnm-label" for="tableFilter">Tabla / Entidad</label>
            <select id="tableFilter" v-model="filters.tableId" class="tecnm-form-control">
              <option value="">Todas las tablas</option>
              <option v-for="t in tables" :key="t.id" :value="t.id">{{ t.description }}</option>
            </select>
          </div>

          <div class="tecnm-form-group filter-actions">
            <AppButton type="submit" variant="primary" icon="fa-search">Buscar</AppButton>
            <AppButton type="button" variant="secondary" icon="fa-undo" @click="clearFilters">Limpiar</AppButton>
          </div>
        </form>
      </div>
    </section>

    <!-- Leyenda de Origen de la operación -->
    <section class="tecnm-card tecnm-mb-4 origin-legend">
      <div class="tecnm-card-body">
        <h4 class="legend-title"><i class="fa fa-info-circle" aria-hidden="true"></i> ¿Qué significa cada origen?</h4>
        <ul class="legend-list">
          <li><span class="legend-swatch" style="background: var(--tecnm-success, #2e7d32)"></span><strong>Interfaz Web</strong> — operación realizada por un usuario desde el sistema (login, edición, autorización, etc.).</li>
          <li><span class="legend-swatch" style="background: var(--tecnm-accent, #1565c0)"></span><strong>API Externa</strong> — operación entrante vía invocación con API Key (<code>X-API-KEY</code>).</li>
          <li><span class="legend-swatch" style="background: var(--tecnm-warning, #9e9e9e)"></span><strong>Sistema Automático</strong> — proceso del sistema (ej. cierre de corte o tarea programada), no lo ejecutó un usuario.</li>
        </ul>
      </div>
    </section>

    <AppSpinner v-if="loading" />
    <template v-else>
      <section class="tecnm-card">
        <div class="tecnm-card-header tecnm-card-header--premium">
          <h4 class="filter-title">Eventos de Auditoría</h4>
          <span class="count-badge">{{ logs.length }} registro(s)</span>
        </div>

        <div class="tecnm-table-responsive">
          <table class="tecnm-table tecnm-table-striped">
            <thead>
              <tr>
                <th class="text-center">ID</th>
                <th class="text-center">Fecha / Hora</th>
                <th class="text-center">Tabla Afectada</th>
                <th class="text-center">Acción</th>
                <th class="text-center">Usuario</th>
                <th class="text-center">Origen</th>
                <th class="text-center">Inspeccionar</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="l in logs" :key="l.id">
                <td class="text-center"><strong>#{{ l.id }}</strong></td>
                <td class="text-center">
                  <div class="datetime-cell">
                    <span class="datetime-date">{{ formatTecNMDate(l.createdAt) }}</span>
                  </div>
                </td>
                <td class="text-center">
                  <span class="entity-tag">{{ l.tableName }}</span>
                </td>
                <td class="text-center">
                  <StatusBadge :variant="actionBadgeVariant(l.action)">
                    {{ l.action }}
                  </StatusBadge>
                </td>
                <td class="text-center"><strong>{{ l.userName }}</strong></td>
                <td class="text-center">
                  <span class="origin-tag">{{ l.origin }}</span>
                </td>
                <td class="text-center">
                  <div class="tecnm-table-actions">
                    <button
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--view"
                      title="Ver estado previo (JSON)"
                      @click="inspectLog(l.id)"
                    >
                      <i class="fa fa-eye" aria-hidden="true"></i>
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="!logs.length">
                <td colspan="7"><EmptyState message="No hay eventos de auditoría registrados"><span>Intenta ajustar los filtros de búsqueda</span></EmptyState></td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="logs.length" class="tecnm-card-footer">
          <div class="pagination-wrap">
            <span class="pagination-info">
              <i class="fa fa-list-ol" aria-hidden="true"></i>
              Mostrando página <strong>{{ page }}</strong> de <strong>{{ totalPages }}</strong>
            </span>
            <PaginationBar
              :page="page"
              :total-pages="totalPages"
              @update:page="(v: number) => { page = v; fetchLogs() }"
            />
          </div>
        </div>
      </section>
    </template>

    <!-- Modal de Inspección de Auditoría JSON Reactivo -->
    <AppModal
      v-model:show="showInspectModal"
      :title="selectedLog ? `Auditoría #${selectedLog.id} — ${selectedLog.tableName}` : 'Detalle de Auditoría'"
      icon="fa-search"
      size="lg"
      :show-cancel="false"
      confirm-text="Cerrar"
      @confirm="showInspectModal = false"
    >
      <div v-if="selectedLog" class="audit-details-content">
        <div class="audit-info-grid mb-3">
          <div><strong>Usuario:</strong> {{ selectedLog.userName }}</div>
          <div><strong>Acción:</strong> {{ selectedLog.action }}</div>
          <div><strong>Origen:</strong> {{ selectedLog.origin }}</div>
          <div><strong>Fecha:</strong> {{ formatTecNMDate(selectedLog.createdAt) }}</div>
        </div>

        <div class="json-box">
          <span class="json-box-title">Estado Previo (JSON):</span>
          <pre class="json-code"><code>{{ formattedJson }}</code></pre>
        </div>
      </div>
    </AppModal>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { http } from "@/shared/api/client"
import AppPageHeader from '@/shared/components/AppPageHeader.vue'
import AppButton from '@/shared/components/AppButton.vue'
import AppSpinner from '@/shared/components/AppSpinner.vue'
import EmptyState from '@/shared/components/EmptyState.vue'
import StatusBadge from '@/shared/components/StatusBadge.vue'
import PaginationBar from '@/shared/components/PaginationBar.vue'
import AppModal from '@/shared/components/AppModal.vue'
import { formatTecNMDate } from '@/shared/composables/useDates'
import { showToast } from '@/shared/composables/useAlerts'

interface AuditItem {
  id: number
  tableName: string
  action: string
  userName: string
  origin: string
  createdAt: string
  previousData?: string
}

const logs = ref<AuditItem[]>([])
const tables = ref<any[]>([])
const loading = ref(true)
const page = ref(1)
const totalPages = ref(1)

// Modal State
const showInspectModal = ref(false)
const selectedLog = ref<AuditItem | null>(null)

const formattedJson = computed(() => {
  if (!selectedLog.value || !selectedLog.value.previousData) return 'Sin datos previos registrados.'
  try {
    return JSON.stringify(JSON.parse(selectedLog.value.previousData), null, 2)
  } catch {
    return selectedLog.value.previousData
  }
})

const filters = reactive({
  dateFrom: '',
  dateTo: '',
  tableId: ''
})

function actionBadgeVariant(action: string) {
  if (!action) return 'pending'
  const a = action.toLowerCase()
  if (a.includes('crea') || a.includes('post') || a.includes('insert')) return 'approved'
  if (a.includes('edita') || a.includes('put') || a.includes('update')) return 'pending'
  if (a.includes('elimina') || a.includes('delete')) return 'rejected'
  return 'info'
}

async function fetchTables() {
  try {
    const res = await http.get('/api/DatabaseTablesApi')
    if (res.status === 200) tables.value = res.data
  } catch (e) {
    console.error(e)
  }
}

async function fetchLogs() {
  loading.value = true
  try {
    const params = new URLSearchParams()
    params.append('page', page.value.toString())
    params.append('pageSize', '10')

    if (filters.dateFrom) params.append('dateFrom', filters.dateFrom)
    if (filters.dateTo) params.append('dateTo', filters.dateTo)
    if (filters.tableId) params.append('tableId', filters.tableId)

    const res = await http.get(`/api/AuditLogsApi?${params.toString()}`)
    const data = res.data

    if (res.status === 200) {
      logs.value = data.items || []
      totalPages.value = data.totalPages || 1
    }
  } catch (err) {
    showToast('Error cargando bitácora de auditoría.', 'error')
  } finally {
    loading.value = false
  }
}

function applyFilters() {
  page.value = 1
  fetchLogs()
}

function clearFilters() {
  filters.dateFrom = ''
  filters.dateTo = ''
  filters.tableId = ''
  applyFilters()
}

async function inspectLog(id: number) {
  try {
    const res = await http.get(`/api/AuditLogsApi/${id}`)
    selectedLog.value = res.data
    showInspectModal.value = true
  } catch (e) {
    showToast('No se pudo obtener el detalle de auditoría.', 'error')
  }
}

onMounted(() => {
  fetchTables()
  fetchLogs()
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

.entity-tag {
  font-weight: 600;
  font-size: 0.85rem;
  color: var(--tecnm-navy);
}

.origin-tag {
  font-size: 0.8rem;
  background: var(--tecnm-bg-light);
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
  border: 1px solid var(--tecnm-border);
}

.origin-legend .tecnm-card-body {
  padding-top: 0.85rem;
  padding-bottom: 0.85rem;
}

.legend-title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin: 0 0 0.6rem;
  font-size: 0.9rem;
}

.legend-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  font-size: 0.85rem;
}

.legend-list li {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--tecnm-text-secondary);
}

.legend-list code {
  background: var(--tecnm-bg-light);
  padding: 0.1rem 0.35rem;
  border-radius: 4px;
}

.legend-swatch {
  flex: 0 0 auto;
  width: 0.7rem;
  height: 0.7rem;
  border-radius: 50%;
  display: inline-block;
}

.audit-info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 0.75rem;
  font-size: 0.9rem;
  background: var(--tecnm-soft-hover);
  padding: 0.75rem 1rem;
  border-radius: 6px;
  border: 1px solid var(--tecnm-border-color);
}

.json-box-title {
  display: block;
  font-weight: 600;
  font-size: 0.85rem;
  margin-bottom: 0.5rem;
}

.json-code {
  background: #0f172a;
  color: #f8fafc;
  padding: 1rem;
  border-radius: 8px;
  max-height: 300px;
  overflow-y: auto;
  font-size: 0.85rem;
  margin: 0;
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

.text-center {
  text-align: center;
}

@media (max-width: 640px) {
  .filter-grid {
    grid-template-columns: 1fr;
  }
}
</style>
