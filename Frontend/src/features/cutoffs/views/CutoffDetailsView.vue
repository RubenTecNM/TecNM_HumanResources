<template>
  <div class="cutoff-details">
    <AppPageHeader
      :title="`Corte Quincenal #${cutoffId}`"
      :subtitle="cutoff ? `${formatTecNMDate(cutoff.startDate)} – ${formatTecNMDate(cutoff.endDate)}` : 'Dashboard Analítico'"
      icon="fa-calendar"
    >
      <template #actions>
        <AppButton variant="secondary" icon="fa-arrow-left" size="sm" @click="$router.push({ name: 'cutoffs.index' })">
          Volver
        </AppButton>

        <span class="status-slot" v-if="cutoff">
          <StatusBadge :variant="cutoffStatusVariant(cutoff.cutoffStatusTypeId)">
            {{ cutoff.statusDescription }}
          </StatusBadge>
        </span>

        <div class="export-menu">
          <button
            type="button"
            ref="exportToggleEl"
            class="export-toggle"
            :disabled="exporting"
            aria-haspopup="true"
            :aria-expanded="exportOpen"
            @click="openExportMenu"
          >
            <i class="fa fa-download" aria-hidden="true"></i>
            <span>{{ exporting ? 'Exportando…' : 'Exportar' }}</span>
            <i class="fa fa-chevron-down export-caret" :class="{ open: exportOpen }" aria-hidden="true"></i>
          </button>
          <ul v-if="exportOpen" class="export-menu__list" role="menu" :style="exportMenuStyle">
            <li v-for="opt in exportOptions" :key="opt.format" role="none">
              <button type="button" role="menuitem" @click="runExport(opt.format)">
                <i :class="opt.icon" aria-hidden="true"></i> {{ opt.label }}
              </button>
            </li>
            <li role="none"><hr class="export-menu__divider" /></li>
            <li role="none">
              <button type="button" role="menuitem" @click="printPage(); exportOpen = false">
                <i class="fa fa-print" aria-hidden="true"></i> Imprimir / PDF
              </button>
            </li>
          </ul>
        </div>
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
      <div class="kpi-grid">
<AppCard premium class="h-100">
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

        <AppCard premium class="h-100">
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

        <AppCard premium class="h-100">
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

        <AppCard premium class="h-100">
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

      <!-- Tendencia Temporal -->
      <AppCard premium>
        <template #header>
          <div class="trend-header">
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

      <!-- Distribución por Tipo y Proporción de Pago: 2 columnas en la misma fila -->
      <div class="charts-duo">
        <div>
          <AppCard premium class="h-100">
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
        <div>
          <AppCard premium class="h-100">
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
      <AppCard premium>
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

      <!-- Actividad Semanal y Top Solicitantes -->
      <div class="charts-duo">
        <div>
          <AppCard premium class="h-100">
            <template #header>
              <h4 class="filter-title">
                <i class="fa fa-calendar-check-o me-2" aria-hidden="true"></i>
                Solicitudes por Día de la Semana
              </h4>
            </template>
            <div v-if="!weekdaySeries || weekdaySeries.data.length === 0" class="chart-empty">Sin solicitudes en este periodo.</div>
            <apexchart v-else type="bar" height="280" :options="weekdayChartOptions" :series="weekdaySeries ? [weekdaySeries] : []"></apexchart>
          </AppCard>
        </div>
        <div>
          <AppCard premium class="h-100">
            <template #header>
              <h4 class="filter-title">
                <i class="fa fa-trophy me-2" aria-hidden="true"></i>
                Top Solicitantes del Periodo
              </h4>
            </template>
            <div v-if="topRequesters.length === 0" class="chart-empty">Sin solicitantes registrados.</div>
            <ol v-else class="top-list">
              <li v-for="(r, i) in topRequesters" :key="r.name" class="top-item">
                <span class="top-rank" :class="`top-rank--${i + 1}`">{{ i + 1 }}</span>
                <span class="top-info">
                  <span class="top-name">{{ r.name }}</span>
                  <span v-if="r.area" class="top-area">{{ r.area }}</span>
                </span>
                <span class="top-count">{{ r.value }}</span>
              </li>
            </ol>
          </AppCard>
        </div>
      </div>

      <!-- Auditoría de Registro -->
      <AppCard premium>
        <template #header>
          <h4 class="filter-title">
            <i class="fa fa-info-circle me-2 text-primary" aria-hidden="true"></i>
            Auditoría de Registro del Periodo
          </h4>
        </template>
        <div class="audit-grid">
          <div class="audit-box">
            <div class="audit-head">
              <div class="audit-icon audit-icon--success">
                <i class="fa fa-user-plus" aria-hidden="true"></i>
              </div>
              <div>
                <span class="audit-label">Generado por</span>
                <p class="audit-value">{{ cutoff.createdBy || 'Sistema' }}</p>
              </div>
            </div>
            <span v-if="createdAtLabel" class="audit-date">{{ createdAtLabel }}</span>
          </div>
          <div class="audit-box">
            <div class="audit-head">
              <div class="audit-icon audit-icon--warning">
                <i class="fa fa-pencil-square-o" aria-hidden="true"></i>
              </div>
              <div>
                <span class="audit-label">Última modificación</span>
                <p class="audit-value">{{ cutoff.updatedBy || 'Sin modificaciones' }}</p>
              </div>
            </div>
            <span v-if="updatedAtLabel" class="audit-date">{{ updatedAtLabel }}</span>
          </div>
        </div>
      </AppCard>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted , onUnmounted} from 'vue'
