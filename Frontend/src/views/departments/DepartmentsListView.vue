<template>
  <div>
    <AppPageHeader title="Gestión de Departamentos" subtitle="Administra el catálogo de departamentos y su área" icon="fa-sitemap">
      <template #actions>
        <AppButton variant="primary" icon="fa-plus" @click="openCreateModal">
          Nuevo Departamento
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
            <label class="tecnm-label" for="searchQuery">Nombre del Departamento</label>
            <input
              id="searchQuery"
              v-model="filters.search"
              type="text"
              class="tecnm-form-control"
              placeholder="Buscar por departamento..."
            />
          </div>

          <div class="tecnm-form-group">
            <label class="tecnm-label" for="areaFilter">Área</label>
            <select id="areaFilter" v-model="filters.areaId" class="tecnm-form-control">
              <option value="">Todas las áreas</option>
              <option v-for="a in areaOptions" :key="a.id" :value="String(a.id)">{{ a.description }}</option>
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

    <!-- Leyenda: diferencia entre Estado y Vigencia -->
    <details class="tecnm-card legend-box">
      <summary>
        <i class="fa fa-question-circle" aria-hidden="true"></i>
        ¿Qué significan <strong>Estado</strong> y <strong>Vigencia</strong>?
      </summary>
      <p><strong>Estado</strong> — ¿existe el registro? <strong>Activo</strong>: aparece en listados y puede asignarse a usuarios. <strong>Inactivo</strong>: baja lógica; nunca se borra físicamente, solo deja de mostrarse y conserva su información e historial.</p>
      <p><strong>Vigencia</strong> — ¿es el departamento oficial actual? <strong>Vigente</strong>: forma parte del organigrama vigente. <strong>Reemplazado</strong>: fue renombrado o absorbido por una reestructura; se conserva para no romper el historial de usuarios y permisos que lo referencian.</p>
      <p class="legend-note"><i class="fa fa-link" aria-hidden="true"></i> Los usuarios asignados conservan su área e historial de permisos aunque el departamento quede Inactivo o Reemplazado. Ninguna acción aquí borra datos físicamente.</p>
    </details>

    <AppSpinner v-if="loading" />
    <template v-else>
      <section class="tecnm-card">
        <div class="tecnm-card-header tecnm-card-header--premium">
          <h4 class="filter-title">Listado de Departamentos</h4>
          <span class="count-badge">{{ totalItems }} registro(s)</span>
        </div>

        <div class="tecnm-table-responsive">
          <table class="tecnm-table tecnm-table-striped">
            <thead>
              <tr>
                <th class="text-center">ID</th>
                <th class="text-center">Descripción / Nombre del Departamento</th>
                <th class="text-center">Área</th>
                <th class="text-center">Estado</th>
                <th class="text-center">Vigencia</th>
                <th class="text-center">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="d in departments" :key="d.id">
                <td class="text-center"><strong>#{{ d.id }}</strong></td>
                <td><strong>{{ d.description }}</strong></td>
                <td>{{ d.areaDescription || '—' }}</td>
                <td class="text-center">
                  <StatusBadge :variant="d.status ? 'approved' : 'rejected'">
                    {{ d.status ? 'Activo' : 'Inactivo' }}
                  </StatusBadge>
                </td>
                <td class="text-center">
                  <StatusBadge :variant="d.isActive ? 'approved' : 'info'">
                    {{ d.isActive ? 'Vigente' : 'Reemplazado' }}
                  </StatusBadge>
                </td>
                <td class="text-center">
                  <div class="tecnm-table-actions">
                    <button
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--edit"
                      title="Editar / cambiar área"
                      @click="openEditModal(d)"
                    >
                      <i class="fa fa-edit" aria-hidden="true"></i>
                    </button>
                    <button
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--toggle"
                      :title="d.isActive ? 'Marcar como Reemplazado' : 'Marcar como Vigente'"
                      @click="toggleDepartmentActive(d)"
                    >
                      <i :class="d.isActive ? 'fa fa-toggle-on' : 'fa fa-toggle-off'" aria-hidden="true"></i>
                    </button>
                    <button
                      v-if="d.status"
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--delete"
                      title="Desactivar departamento"
                      @click="deactivateDepartment(d)"
                    >
                      <i class="fa fa-trash" aria-hidden="true"></i>
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="!departments.length">
                <td colspan="6"><EmptyState message="No hay departamentos registrados"><span>Crea un nuevo departamento para asignarle colaboradores</span></EmptyState></td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="departments.length" class="tecnm-card-footer">
          <div class="pagination-wrap">
            <span class="pagination-info">
              <i class="fa fa-list-ol" aria-hidden="true"></i>
              Mostrando página <strong>{{ page }}</strong> de <strong>{{ totalPages }}</strong>
            </span>
            <PaginationBar
              :page="page"
              :total-pages="totalPages"
              @update:page="(v: number) => { page = v; fetchDepartments() }"
            />
          </div>
        </div>
      </section>
    </template>

    <!-- Modal Formulario Reactivo (Crear / Editar Departamento) -->
    <AppModal
      v-model:show="showModal"
      :title="editingDepartment ? `Editar Departamento #${editingDepartment.id}` : 'Nuevo Departamento'"
      icon="fa-sitemap"
      :loading="submitting"
      :confirm-text="editingDepartment ? 'Actualizar' : 'Guardar'"
      @confirm="saveDepartment"
    >
      <div class="tecnm-form-group">
        <label class="tecnm-label" for="deptDescription">Nombre del Departamento <span class="text-danger">*</span></label>
        <input
          id="deptDescription"
          v-model="modalForm.description"
          type="text"
          class="tecnm-form-control"
          placeholder="Ej. Recursos Humanos"
          maxlength="50"
        />
      </div>
      <div class="tecnm-form-group">
        <label class="tecnm-label" for="deptAreaSelect">Área a la que pertenece</label>
        <select id="deptAreaSelect" v-model="modalForm.areaId" class="tecnm-form-control">
          <option value="">— Sin área asignada —</option>
          <option v-for="a in areaOptions" :key="a.id" :value="String(a.id)">{{ a.description }}</option>
        </select>
        <small class="text-muted">La relación determina el área del departamento y de sus usuarios.</small>
      </div>
    </AppModal>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { http } from '@/api/client'
