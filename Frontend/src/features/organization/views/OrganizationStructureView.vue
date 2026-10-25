<template>
  <div class="org-structure-view">
    <!-- Encabezado de Página Institucional -->
    <AppPageHeader
      title="Estructura Organizacional"
      subtitle="Jerarquía centralizada e interactiva de Áreas y Departamentos del TecNM"
      icon="fa-sitemap"
    />

    <!-- Tarjeta Superior: Resumen de Navegación y Breadcrumb Dinámico -->
    <section class="tecnm-card tecnm-mb-4 org-topbar-card">
      <div class="org-topbar">
        <div class="org-topbar__info">
          <h2 class="org-topbar__title">Jerarquía Institucional</h2>
          <p class="org-topbar__desc">
            Las Áreas (Direcciones) agrupan y coordinan los Departamentos operativos institucionales.
          </p>
        </div>

        <div class="org-path" aria-label="Ruta de exploración">
          <button
            type="button"
            class="org-path__chip"
            :class="{ 'org-path__chip--active': selectedArea }"
            @click="clearAreaSelection"
            title="Filtrar por área"
          >
            <i class="fa fa-layer-group" aria-hidden="true"></i>
            <span>{{ selectedArea ? selectedArea.description : 'Todas las Áreas' }}</span>
          </button>

          <i class="fa fa-chevron-right org-path__sep" aria-hidden="true"></i>

          <button
            type="button"
            class="org-path__chip"
            :class="{ 'org-path__chip--active': selectedDept, 'org-path__chip--empty': !selectedDept }"
            @click="clearDeptSelection"
            title="Filtrar por departamento"
          >
            <i class="fa fa-sitemap" aria-hidden="true"></i>
            <span>{{ selectedDept ? selectedDept.description : 'Todos los Departamentos' }}</span>
          </button>
        </div>
      </div>
    </section>

    <!-- Spinner de Carga General -->
    <AppSpinner v-if="loading" />

    <!-- Grid de 2 Columnas Centralizadas (Áreas y Departamentos) -->
    <div v-else class="org-cols">
      <!-- ================================================================= -->
      <!-- COLUMNA 1: ÁREAS                                                  -->
      <!-- ================================================================= -->
      <section class="tecnm-card org-col">
        <div class="org-col__header">
          <div class="org-col__icon org-col__icon--area">
            <i class="fa fa-layer-group" aria-hidden="true"></i>
          </div>
          <div class="org-col__titles">
            <h3 class="org-col__title">Áreas (Direcciones)</h3>
            <small class="org-col__subtitle">{{ areas.length }} área(s) en total</small>
          </div>
          <AppButton
            variant="primary"
            icon="fa-plus"
            class="org-col__btn-new"
            @click="openAreaModal()"
          >
            Nueva
          </AppButton>
        </div>

        <div class="org-col__search">
          <div class="org-search-wrap">
            <i class="fa fa-search org-search-icon" aria-hidden="true"></i>
            <input
              v-model="queryArea"
              type="text"
              class="org-search-input"
              placeholder="Buscar área…"
              aria-label="Buscar área"
            />
          </div>
        </div>

        <div class="org-col__list" role="listbox">
          <div
            v-for="a in filteredAreas"
            :key="a.id"
            class="org-item"
            :class="{ 'org-item--selected': selectedAreaId === a.id, 'org-item--inactive': !a.status }"
            tabindex="0"
            role="option"
            :aria-selected="selectedAreaId === a.id"
            @click="pickArea(a.id)"
            @keydown.enter="pickArea(a.id)"
          >
            <div class="org-item__icon org-col__icon--area">
              <i class="fa fa-layer-group" aria-hidden="true"></i>
            </div>
            <div class="org-item__body">
              <b class="org-item__title">{{ a.description }}</b>
              <small class="org-item__meta">
                {{ a.departmentCount ?? 0 }} departamento(s) adscrito(s)
                <span v-if="!a.status" class="org-tag org-tag--inactive">Inactiva</span>
              </small>
            </div>
            <div class="org-item__actions">
              <button
                type="button"
                class="org-item__btn"
                title="Editar área"
                @click.stop="openAreaModal(a)"
              >
                <i class="fa fa-edit" aria-hidden="true"></i>
              </button>
              <button
                v-if="a.status"
                type="button"
                class="org-item__btn org-item__btn--danger"
                title="Desactivar área (borrado lógico)"
                @click.stop="deactivateArea(a)"
              >
                <i class="fa fa-trash" aria-hidden="true"></i>
              </button>
              <button
                v-else
                type="button"
                class="org-item__btn org-item__btn--reactivate"
                title="Reactivar área"
                @click.stop="reactivateArea(a)"
              >
                <i class="fa fa-undo" aria-hidden="true"></i>
              </button>
            </div>
            <i class="fa fa-chevron-right org-item__arrow" aria-hidden="true"></i>
          </div>

          <div v-if="!filteredAreas.length" class="org-empty">
            <i class="fa fa-folder-open-o org-empty__icon" aria-hidden="true"></i>
            <p>{{ queryArea ? 'Sin resultados para la búsqueda.' : 'No hay áreas registradas.' }}</p>
          </div>
        </div>
      </section>

      <!-- ================================================================= -->
      <!-- COLUMNA 2: DEPARTAMENTOS                                          -->
      <!-- ================================================================= -->
      <section class="tecnm-card org-col">
        <div class="org-col__header">
          <div class="org-col__icon org-col__icon--dept">
            <i class="fa fa-sitemap" aria-hidden="true"></i>
          </div>
          <div class="org-col__titles">
            <h3 class="org-col__title">Departamentos</h3>
            <small class="org-col__subtitle">
              {{ selectedArea ? `${filteredDepartments.length} departamento(s) en ${selectedArea.description}` : `${departments.length} en total` }}
            </small>
          </div>
          <AppButton
            variant="primary"
            icon="fa-plus"
            class="org-col__btn-new"
            @click="openDeptModal()"
          >
            Nuevo
          </AppButton>
        </div>

        <div class="org-col__search">
          <div class="org-search-wrap">
            <i class="fa fa-search org-search-icon" aria-hidden="true"></i>
            <input
              v-model="queryDept"
              type="text"
              class="org-search-input"
              placeholder="Buscar departamento…"
              aria-label="Buscar departamento"
            />
          </div>
        </div>

        <div class="org-col__list" role="listbox">
          <div
            v-for="d in filteredDepartments"
            :key="d.id"
            class="org-item"
            :class="{ 'org-item--selected': selectedDeptId === d.id, 'org-item--inactive': !d.status }"
            tabindex="0"
            role="option"
            :aria-selected="selectedDeptId === d.id"
            @click="pickDept(d.id)"
            @keydown.enter="pickDept(d.id)"
          >
            <div class="org-item__icon org-col__icon--dept">
              <i class="fa fa-sitemap" aria-hidden="true"></i>
            </div>
            <div class="org-item__body">
              <b class="org-item__title">{{ d.description }}</b>
              <small class="org-item__meta">
                {{ d.areaDescription || 'Sin área asignada' }}
                <span v-if="!d.status" class="org-tag org-tag--inactive">Inactivo</span>
              </small>
            </div>
            <div class="org-item__actions">
              <button
                type="button"
                class="org-item__btn"
                title="Editar departamento"
                @click.stop="openDeptModal(d)"
              >
                <i class="fa fa-edit" aria-hidden="true"></i>
              </button>
              <button
                v-if="d.status"
                type="button"
                class="org-item__btn org-item__btn--danger"
                title="Desactivar departamento (borrado lógico)"
                @click.stop="deactivateDept(d)"
              >
                <i class="fa fa-trash" aria-hidden="true"></i>
              </button>
              <button
                v-else
                type="button"
                class="org-item__btn org-item__btn--reactivate"
                title="Reactivar departamento"
                @click.stop="reactivateDept(d)"
              >
                <i class="fa fa-undo" aria-hidden="true"></i>
              </button>
            </div>
          </div>

          <div v-if="!filteredDepartments.length" class="org-empty">
            <i class="fa fa-sitemap org-empty__icon" aria-hidden="true"></i>
            <p v-if="selectedArea">
              No hay departamentos en <b>{{ selectedArea.description }}</b>.<br />
              Crea uno nuevo con el botón "+ Nuevo".
            </p>
            <p v-else-if="queryDept">Sin resultados para la búsqueda.</p>
            <p v-else>No hay departamentos registrados.</p>
          </div>
        </div>
      </section>
    </div>

    <!-- ================================================================= -->
    <!-- MODAL 1: FORMULARIO DE ÁREA                                       -->
    <!-- ================================================================= -->
    <AppModal
      v-model:show="showAreaModal"
      :title="editingArea ? `Editar Área #${editingArea.id}` : 'Nueva Área'"
      :eyebrow="editingArea ? `ÁREA ORGANIZACIONAL #${editingArea.id}` : 'CATÁLOGO DE ÁREAS'"
      header-variant="icon"
      icon="fa-layer-group"
      size="md"
      :loading="submitting"
      :confirm-text="editingArea ? 'Actualizar Área' : 'Guardar Área'"
      @confirm="saveArea"
    >
      <div v-if="editingArea" class="sec tecnm-mb-3">
        <div class="row">
          <i class="fa fa-sitemap ic" aria-hidden="true"></i>
          <div class="tx">
            <strong>Departamentos Vinculados</strong>
            <small>{{ editingArea.departmentCount ?? 0 }} departamento(s) activos asignados</small>
          </div>
          <span :class="editingArea.status ? 'badge--ok' : 'badge--no'">
            {{ editingArea.status ? 'Activa' : 'Inactiva' }}
          </span>
        </div>
      </div>

      <label class="field">
        <span class="lbl">NOMBRE DEL ÁREA *</span>
        <input
          v-model="areaForm.description"
          type="text"
          class="in"
          placeholder="Ej. ADMINISTRACIÓN Y FINANZAS, DIRECCIÓN GENERAL..."
          maxlength="100"
          autofocus
          @keyup.enter="saveArea"
        />
        <span class="help">Nombre canónico del área organizacional (máx. 100 caracteres).</span>
      </label>
    </AppModal>

    <!-- ================================================================= -->
    <!-- MODAL 2: FORMULARIO DE DEPARTAMENTO                               -->
    <!-- ================================================================= -->
    <AppModal
      v-model:show="showDeptModal"
      :title="editingDept ? `Editar Departamento #${editingDept.id}` : 'Nuevo Departamento'"
      :eyebrow="editingDept ? `DEPARTAMENTO #${editingDept.id}` : 'CATÁLOGO DE DEPARTAMENTOS'"
      header-variant="icon"
      icon="fa-sitemap"
      size="md"
      :loading="submitting"
      :confirm-text="editingDept ? 'Actualizar Departamento' : 'Guardar Departamento'"
      @confirm="saveDept"
    >
      <label class="field tecnm-mb-3">
        <span class="lbl">NOMBRE DEL DEPARTAMENTO *</span>
        <input
          v-model="deptForm.description"
          type="text"
          class="in"
          placeholder="Ej. RECURSOS HUMANOS, DOCENCIA, MANTENIMIENTO..."
          maxlength="100"
          autofocus
          @keyup.enter="saveDept"
        />
        <span class="help">Descripción oficial del departamento (máx. 100 caracteres).</span>
      </label>

      <label class="field">
        <span class="lbl">ÁREA A LA QUE PERTENECE *</span>
        <select v-model="deptForm.areaId" class="in">
          <option value="">Selecciona un área organizacional...</option>
          <option v-for="a in activeAreas" :key="a.id" :value="a.id">
            {{ a.description }}
          </option>
        </select>
        <span class="help">El departamento quedará adscrito a esta área organizacional.</span>
      </label>
    </AppModal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted } from 'vue'