import { useRoute } from 'vue-router'
import AppPageHeader from '@/shared/components/AppPageHeader.vue'
import AppButton from '@/shared/components/AppButton.vue'
import AppCard from '@/shared/components/AppCard.vue'
import AppSpinner from '@/shared/components/AppSpinner.vue'
import StatusBadge from '@/shared/components/StatusBadge.vue'
import apexchart from 'vue3-apexcharts'
import { fetchCutoffAnalytics, downloadCutoffExport, cutoffStatusVariant, type CutoffDetail, type CutoffKpis } from "@/features/cutoffs/api/cutoffs"
import { formatTecNMDate } from '@/shared/composables/useDates'

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
const topRequesters = ref<{ name: string; area?: string; value: number }[]>([])

// Distribución por día de la semana derivada de la serie diaria (sin ida extra al API)
const weekdaySeries = computed(() => {
  if (!cutoff.value || trendFull.value.length === 0) return null
  const counts = [0, 0, 0, 0, 0, 0, 0]
  const start = new Date(cutoff.value.startDate)
  trendFull.value.forEach((t, i) => {
    const d = new Date(start)
    d.setDate(d.getDate() + i)
    counts[d.getDay()] += t.total
  })
  // Mostramos Lun → Dom
  return { name: 'Solicitudes', data: [1, 2, 3, 4, 5, 6, 0].map(idx => counts[idx]) }
})

const weekdayChartOptions = computed((): any => ({
  chart: { fontFamily: chartFontFamily, toolbar: { show: false } },
  xaxis: {
    categories: ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'],
    axisBorder: { color: gridColor() },
    axisTicks: { color: gridColor() },
    labels: { style: { colors: axisColor(), fontSize: '11px', fontWeight: 600 } }
  },
  yaxis: {
    min: 0,
    forceNiceScale: true,
    labels: { style: { colors: legendColor(), fontSize: '11px', fontWeight: 600 } }
  },
  colors: ['#38BDF8'],
  plotOptions: { bar: { borderRadius: 6, columnWidth: '55%' } },
  dataLabels: { enabled: false },
  grid: { borderColor: gridColor(), strokeDashArray: 4 },
  tooltip: { theme: isDark.value ? 'dark' : 'light' }
}))

// Fechas de auditoría legibles
const createdAtLabel = computed(() => cutoff.value?.createdAt ? formatTecNMDate(cutoff.value.createdAt) : '')
const updatedAtLabel = computed(() => cutoff.value?.updatedAt ? formatTecNMDate(cutoff.value.updatedAt) : '')

const trendChartType = ref<'area' | 'bar' | 'line'>('area')

const chartFontFamily = 'Montserrat, Helvetica, Arial, sans-serif'

// ── Menú de exportación (dropdown propio; no dependemos de Bootstrap) ──
const exportOpen = ref(false)
const exportToggleEl = ref<HTMLElement | null>(null)
const exportMenuStyle = ref<Record<string, string>>({})
const exportOptions = [
  { format: 'excel' as const, label: 'Excel (.xlsx)', icon: 'fa fa-file-excel-o export-icon--excel' },
  { format: 'csv' as const, label: 'CSV (.csv)', icon: 'fa fa-file-text-o export-icon--csv' },
  { format: 'json' as const, label: 'JSON (.json)', icon: 'fa fa-file-code-o export-icon--json' },
  { format: 'xml' as const, label: 'XML (.xml)', icon: 'fa fa-file-code-o export-icon--xml' }
]

