<template>
  <div>
    <AppPageHeader
      :title="`Corte Quincenal #${cutoffId}`"
      :subtitle="cutoff ? `${formatTecNMDate(cutoff.startDate)} – ${formatTecNMDate(cutoff.endDate)}` : 'Dashboard Analítico'"
      icon="fa-calendar"
    >
      <template #actions>
        <StatusBadge v-if="cutoff" :variant="cutoffStatusVariant(cutoff.cutoffStatusTypeId)" class="align-self-center">
          {{ cutoff.statusDescription }}
        </StatusBadge>
        <div class="dropdown" v-if="cutoff">
          <button
            class="btn btn-success dropdown-toggle d-flex align-items-center gap-2 btn-sm rounded-pill px-3"
            type="button"
            data-bs-toggle="dropdown"
            aria-expanded="false"
          >
            <i class="fa fa-download" aria-hidden="true"></i> Exportar
          </button>
          <ul class="dropdown-menu dropdown-menu-end shadow border-0 p-2">
            <li>
              <a class="dropdown-item d-flex align-items-center gap-2 py-2 rounded" href="#" @click.prevent="exportCutoff('excel')">
                <i class="fa fa-file-excel-o text-success" aria-hidden="true"></i> Excel (.xlsx)
              </a>
            </li>
            <li>
              <a class="dropdown-item d-flex align-items-center gap-2 py-2 rounded" href="#" @click.prevent="exportCutoff('csv')">
                <i class="fa fa-file-text-o text-info" aria-hidden="true"></i> CSV (.csv)
              </a>
            </li>
            <li>
              <a class="dropdown-item d-flex align-items-center gap-2 py-2 rounded" href="#" @click.prevent="exportCutoff('json')">
                <i class="fa fa-file-code-o text-warning" aria-hidden="true"></i> JSON (.json)
              </a>
            </li>
            <li>
              <a class="dropdown-item d-flex align-items-center gap-2 py-2 rounded" href="#" @click.prevent="exportCutoff('xml')">
                <i class="fa fa-file-code-o text-danger" aria-hidden="true"></i> XML (.xml)
              </a>
            </li>
            <li><hr class="dropdown-divider"></li>
            <li>
              <a class="dropdown-item d-flex align-items-center gap-2 py-2 rounded" href="#" @click.prevent="printPage">
                <i class="fa fa-print text-secondary" aria-hidden="true"></i> Imprimir / PDF
              </a>
            </li>
          </ul>
        </div>
        <AppButton variant="secondary" icon="fa-arrow-left" size="sm" @click="$router.push({ name: 'cutoffs.index' })">
          Volver
        </AppButton>
      </template>
    </AppPageHeader>

    <div v-if="exportError" class="alert alert-danger">
      <i class="fas fa-exclamation-triangle me-2"></i> {{ exportError }}
    </div>

    <AppSpinner v-if="loading" />

    <div v-else-if="error" class="alert alert-danger">
      <i class="fas fa-exclamation-triangle me-2"></i> {{ error }}
    </div>

    <template v-else-if="cutoff">
      <!-- KPI Cards -->
      <div class="row g-3 mb-4">
        <div class="col-12 col-sm-6 col-md-3">
          <AppCard>
            <div class="kpi-card">
              <div class="kpi-icon kpi-icon--blue">
                <i class="fa fa-file-text" aria-hidden="true"></i>
              </div>
              <div class="kpi-content">
                <span class="kpi-label">Total Permisos</span>
                <h3 class="kpi-value">{{ kpis.totalPermissions }}</h3>
                <span class="kpi-sub">solicitudes en periodo</span>
              </div>
            </div>
          </AppCard>
        </div>
        <div class="col-12 col-sm-6 col-md-3">
          <AppCard>
            <div class="kpi-card">
              <div class="kpi-icon kpi-icon--success">
                <i class="fa fa-money" aria-hidden="true"></i>
              </div>
              <div class="kpi-content">
                <span class="kpi-label">Con Goce de Sueldo</span>
                <h3 class="kpi-value text-success">{{ kpis.withPay }}</h3>
                <span class="kpi-sub">autorizados con goce</span>
              </div>
            </div>
          </AppCard>
        </div>
        <div class="col-12 col-sm-6 col-md-3">
          <AppCard>
            <div class="kpi-card">
              <div class="kpi-icon kpi-icon--danger">
                <i class="fa fa-times-circle" aria-hidden="true"></i>
              </div>
              <div class="kpi-content">
                <span class="kpi-label">Sin Goce de Sueldo</span>
                <h3 class="kpi-value text-danger">{{ kpis.withoutPay }}</h3>
                <span class="kpi-sub">no gozaron sueldo</span>
              </div>
            </div>
          </AppCard>
        </div>
        <div class="col-12 col-sm-6 col-md-3">
          <AppCard>
            <div class="kpi-card">
              <div class="kpi-icon kpi-icon--info">
                <i class="fa fa-clock-o" aria-hidden="true"></i>
              </div>
              <div class="kpi-content">
                <span class="kpi-label">Promedio de Días</span>
                <h3 class="kpi-value text-info">{{ kpis.averageDays }}d</h3>
                <span class="kpi-sub">duración media</span>
              </div>
            </div>
          </AppCard>
        </div>
      </div>

      <!-- Tendencia Temporal -->
      <AppCard class="mb-4">
        <template #header>
          <div class="d-flex justify-content-between align-items-center">
            <h4 class="filter-title">
              <i class="fa fa-line-chart me-2" aria-hidden="true"></i>
              Tendencia de Solicitudes en el Periodo
            </h4>
            <div class="chart-switcher">
              <button
                v-for="type in (['area', 'bar', 'line'] as const)"
                :key="type"
                :class="['chart-switcher-btn', { active: trendChartType === type }]"
                @click="trendChartType = type"
              >
                {{ type === 'area' ? 'Área' : type === 'bar' ? 'Barras' : 'Línea' }}
              </button>
            </div>
          </div>
        </template>
        <div v-if="trendData.length === 0" class="text-center py-5 text-muted">
          Sin suficientes datos de tendencia en el periodo de nómina.
        </div>
        <apexchart v-else :type="trendChartType" height="280" :options="trendChartOptions" :series="trendChartSeries"></apexchart>
      </AppCard>

      <!-- Distribución por Tipo y Proporción de Pago -->
      <div class="row g-4 mb-4">
        <div class="col-12 col-md-6">
          <AppCard class="h-100">
            <template #header>
              <h4 class="filter-title">
                <i class="fa fa-pie-chart me-2" aria-hidden="true"></i>
                Distribución por Tipo de Permiso
              </h4>
            </template>
            <div class="chart-container">
              <div v-if="permissionsByType.length === 0" class="text-center py-5 text-muted">
                Sin datos de tipo registrados.
              </div>
              <apexchart v-else type="donut" height="260" width="100%" :options="donutChartOptions" :series="donutChartSeries"></apexchart>
            </div>
          </AppCard>
        </div>
        <div class="col-12 col-md-6">
          <AppCard class="h-100">
            <template #header>
              <h4 class="filter-title">
                <i class="fa fa-adjust me-2" aria-hidden="true"></i>
                Proporción de Pago
              </h4>
            </template>
            <div class="chart-container">
              <div v-if="kpis.totalPermissions === 0" class="text-center py-5 text-muted">
                Sin datos de pago registrados.
              </div>
              <apexchart v-else type="radialBar" height="260" width="100%" :options="radialChartOptions" :series="radialChartSeries"></apexchart>
            </div>
          </AppCard>
        </div>
      </div>

      <!-- Permisos por Área -->
      <AppCard class="mb-4">
        <template #header>
          <h4 class="filter-title">
            <i class="fa fa-sitemap me-2" aria-hidden="true"></i>
            Solicitudes Autorizadas por Área / Departamento
          </h4>
        </template>
        <div v-if="permissionsByArea.length === 0" class="text-center py-5 text-muted">
          Sin datos de áreas registrados en este corte.
        </div>
        <apexchart v-else type="bar" height="280" :options="barChartOptions" :series="barChartSeries"></apexchart>
      </AppCard>

      <!-- Heatmap de Solicitudes -->
      <AppCard class="mb-4">
        <template #header>
          <h4 class="filter-title">
            <i class="fa fa-calendar-check-o me-2 text-primary" aria-hidden="true"></i>
            Heatmap de Solicitudes por Día de la Semana
          </h4>
        </template>
        <div v-if="heatmapRows.length === 0" class="text-center py-5 text-muted">
          Sin solicitudes para generar el mapa de calor.
        </div>
        <div v-else class="table-responsive">
          <table class="table table-sm align-middle heatmap-table">
            <thead>
              <tr>
                <th class="heatmap-day-col">Día</th>
                <th v-for="w in maxWeek" :key="w" class="text-center">Semana {{ w }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in heatmapRows" :key="row.day">
                <td class="heatmap-day-col fw-semibold">{{ row.day }}</td>
                <td
                  v-for="w in maxWeek"
                  :key="w"
                  class="text-center heatmap-cell"
                  :class="`heatmap-cell--lvl${cellLevel(row.day, w)}`"
                >
                  {{ cellCount(row.day, w) }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </AppCard>

      <!-- Auditoría de Registro -->
      <AppCard>
        <template #header>
          <h4 class="filter-title">
            <i class="fa fa-info-circle me-2 text-primary" aria-hidden="true"></i>
            Auditoría de Registro del Periodo
          </h4>
        </template>
        <div class="row g-3">
          <div class="col-12 col-md-6">
            <div class="audit-box">
              <div class="d-flex align-items-center gap-3">
                <div class="audit-icon audit-icon--success">
                  <i class="fa fa-user-plus" aria-hidden="true"></i>
                </div>
                <div>
                  <span class="audit-label">Generado por</span>
                  <p class="audit-value mb-0">{{ cutoff.createdBy || 'Sistema' }}</p>
                </div>
              </div>
            </div>
          </div>
          <div class="col-12 col-md-6">
            <div class="audit-box">
              <div class="d-flex align-items-center gap-3">
                <div class="audit-icon audit-icon--warning">
                  <i class="fa fa-pencil-square-o" aria-hidden="true"></i>
                </div>
                <div>
                  <span class="audit-label">Última Modificación por</span>
                  <p class="audit-value mb-0">{{ cutoff.updatedBy || 'Sin modificaciones' }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </AppCard>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import AppPageHeader from '@/components/AppPageHeader.vue'
import AppButton from '@/components/AppButton.vue'
import AppCard from '@/components/AppCard.vue'
import AppSpinner from '@/components/AppSpinner.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import apexchart from 'vue3-apexcharts'
import { fetchCutoffAnalytics, downloadCutoffExport, cutoffStatusVariant, type CutoffDetail, type CutoffKpis } from '@/api/cutoffs'
import { formatTecNMDate } from '@/composables/useDates'

const route = useRoute()
const cutoffId = computed(() => Number(route.params.id))

const loading = ref(true)
const error = ref('')

const cutoff = ref<CutoffDetail | null>(null)
const kpis = ref<CutoffKpis>({
  totalPermissions: 0,
  withPay: 0,
  withoutPay: 0,
  averageDays: 0,
  pendingCount: 0,
  approvedCount: 0
})

const permissionsByArea = ref<{ name: string; value: number }[]>([])
const permissionsByType = ref<{ name: string; value: number; color?: string }[]>([])
const trendData = ref<{ date: string; total: number; approved: number; rejected: number }[]>([])
const heatmap = ref<{ day: string; week: number; count: number }[]>([])

const DAY_ORDER = ['Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado', 'Domingo']

const heatmapRows = computed(() => DAY_ORDER.map(day => ({ day })))

const maxWeek = computed(() => {
  const weeks = heatmap.value.map(h => h.week)
  return weeks.length > 0 ? Math.max(...weeks) : 0
})

function cellCount(day: string, week: number): number {
  return heatmap.value.find(h => h.day === day && h.week === week)?.count ?? 0
}

function cellLevel(day: string, week: number): number {
  const count = cellCount(day, week)
  if (count === 0) return 0
  if (count <= 2) return 1
  if (count <= 4) return 2
  if (count <= 6) return 3
  return 4
}

const trendChartType = ref<'area' | 'bar' | 'line'>('area')

const chartFontFamily = 'Montserrat, Helvetica, Arial, sans-serif'

const trendChartOptions = computed(() => ({
  chart: {
    id: 'trend-chart',
    toolbar: { show: false },
    fontFamily: chartFontFamily
  },
  xaxis: {
    categories: trendData.value.map(t => t.date),
    labels: { style: { colors: '#475569', fontSize: '11px', fontWeight: 600 } }
  },
  yaxis: {
    labels: { style: { colors: '#475569', fontSize: '11px', fontWeight: 600 } }
  },
  colors: ['#1e3a8a', '#10b981', '#ef4444'],
  dataLabels: { enabled: false },
  stroke: { curve: 'smooth' as const, width: 2.5 },
  grid: { borderColor: '#e2e8f0', strokeDashArray: 4 },
  legend: {
    fontFamily: chartFontFamily,
    labels: { colors: '#1e293b' },
    fontSize: '12px'
  }
}))

const trendChartSeries = computed(() => [
  { name: 'Total Solicitudes', data: trendData.value.map(t => t.total) },
  { name: 'Aprobados', data: trendData.value.map(t => t.approved) },
  { name: 'Rechazados', data: trendData.value.map(t => t.rejected) }
])

const donutChartOptions = computed(() => ({
  chart: { fontFamily: chartFontFamily },
  labels: permissionsByType.value.map(t => t.name),
  colors: permissionsByType.value.map(t => t.color || '#3b82f6'),
  legend: {
    position: 'bottom' as const,
    fontFamily: chartFontFamily,
    labels: { colors: '#1e293b' },
    fontSize: '11px'
  },
  dataLabels: {
    enabled: true,
    style: { fontFamily: chartFontFamily, fontSize: '11px', fontWeight: 700 }
  }
}))

const donutChartSeries = computed(() => permissionsByType.value.map(t => t.value))

const barChartOptions = computed(() => ({
  chart: { fontFamily: chartFontFamily, toolbar: { show: false } },
  plotOptions: {
    bar: { horizontal: true, barHeight: '50%', borderRadius: 4 }
  },
  colors: ['#1e3a8a'],
  xaxis: {
    categories: permissionsByArea.value.map(a => a.name),
    labels: { style: { colors: '#475569', fontSize: '11px', fontWeight: 600 } }
  },
  yaxis: {
    labels: { style: { colors: '#1e293b', fontSize: '11px', fontWeight: 600 } }
  },
  grid: { borderColor: '#e2e8f0' },
  dataLabels: { enabled: true, style: { fontSize: '11px', fontWeight: 700 } }
}))

const barChartSeries = computed(() => [
  { name: 'Solicitudes', data: permissionsByArea.value.map(a => a.value) }
])

const radialChartOptions = computed(() => ({
  chart: { fontFamily: chartFontFamily },
  plotOptions: {
    radialBar: {
      hollow: { size: '55%' },
      dataLabels: {
        name: { show: true, fontSize: '12px', color: '#475569', fontFamily: chartFontFamily, fontWeight: 600 },
        value: {
          show: true, fontSize: '18px', color: '#0f172a', fontFamily: chartFontFamily, fontWeight: 800,
          formatter: (val: number) => `${val}%`
        }
      }
    }
  },
  labels: ['Con Goce', 'Sin Goce'],
  colors: ['#10b981', '#ef4444'],
  legend: { show: true, position: 'bottom' as const, fontFamily: chartFontFamily, fontSize: '11px' }
}))

const radialChartSeries = computed(() => {
  const total = kpis.value.totalPermissions
  if (total === 0) return [0, 0]
  const withPayPct = Math.round((kpis.value.withPay / total) * 100)
  const withoutPayPct = Math.round((kpis.value.withoutPay / total) * 100)
  return [withPayPct, withoutPayPct]
})

async function loadAnalytics() {
  loading.value = true
  error.value = ''
  try {
    const data = await fetchCutoffAnalytics(cutoffId.value)
    cutoff.value = data.cutoff
    kpis.value = data.kpis
    permissionsByArea.value = data.permissionsByArea || []
    permissionsByType.value = data.permissionsByType || []
    trendData.value = data.trend || []
    heatmap.value = data.heatmap || []
  } catch (e: any) {
    error.value = e.response?.data?.message || 'No se pudieron obtener las analíticas del periodo.'
  } finally {
    loading.value = false
  }
}

function printPage() {
  window.print()
}

const exporting = ref(false)
const exportError = ref('')

async function exportCutoff(format: 'excel' | 'csv' | 'json' | 'xml') {
  exporting.value = true
  exportError.value = ''
  try {
    await downloadCutoffExport(cutoffId.value, format)
  } catch (e: any) {
    exportError.value = e.response?.data?.message || 'No se pudo descargar el reporte del periodo.'
  } finally {
    exporting.value = false
  }
}

onMounted(() => {
  loadAnalytics()
})
</script>

<style scoped>
.kpi-card {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.kpi-icon {
  width: 44px;
  height: 44px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
  flex-shrink: 0;
}

.kpi-icon--blue { background-color: #eff6ff; color: #1d4ed8; }
.kpi-icon--success { background-color: #ecfdf5; color: #10b981; }
.kpi-icon--danger { background-color: #fef2f2; color: #ef4444; }
.kpi-icon--info { background-color: #ecfeff; color: #0891b2; }

.kpi-content {
  display: flex;
  flex-direction: column;
}

.kpi-label {
  font-size: 0.72rem;
  font-weight: 700;
  color: var(--tecnm-text-secondary, #475569);
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.kpi-value {
  font-size: 1.6rem;
  font-weight: 800;
  color: var(--tecnm-text-primary, #0f172a);
  margin: 0.15rem 0;
  line-height: 1.1;
}

.kpi-sub {
  font-size: 0.68rem;
  color: var(--tecnm-text-secondary, #64748b);
}

.chart-switcher {
  display: flex;
  background: var(--tecnm-bg-light, #e2e8f0);
  padding: 0.2rem;
  border-radius: 6px;
}

.chart-switcher-btn {
  background: transparent;
  border: none;
  font-size: 0.72rem;
  font-weight: 700;
  color: var(--tecnm-text-secondary, #475569);
  padding: 0.25rem 0.75rem;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.15s;
}

.chart-switcher-btn.active {
  background: #ffffff;
  color: var(--tecnm-text-primary, #0f172a);
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.chart-container {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 290px;
}

.heatmap-table {
  font-size: 0.8rem;
}

.heatmap-table th,
.heatmap-table td {
  border: 1px solid var(--tecnm-border-color, #e2e8f0);
}

.heatmap-day-col {
  min-width: 110px;
  background: var(--tecnm-bg-main, #f8fafc);
}

.heatmap-cell {
  min-width: 70px;
  transition: transform 0.15s;
}

.heatmap-cell--lvl0 { background-color: #f1f5f9; color: #94a3b8; }
.heatmap-cell--lvl1 { background-color: #dbeafe; color: #1e40af; }
.heatmap-cell--lvl2 { background-color: #93c5fd; color: #1e3a8a; }
.heatmap-cell--lvl3 { background-color: #3b82f6; color: #ffffff; }
.heatmap-cell--lvl4 { background-color: #1d4ed8; color: #ffffff; font-weight: 700; }

.audit-box {
  background: var(--tecnm-bg-main, #f8fafc);
  border: 1px solid var(--tecnm-border-color, #e2e8f0);
  border-radius: var(--tecnm-radius-sm, 8px);
  padding: 1rem;
}

.audit-icon {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.1rem;
  flex-shrink: 0;
}

.audit-icon--success { background-color: #ecfdf5; color: #10b981; }
.audit-icon--warning { background-color: #fffbeb; color: #d97706; }

.audit-label {
  display: block;
  font-size: 0.72rem;
  font-weight: 700;
  color: var(--tecnm-text-secondary, #475569);
  text-transform: uppercase;
  letter-spacing: 0.04em;
  margin-bottom: 0.15rem;
}

.audit-value {
  font-size: 0.95rem;
  font-weight: 800;
  color: var(--tecnm-text-primary, #0f172a);
}

.filter-title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin: 0;
}
</style>