import { http } from '@/shared/api/client'
import AppPageHeader from '@/shared/components/AppPageHeader.vue'
import AppButton from '@/shared/components/AppButton.vue'
import AppSpinner from '@/shared/components/AppSpinner.vue'
import AppModal from '@/shared/components/AppModal.vue'
import { askConfirmation, showToast } from '@/shared/composables/useAlerts'

// --- Interfaces de Datos ---
interface AreaItem {
  id: number
  description: string
  status: boolean
  departmentCount?: number
}

interface DepartmentItem {
  id: number
  description: string
  areaId?: number | null
  areaDescription?: string | null
  status: boolean
}

interface Paged<T> {
  items: T[]
  totalItems: number
  totalPages: number
  page: number
}

// --- Estado Reactivo Principal ---
const loading = ref(true)
const submitting = ref(false)

const areas = ref<AreaItem[]>([])
const departments = ref<DepartmentItem[]>([])

// Selección interactiva
const selectedAreaId = ref<number | null>(null)
const selectedDeptId = ref<number | null>(null)

// Filtros de búsqueda
const queryArea = ref('')
const queryDept = ref('')

// Modales reactivos
const showAreaModal = ref(false)
const editingArea = ref<AreaItem | null>(null)
const areaForm = reactive({ description: '' })