function positionExportMenu() {
  const btn = exportToggleEl.value
  if (!btn) return
  const r = btn.getBoundingClientRect()
  exportMenuStyle.value = {
    position: 'fixed',
    top: `${r.bottom + 6}px`,
    right: `${Math.max(window.innerWidth - r.right, 8)}px`
  }
}

function openExportMenu() {
  if (exporting.value) return
  exportOpen.value = !exportOpen.value
  if (exportOpen.value) positionExportMenu()
}

// El hero tiene overflow:hidden por sus círculos decorativos; el menú flota
// con position:fixed y se cierra si la página se mueve.
function closeExportMenuOnMove() {
  if (exportOpen.value) exportOpen.value = false
}

function onDocumentClick(e: MouseEvent) {
  const target = e.target as HTMLElement
  if (exportOpen.value && !target.closest('.export-menu')) {
    exportOpen.value = false
  }
}

async function runExport(format: 'excel' | 'csv' | 'json' | 'xml') {
  exportOpen.value = false
  await exportCutoff(format)
}

// Tema actual (para colores de ejes/leyendas legibles en ambos modos)
const isDark = ref(document.documentElement.getAttribute('data-theme') === 'dark')
let themeObserver: MutationObserver | null = null

const axisColor = () => (isDark.value ? '#9FB0C9' : '#475569')
const gridColor = () => (isDark.value ? 'rgba(148, 163, 184, 0.22)' : '#E2E8F0')
const legendColor = () => (isDark.value ? '#E6EDF7' : '#334155')

// ── Serie completa del periodo: rellena con ceros los días sin solicitudes ──
const trendFull = computed(() => {
  if (!cutoff.value || trendData.value.length === 0) return []
  const start = new Date(cutoff.value.startDate)
  const end = new Date(cutoff.value.endDate)
  const days = Math.min(Math.round((end.getTime() - start.getTime()) / 86400000) + 1, 92)
  if (!Number.isFinite(days) || days <= 0) return []
  const porDia = new Map(trendData.value.map(t => [t.date.slice(0, 10), t]))
  const out: { label: string; total: number; approved: number; rejected: number }[] = []
  for (let i = 0; i < days; i++) {
    const d = new Date(start)
    d.setDate(d.getDate() + i)
    const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
    const hit = porDia.get(key)
    out.push({
      label: d.toLocaleDateString('es-MX', { day: '2-digit', month: 'short' }),
      total: hit?.total ?? 0,
      approved: hit?.approved ?? 0,
      rejected: hit?.rejected ?? 0
    })
  }
  return out
})

const trendChartOptions = computed((): any => ({
  chart: {
    id: 'trend-chart',
    toolbar: { show: false },
    fontFamily: chartFontFamily
  },
  xaxis: {
    categories: trendFull.value.map(t => t.label),
    tickAmount: Math.min(trendFull.value.length || 1, 14),
    labels: {
      rotate: -35,
      hideOverlappingLabels: true,
      style: { colors: axisColor(), fontSize: '11px', fontWeight: 600 }
    }
  },
  yaxis: {
    min: 0,
    forceNiceScale: true,
    labels: { style: { colors: axisColor(), fontSize: '11px', fontWeight: 600 } }
  },
  colors: ['#C5A059', '#10B981', '#EF4444'],
  dataLabels: { enabled: false },
  stroke: { curve: 'smooth' as const, width: 2.5 },
  fill: { type: 'gradient', gradient: { shadeIntensity: 0.4, opacityFrom: 0.35, opacityTo: 0.05 } },
  grid: { borderColor: gridColor(), strokeDashArray: 4 },
  legend: {
    position: 'top' as const,
    horizontalAlign: 'left' as const,
    offsetY: -4,
    fontFamily: chartFontFamily,
    fontSize: '12px',
    labels: { colors: legendColor() },
    markers: { radius: 4 }
  },
  tooltip: { theme: isDark.value ? 'dark' : 'light' }
}))

const trendChartSeries = computed(() => [
  { name: 'Total Solicitudes', data: trendFull.value.map(t => t.total) },
  { name: 'Aprobados', data: trendFull.value.map(t => t.approved) },
  { name: 'Rechazados', data: trendFull.value.map(t => t.rejected) }
])

const donutTotal = computed(() => permissionsByType.value.reduce((a, t) => a + t.value, 0))