import AppPageHeader from '@/components/AppPageHeader.vue'
import AppButton from '@/components/AppButton.vue'
import AppSpinner from '@/components/AppSpinner.vue'
import EmptyState from '@/components/EmptyState.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import AppModal from '@/components/AppModal.vue'
import PaginationBar from '@/components/PaginationBar.vue'
import { askConfirmation, showToast } from '@/composables/useAlerts'

interface DepartmentItem {
  id: number
  description: string
  areaId: number | null
  areaDescription: string | null
  status?: boolean
  isActive?: boolean
}

interface AreaOption {
  id: number
  description: string
}

interface Paged<T> {
  items: T[]
  totalItems: number
  totalPages: number
  page: number
}

const departments = ref<DepartmentItem[]>([])
const areaOptions = ref<AreaOption[]>([])
const loading = ref(true)
const submitting = ref(false)

const page = ref(1)
const totalPages = ref(1)
const totalItems = ref(0)

const filters = reactive({
  search: '',
  areaId: '',
  status: ''
})

// Modal reactive state
const showModal = ref(false)
const editingDepartment = ref<DepartmentItem | null>(null)
const modalForm = reactive({
  description: '',
  areaId: ''
})

async function fetchDepartments() {
  loading.value = true
  try {
    const params: Record<string, string | number> = {
      page: page.value,
      pageSize: 10
    }
    if (filters.search.trim()) params.search = filters.search.trim()
    if (filters.areaId) params.areaId = filters.areaId
    if (filters.status) params.status = filters.status

    const res = await http.get<Paged<DepartmentItem>>('/api/Departments', { params })
    departments.value = res.data.items
    totalPages.value = res.data.totalPages || 1
    totalItems.value = res.data.totalItems
  } catch {
    showToast('Error cargando departamentos.', 'error')
  } finally {
    loading.value = false
  }
}

async function fetchAreaOptions() {
  try {
    const res = await http.get<Paged<AreaOption>>('/api/Areas', {
      params: { page: 1, pageSize: 100, status: true }
    })
    areaOptions.value = res.data.items
  } catch {
    showToast('Error cargando catálogo de áreas.', 'error')
  }
}

function applyFilters() {
  page.value = 1
  fetchDepartments()
}

function clearFilters() {
  filters.search = ''
  filters.areaId = ''
  filters.status = ''
  applyFilters()
}

function openCreateModal() {
  editingDepartment.value = null
  modalForm.description = ''
  modalForm.areaId = ''
  showModal.value = true
}

function openEditModal(d: DepartmentItem) {
  editingDepartment.value = d
  modalForm.description = d.description
  modalForm.areaId = d.areaId != null ? String(d.areaId) : ''
  showModal.value = true
}

async function saveDepartment() {
  if (!modalForm.description.trim()) {
    showToast('Debes ingresar un nombre válido para el departamento.', 'warning')
    return
  }

  submitting.value = true
  try {
    const payload = {
      description: modalForm.description.trim(),
      areaId: modalForm.areaId ? Number(modalForm.areaId) : null
    }
    if (editingDepartment.value) {
      await http.put(`/api/Departments/${editingDepartment.value.id}`, {
        id: editingDepartment.value.id,
        ...payload
      })
      showToast('Departamento actualizado correctamente.', 'success')
    } else {
      await http.post('/api/Departments', payload)
      showToast('Departamento registrado correctamente.', 'success')
    }
    showModal.value = false
    fetchDepartments()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo guardar el departamento.', 'error')
  } finally {
    submitting.value = false
  }
}

async function deactivateDepartment(d: DepartmentItem) {
  const ok = await askConfirmation({
    title: '¿Desactivar Departamento?',
    text: `Esta acción marcará inactivo el departamento "${d.description}".`,
    confirmText: 'Sí, desactivar',
    danger: true
  })

  if (ok) {
    try {
      await http.put(`/api/Departments/${d.id}/deactivate`)
      showToast('Departamento inactivado.', 'success')
      fetchDepartments()
    } catch (e: any) {
      showToast(errMessage(e) || 'No se pudo desactivar.', 'error')
    }
  }
}

async function toggleDepartmentActive(d: DepartmentItem) {
  const action = d.isActive ? 'Reemplazado' : 'Vigente'
  const ok = await askConfirmation({
    title: `¿Marcar como ${action}?`,
    text: `El departamento "${d.description}" se marcará como ${action}.`,
    confirmText: `Sí, marcar como ${action}`,
    danger: false
  })
  if (!ok) return
  try {
    await http.put(`/api/Departments/${d.id}/toggle-active`)
    showToast(`Departamento marcado como ${action}.`, 'success')
    fetchDepartments()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo cambiar el estado.', 'error')
  }
}

function errMessage(e: any): string {
  const d = e?.response?.data
  if (typeof d === 'string') return d
  return d?.message || ''
}

onMounted(() => {
  fetchDepartments()
  fetchAreaOptions()
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

.text-center {
  text-align: center;
}

.tecnm-action-btn--toggle {
  color: #d97706;
}

.tecnm-action-btn--toggle:hover {
  color: #ffffff;
  border-color: #d97706;
  background: #d97706;
}
</style>