const showDeptModal = ref(false)
const editingDept = ref<DepartmentItem | null>(null)
const deptForm = reactive({ description: '', areaId: '' as string | number })

// --- Propiedades Computadas ---
const selectedArea = computed(() => areas.value.find(a => a.id === selectedAreaId.value) || null)
const selectedDept = computed(() => departments.value.find(d => d.id === selectedDeptId.value) || null)
const activeAreas = computed(() => areas.value.filter(a => a.status))

const filteredAreas = computed(() => {
  const q = queryArea.value.trim().toLowerCase()
  let list = [...areas.value]
  if (q) list = list.filter(a => a.description.toLowerCase().includes(q))
  return list.sort((a, b) => a.description.localeCompare(b.description, 'es'))
})

const filteredDepartments = computed(() => {
  const q = queryDept.value.trim().toLowerCase()
  let list = [...departments.value]

  // Si hay un área seleccionada, filtrar exclusivamente sus departamentos
  if (selectedAreaId.value !== null) {
    list = list.filter(d => d.areaId === selectedAreaId.value)
  }

  if (q) list = list.filter(d => d.description.toLowerCase().includes(q))
  return list.sort((a, b) => a.description.localeCompare(b.description, 'es'))
})

// --- Carga de Datos ---
async function fetchAllData() {
  loading.value = true
  try {
    const [resAreas, resDepts] = await Promise.all([
      http.get<Paged<AreaItem>>('/api/Areas', { params: { pageSize: 100 } }),
      http.get<Paged<DepartmentItem>>('/api/Departments', { params: { pageSize: 100 } })
    ])

    areas.value = resAreas.data.items || []
    departments.value = resDepts.data.items || []

    // Inicializar selección en la primera área si no hay una activa
    if (selectedAreaId.value === null && areas.value.length > 0) {
      selectedAreaId.value = areas.value[0].id
    }
  } catch {
    showToast('Error cargando estructura organizacional.', 'error')
  } finally {
    loading.value = false
  }
}