const donutChartOptions = computed((): any => {
  const total = donutTotal.value
  return {
    chart: { fontFamily: chartFontFamily, type: 'donut' as const },
    labels: permissionsByType.value.map(t => t.name),
    colors: permissionsByType.value.map(t => t.color || '#3b82f6'),
    stroke: { width: 2, colors: [isDark.value ? '#1D2E52' : '#FFFFFF'] },
    legend: {
      position: 'bottom' as const,
      fontFamily: chartFontFamily,
      fontSize: '12px',
      markers: { radius: 4 },
      labels: { colors: legendColor() },
      formatter: (name: string, opts: any) => {
        const serie = opts?.w?.globals?.series?.[opts.seriesIndex] ?? 0
        const pct = total > 0 ? Math.round((serie / total) * 100) : 0
        return `${name}: ${serie} (${pct}%)`
      }
    },
    dataLabels: {
      enabled: total > 0,
      formatter: (_val: number, opts: any) => {
        const pct = opts?.percent ?? 0
        return pct >= 5 ? `${Math.round(pct)}%` : ''
      },
      style: { fontFamily: chartFontFamily, fontSize: '11px', fontWeight: 700 },
      dropShadow: { enabled: false }
    },
    plotOptions: {
      pie: {
        donut: {
          size: '66%',
          labels: {
            show: true,
            name: { show: true, fontSize: '12px', color: axisColor(), fontFamily: chartFontFamily, offsetY: -2 },
            value: {
              show: true, fontSize: '26px', fontWeight: 800,
              color: legendColor(), fontFamily: chartFontFamily,
              offsetY: 4, formatter: () => `${donutTotal.value}`
            },
            total: {
              show: true, showAlways: true,
              label: 'Permisos',
              fontSize: '12px', fontWeight: 600,
              color: axisColor(), fontFamily: chartFontFamily,
              formatter: () => `${donutTotal.value}`
            }
          }
        }
      }
    },
    tooltip: { theme: isDark.value ? 'dark' : 'light' }
  }
})

const donutChartSeries = computed(() => permissionsByType.value.map(t => t.value))

const barChartOptions = computed((): any => ({
  chart: { fontFamily: chartFontFamily, toolbar: { show: false } },
  plotOptions: {
    bar: { horizontal: true, barHeight: '50%', borderRadius: 4 }
  },
  colors: ['#1e3a8a'],
  xaxis: {
    categories: permissionsByArea.value.map(a => a.name),
    labels: { style: { colors: axisColor(), fontSize: '11px', fontWeight: 600 } }
  },
  yaxis: {
    labels: { style: { colors: legendColor(), fontSize: '11px', fontWeight: 600 } }
  },
  grid: { borderColor: gridColor() },
  dataLabels: { enabled: true, style: { fontSize: '11px', fontWeight: 700 }, background: { enabled: false } },
  legend: { show: false }
}))

const barChartSeries = computed(() => [
  { name: 'Solicitudes', data: permissionsByArea.value.map(a => a.value) }
])

const radialChartOptions = computed((): any => ({
  chart: { fontFamily: chartFontFamily },
  plotOptions: {
    radialBar: {
      hollow: { size: '55%' },
      dataLabels: {
        name: { show: true, fontSize: '12px', color: axisColor(), fontFamily: chartFontFamily, fontWeight: 600 },
        value: {
          show: true, fontSize: '18px', color: legendColor(), fontFamily: chartFontFamily, fontWeight: 800,
          formatter: (val: number) => `${val}%`
        }
      }
    }
  },
  labels: ['Con Goce', 'Sin Goce'],
  colors: ['#10b981', '#ef4444'],
  legend: { show: true, position: 'bottom' as const, fontFamily: chartFontFamily, fontSize: '12px', labels: { colors: legendColor() }, markers: { radius: 4 } }
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
    topRequesters.value = data.topRequesters || []
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
  document.addEventListener('click', onDocumentClick)
  window.addEventListener('scroll', closeExportMenuOnMove, true)
  window.addEventListener('resize', closeExportMenuOnMove)
  themeObserver = new MutationObserver(() => {
    isDark.value = document.documentElement.getAttribute('data-theme') === 'dark'
  })
  themeObserver.observe(document.documentElement, { attributes: true, attributeFilter: ['data-theme'] })
})

onUnmounted(() => {
  document.removeEventListener('click', onDocumentClick)
  window.removeEventListener('scroll', closeExportMenuOnMove, true)
  window.removeEventListener('resize', closeExportMenuOnMove)
  themeObserver?.disconnect()
})
</script>

<style scoped>
/* Separación uniforme entre todas las secciones de la vista */
.cutoff-details > * + * {
  margin-top: 1.4rem;
}

/* ── Header: badge con espacio propio + dropdown de exportación ── */
.status-slot {
  display: inline-flex;
  align-items: center;
  min-width: 96px;
  justify-content: center;
}

.export-menu {
  position: relative;
}

.export-toggle {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  background: var(--tecnm-status-approved-bg, rgba(16, 185, 129, 0.16));
  color: var(--tecnm-status-approved-text, #10b981);
  border: none;
  border-radius: 999px;
  padding: 0.4rem 0.95rem;
  font-size: 0.8rem;
  font-weight: 700;
  font-family: inherit;
  cursor: pointer;
  transition: filter 0.15s ease;
}

.export-toggle:hover:not(:disabled) { filter: brightness(1.12); }
.export-toggle:disabled { opacity: 0.6; cursor: wait; }

.export-caret {
  font-size: 0.65rem;
  transition: transform 0.15s ease;
}
.export-caret.open { transform: rotate(180deg); }

.export-menu__list {
  z-index: 1000;
  margin: 0;
  padding: 0.35rem;
  list-style: none;
  min-width: 190px;
  background: var(--tecnm-surface-white);
  border: 1px solid var(--tecnm-border-color);
  border-radius: 10px;
  box-shadow: var(--tecnm-shadow-lg, 0 10px 15px -3px rgba(0, 0, 0, 0.35));
}

.export-menu__list button {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  width: 100%;
  padding: 0.5rem 0.65rem;
  border: none;
  border-radius: 7px;
  background: transparent;
  color: var(--tecnm-text-primary);
  font-size: 0.8rem;
  font-weight: 600;
  font-family: inherit;
  text-align: left;
  cursor: pointer;
}

.export-menu__list button:hover { background: var(--tecnm-soft-hover); }

.export-icon--excel { color: #10b981; }
.export-icon--csv { color: #38bdf8; }
.export-icon--json { color: #f59e0b; }
.export-icon--xml { color: #ef4444; }

.export-menu__divider {
  margin: 0.35rem 0.4rem;
  border: none;
  border-top: 1px solid var(--tecnm-border-color);
}

/* ── KPIs: scorecards en grid ── */
.kpi-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1rem;
}

@media (min-width: 576px) {
  .kpi-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}

@media (min-width: 992px) {
  .kpi-grid { grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 1.1rem; }
}

.kpi-card {
  display: flex;
  align-items: center;
  gap: 0.8rem;
}

.kpi-icon {
  width: 40px;
  height: 40px;
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
  font-size: 0.75rem;
  font-weight: 700;
  color: var(--tecnm-text-secondary, #475569);
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.kpi-value {
  font-size: 1.35rem;
  font-weight: 800;
  color: var(--tecnm-text-primary, #0f172a);
  margin: 0.15rem 0;
  line-height: 1.1;
}

.kpi-sub {
  font-size: 0.75rem;
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
  font-size: 0.75rem;
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

.charts-duo {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1rem;
}

@media (min-width: 768px) {
  .charts-duo { grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 1.1rem; }
}

.trend-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.chart-container {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 290px;
}

.chart-empty {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 280px;
  color: var(--tecnm-text-secondary, #64748b);
  font-size: 0.85rem;
}

.top-list {
  list-style: none;
  margin: 0;
  padding: 0.25rem 0.25rem 0;
}

.top-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.55rem 0.35rem;
}

.top-item + .top-item { border-top: 1px solid var(--tecnm-border-color); }

.top-rank {
  width: 26px;
  height: 26px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.75rem;
  font-weight: 800;
  flex-shrink: 0;
  background: var(--tecnm-bg-main);
  color: var(--tecnm-text-secondary);
  border: 1px solid var(--tecnm-border-color);
}
.top-rank--1 { background: #FEF3C7; color: #B45309; border-color: #FCD34D; }
.top-rank--2 { background: #E2E8F0; color: #475569; border-color: #CBD5E1; }
.top-rank--3 { background: #FFEDD5; color: #C2410C; border-color: #FDBA74; }

.top-info { display: flex; flex-direction: column; min-width: 0; flex: 1; }
.top-name {
  font-size: 0.86rem;
  font-weight: 700;
  color: var(--tecnm-text-primary);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.top-area {
  font-size: 0.72rem;
  color: var(--tecnm-text-secondary);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.top-count {
  font-size: 0.95rem;
  font-weight: 800;
  color: var(--tecnm-blue-primary, #1d4ed8);
  flex-shrink: 0;
}

.audit-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 0.9rem;
}

@media (min-width: 768px) {
  .audit-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}

.audit-box {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
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

.audit-head { display: flex; align-items: center; gap: 0.75rem; }

.audit-date {
  font-size: 0.72rem;
  font-weight: 600;
  color: var(--tecnm-text-secondary);
  white-space: nowrap;
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