// --- Selección de Niveles ---
function pickArea(id: number) {
  if (selectedAreaId.value === id) return
  selectedAreaId.value = id
  const firstDept = departments.value.find(d => d.areaId === id)
  selectedDeptId.value = firstDept ? firstDept.id : null
}

function pickDept(id: number) {
  selectedDeptId.value = id
}

function clearAreaSelection() {
  selectedAreaId.value = null
  selectedDeptId.value = null
}

function clearDeptSelection() {
  selectedDeptId.value = null
}

// =====================================================================
// OPERACIONES CRUD: ÁREAS
// =====================================================================
function openAreaModal(a?: AreaItem) {
  if (a) {
    editingArea.value = a
    areaForm.description = a.description
  } else {
    editingArea.value = null
    areaForm.description = ''
  }
  showAreaModal.value = true
}

async function saveArea() {
  const desc = areaForm.description.trim()
  if (!desc) {
    showToast('El nombre del área es requerido.', 'warning')
    return
  }

  submitting.value = true
  try {
    if (editingArea.value) {
      await http.put(`/api/Areas/${editingArea.value.id}`, {
        id: editingArea.value.id,
        description: desc
      })
      showToast('Área actualizada.', 'success')
    } else {
      const res = await http.post('/api/Areas', { description: desc })
      showToast('Área creada exitosamente.', 'success')
      if (res.data?.id) selectedAreaId.value = res.data.id
    }
    showAreaModal.value = false
    await fetchAllData()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo guardar el área.', 'error')
  } finally {
    submitting.value = false
  }
}

async function deactivateArea(a: AreaItem) {
  const hasDepts = (a.departmentCount && a.departmentCount > 0)
  const message = hasDepts
    ? `Atención: Esta área tiene ${a.departmentCount} departamento(s) activos asociados. ¿Deseas desactivarla?`
    : `Esta acción marcará inactiva el área "${a.description}". Podrás reactivarla después.`

  const ok = await askConfirmation({
    title: '¿Desactivar Área?',
    text: message,
    confirmText: 'Sí, desactivar',
    danger: true
  })

  if (!ok) return
  try {
    await http.put(`/api/Areas/${a.id}/deactivate`)
    showToast('Área desactivada.', 'success')
    await fetchAllData()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo desactivar el área.', 'error')
  }
}

async function reactivateArea(a: AreaItem) {
  const ok = await askConfirmation({
    title: '¿Reactivar Área?',
    text: `El área "${a.description}" volverá a estar activa.`,
    confirmText: 'Sí, reactivar',
    danger: false
  })

  if (!ok) return
  try {
    await http.put(`/api/Areas/${a.id}/activate`)
    showToast('Área reactivada.', 'success')
    await fetchAllData()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo reactivar el área.', 'error')
  }
}

// =====================================================================
// OPERACIONES CRUD: DEPARTAMENTOS
// =====================================================================
function openDeptModal(d?: DepartmentItem) {
  if (d) {
    editingDept.value = d
    deptForm.description = d.description
    deptForm.areaId = d.areaId ?? ''
  } else {
    editingDept.value = null
    deptForm.description = ''
    deptForm.areaId = selectedAreaId.value ?? ''
  }
  showDeptModal.value = true
}

async function saveDept() {
  const desc = deptForm.description.trim()
  if (!desc) {
    showToast('El nombre del departamento es requerido.', 'warning')
    return
  }
  if (!deptForm.areaId) {
    showToast('Debes seleccionar el área a la que pertenece.', 'warning')
    return
  }

  submitting.value = true
  try {
    const payload = {
      description: desc,
      areaId: Number(deptForm.areaId)
    }

    if (editingDept.value) {
      await http.put(`/api/Departments/${editingDept.value.id}`, {
        id: editingDept.value.id,
        ...payload
      })
      showToast('Departamento actualizado.', 'success')
    } else {
      const res = await http.post('/api/Departments', payload)
      showToast('Departamento creado exitosamente.', 'success')
      if (res.data?.id) selectedDeptId.value = res.data.id
    }
    showDeptModal.value = false
    await fetchAllData()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo guardar el departamento.', 'error')
  } finally {
    submitting.value = false
  }
}

async function deactivateDept(d: DepartmentItem) {
  const ok = await askConfirmation({
    title: '¿Desactivar Departamento?',
    text: `Esta acción marcará inactivo el departamento "${d.description}".`,
    confirmText: 'Sí, desactivar',
    danger: true
  })

  if (!ok) return
  try {
    await http.put(`/api/Departments/${d.id}/deactivate`)
    showToast('Departamento desactivado.', 'success')
    await fetchAllData()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo desactivar el departamento.', 'error')
  }
}

async function reactivateDept(d: DepartmentItem) {
  const ok = await askConfirmation({
    title: '¿Reactivar Departamento?',
    text: `El departamento "${d.description}" volverá a estar activo.`,
    confirmText: 'Sí, reactivar',
    danger: false
  })

  if (!ok) return
  try {
    await http.put(`/api/Departments/${d.id}/activate`)
    showToast('Departamento reactivado.', 'success')
    await fetchAllData()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo reactivar el departamento.', 'error')
  }
}

function errMessage(e: any): string {
  const d = e?.response?.data
  if (typeof d === 'string') return d
  return d?.message || d?.error || ''
}

onMounted(() => {
  fetchAllData()
})
</script>

<style scoped>
.org-structure-view {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

/* --- Topbar Card --- */
.org-topbar-card {
  border-top: 3px solid var(--tecnm-gold, #C9A24D);
  padding: 1rem 1.25rem;
}

.org-topbar {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}

.org-topbar__title {
  margin: 0;
  font-size: 1.15rem;
  font-weight: 700;
  color: var(--tecnm-text-primary);
}

.org-topbar__desc {
  margin: 0.25rem 0 0;
  font-size: 0.85rem;
  color: var(--tecnm-text-secondary);
}

.org-path {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.4rem;
}

.org-path__chip {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.35rem 0.75rem;
  border-radius: 9999px;
  border: 1px solid var(--tecnm-border-color);
  background: var(--tecnm-surface-soft, #f8fafc);
  color: var(--tecnm-text-secondary);
  font-size: 0.82rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.org-path__chip:hover {
  background: var(--tecnm-soft-hover);
  color: var(--tecnm-accent-text);
  border-color: var(--tecnm-blue-hover);
}

.org-path__chip--active {
  background: rgba(27, 57, 106, 0.08);
  color: var(--tecnm-primary, #1B396A);
  border-color: rgba(27, 57, 106, 0.2);
}

.org-path__chip--empty {
  opacity: 0.7;
}

.org-path__sep {
  font-size: 0.75rem;
  color: var(--tecnm-gold, #C9A24D);
}

/* --- 2 Column Layout (Simétrico y Amplio) --- */
.org-cols {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1.25rem;
}

.org-col {
  display: flex;
  flex-direction: column;
  overflow: hidden;
  min-height: 480px;
}

.org-col__header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem 1.25rem;
  border-bottom: 1px solid var(--tecnm-border-color);
  border-left: 4px solid var(--tecnm-primary, #1B396A);
  background: var(--tecnm-surface-white);
}

.org-col__icon {
  flex: none;
  width: 34px;
  height: 34px;
  border-radius: var(--tecnm-radius-md, 8px);
  display: grid;
  place-items: center;
  font-size: 0.95rem;
}

.org-col__icon--area {
  background: rgba(27, 57, 106, 0.1);
  color: var(--tecnm-primary, #1B396A);
}

.org-col__icon--dept {
  background: rgba(201, 162, 77, 0.15);
  color: #926d1a;
}

.org-col__titles {
  min-width: 0;
  flex: 1;
}

.org-col__title {
  margin: 0;
  font-size: 1rem;
  font-weight: 700;
  color: var(--tecnm-text-primary);
}

.org-col__subtitle {
  display: block;
  font-size: 0.78rem;
  color: var(--tecnm-text-secondary);
  margin-top: 1px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.org-col__btn-new {
  flex: none;
  font-size: 0.8rem !important;
  padding: 0.35rem 0.75rem !important;
}

.org-col__search {
  padding: 0.75rem 1rem 0.35rem;
}

.org-search-wrap {
  position: relative;
  display: flex;
  align-items: center;
}

.org-search-icon {
  position: absolute;
  left: 0.75rem;
  font-size: 0.82rem;
  color: var(--tecnm-text-secondary);
  pointer-events: none;
}

.org-search-input {
  width: 100%;
  height: 36px;
  padding: 0 0.75rem 0 2.2rem;
  border: 1px solid var(--tecnm-border-color);
  border-radius: var(--tecnm-radius-md, 8px);
  background: var(--tecnm-surface-soft, #f8fafc);
  color: var(--tecnm-text-primary);
  font-size: 0.85rem;
  outline: none;
  transition: all 0.2s ease;
}

.org-search-input:focus {
  background: var(--tecnm-surface-white);
  border-color: var(--tecnm-gold, #C9A24D);
  box-shadow: 0 0 0 3px rgba(201, 162, 77, 0.2);
}

.org-col__list {
  padding: 0.5rem 0.75rem 1rem;
  overflow-y: auto;
  max-height: 520px;
  min-height: 300px;
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
}

/* --- Item Interactivo --- */
.org-item {
  position: relative;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.6rem 0.85rem;
  border-radius: var(--tecnm-radius-md, 8px);
  border: 1px solid transparent;
  cursor: pointer;
  outline: none;
  transition: all 0.2s ease;
  background: transparent;
}

.org-item::before {
  content: "";
  position: absolute;
  left: 0;
  top: 8px;
  bottom: 8px;
  width: 3px;
  border-radius: 3px;
  background: var(--tecnm-gold, #C9A24D);
  opacity: 0;
  transition: opacity 0.2s ease;
}

.org-item:hover {
  background: var(--tecnm-soft-hover);
  border-color: var(--tecnm-border-color);
}

.org-item:hover::before {
  opacity: 0.5;
}

.org-item:focus-visible {
  border-color: var(--tecnm-gold, #C9A24D);
  box-shadow: 0 0 0 3px rgba(201, 162, 77, 0.2);
}

.org-item--selected {
  background: rgba(201, 162, 77, 0.08) !important;
  border-color: rgba(201, 162, 77, 0.35) !important;
}

.org-item--selected::before {
  opacity: 1 !important;
}

.org-item--inactive {
  opacity: 0.65;
}

.org-item__icon {
  flex: none;
  width: 30px;
  height: 30px;
  border-radius: 6px;
  display: grid;
  place-items: center;
  font-size: 0.9rem;
}

.org-item--selected .org-item__icon {
  background: var(--tecnm-primary, #1B396A) !important;
  color: #fff !important;
}

.org-item__body {
  min-width: 0;
  flex: 1;
}

.org-item__title {
  display: block;
  font-size: 0.9rem;
  font-weight: 600;
  color: var(--tecnm-text-primary);
  overflow-wrap: anywhere;
}

.org-item__meta {
  display: block;
  font-size: 0.78rem;
  color: var(--tecnm-text-secondary);
  margin-top: 2px;
}

.org-tag {
  display: inline-block;
  padding: 0.1rem 0.4rem;
  border-radius: 4px;
  font-size: 0.7rem;
  font-weight: 700;
  margin-left: 0.3rem;
}

.org-tag--inactive {
  background: rgba(239, 68, 68, 0.12);
  color: var(--tecnm-danger, #ef4444);
}

.org-item__actions {
  display: flex;
  gap: 0.25rem;
  opacity: 0;
  transition: opacity 0.2s ease;
}

.org-item:hover .org-item__actions,
.org-item:focus-within .org-item__actions,
.org-item--selected .org-item__actions {
  opacity: 1;
}

.org-item__btn {
  width: 30px;
  height: 30px;
  display: grid;
  place-items: center;
  border: 0;
  border-radius: 6px;
  background: transparent;
  color: var(--tecnm-text-secondary);
  cursor: pointer;
  font-size: 0.82rem;
  transition: all 0.2s ease;
}

.org-item__btn:hover {
  background: var(--tecnm-surface-white);
  color: var(--tecnm-primary, #1B396A);
}

.org-item__btn--danger:hover {
  background: rgba(239, 68, 68, 0.1);
  color: var(--tecnm-danger, #ef4444);
}

.org-item__btn--reactivate:hover {
  background: rgba(16, 185, 129, 0.1);
  color: #10b981;
}

.org-item__arrow {
  flex: none;
  font-size: 0.75rem;
  color: #9aa6bb;
}

.org-empty {
  padding: 3rem 1rem;
  text-align: center;
  color: var(--tecnm-text-secondary);
  font-size: 0.85rem;
  line-height: 1.5;
}

.org-empty__icon {
  font-size: 2.2rem;
  opacity: 0.3;
  margin-bottom: 0.5rem;
  display: block;
}

/* --- Responsive --- */
@media (max-width: 900px) {
  .org-cols {
    grid-template-columns: 1fr;
  }
  .org-col__list {
    max-height: 320px;
    min-height: 180px;
  }
}
</style>
