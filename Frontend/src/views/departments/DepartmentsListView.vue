<template>
  <div>
    <AppPageHeader title="Gestión de Departamentos" subtitle="Administra el catálogo de áreas y departamentos organizacionales" icon="fa-sitemap">
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
          <h4 class="filter-title">Listado de Departamentos</h4>
          <span class="count-badge">{{ departments.length }} registro(s)</span>
        </div>

        <div class="tecnm-table-responsive">
          <table class="tecnm-table tecnm-table-striped">
            <thead>
              <tr>
                <th class="text-center">ID</th>
                <th class="text-center">Descripción / Nombre del Departamento</th>
                <th class="text-center">Estado</th>
                <th class="text-center">Creación</th>
                <th class="text-center">Modificación</th>
                <th class="text-center">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="d in departments" :key="d.id">
                <td class="text-center"><strong>#{{ d.id }}</strong></td>
                <td>{{ d.areaDescription || '—' }}</td>
                <td><strong>{{ d.description }}</strong></td>
                <td class="text-center">
                  <StatusBadge :variant="d.status ? 'approved' : 'rejected'">
                    {{ d.status ? 'Activo' : 'Inactivo' }}
                  </StatusBadge>
                </td>
                <td class="text-center">
                  <div class="datetime-cell">
                    <span class="datetime-date">{{ formatDateCell(d.createdAt) }}</span>
                    <span class="datetime-time">{{ formatTimeCell(d.createdAt) }}</span>
                  </div>
                </td>
                <td class="text-center">
                  <div v-if="d.updatedAt" class="datetime-cell">
                    <span class="datetime-date">{{ formatDateCell(d.updatedAt) }}</span>
                    <span class="datetime-time">{{ formatTimeCell(d.updatedAt) }}</span>
                  </div>
                  <span v-else class="text-muted small">Sin modificar</span>
                </td>
                <td class="text-center">
                  <div class="tecnm-table-actions">
                    <button
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--edit"
                      title="Editar nombre"
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
          @keyup.enter="saveDepartment"
        /> 
        <small class="text-muted">Departamento pertenece al Área</small>
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
import { formatTecNMDate } from '@/composables/useDates'
import { askConfirmation, showToast } from '@/composables/useAlerts'

interface DepartmentItem {
  id: number
  description: string
  areaId: number
  areaDescription: string
  status?: boolean
  isActive?: boolean
  createdAt?: string
  updatedAt?: string
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

const departments = ref<DepartmentItem[]>([])
const loading = ref(true)
const submitting = ref(false)

const filters = reactive({
  search: '',
  status: ''
})

// Modal reactive state
const showModal = ref(false)
const editingDepartment = ref<DepartmentItem | null>(null)
const modalForm = reactive({
  description: ''
})

async function fetchDepartments() {
  loading.value = true
  try {
    const params = new URLSearchParams()
    if (filters.search) params.append('search', filters.search)
    if (filters.status) params.append('status', filters.status)

    const res = await http.get(`/api/Departments?${params.toString()}`)
    departments.value = res.data.items || res.data
    // Map areaId to description for display
    const areaNames: Record<number, string> = {1: 'ADMINISTRACIÓN Y FINANZAS', 2: 'DIRECCIÓN GENERAL', 3: 'PLANEACIÓN Y VINCULACIÓN', 4: 'DIRECCIÓN ACADÉMICA'}
    departments.value = departments.value.map((d: any) => ({ ...d, areaDescription: d.areaId ? areaNames[d.areaId] || '' : '—' }))
  } catch (e) {
    showToast('Error cargando departamentos.', 'error')
  } finally {
    loading.value = false
  }
}

function applyFilters() {
  fetchDepartments()
}

function clearFilters() {
  filters.search = ''
  filters.status = ''
  applyFilters()
}

function openCreateModal() {
  editingDepartment.value = null
  modalForm.description = ''
  showModal.value = true
}

function openEditModal(d: DepartmentItem) {
  editingDepartment.value = d
  modalForm.description = d.description
  showModal.value = true
}

async function saveDepartment() {
  if (!modalForm.description.trim()) {
    showToast('Debes ingresar un nombre válido para el departamento.', 'warning')
    return
  }

  submitting.value = true
  try {
    if (editingDepartment.value) {
      await http.put(`/api/Departments/${editingDepartment.value.id}`, {
        id: editingDepartment.value.id,
        description: modalForm.description.trim()
      })
      showToast('Departamento actualizado correctamente.', 'success')
    } else {
      await http.post('/api/Departments', {
        description: modalForm.description.trim()
      })
      showToast('Departamento registrado correctamente.', 'success')
    }
    showModal.value = false
    fetchDepartments()
  } catch (e: any) {
    showToast(e.response?.data?.message || 'No se pudo guardar el departamento.', 'error')
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
      showToast(e.response?.data?.message || 'No se pudo desactivar.', 'error')
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
    showToast(e.response?.data?.message || 'No se pudo cambiar el estado.', 'error')
  }
}

onMounted(() => {
  fetchDepartments()
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

.tecnm-action-btn--toggle {
  color: #d97706;
}

.tecnm-action-btn--toggle:hover {
  color: #ffffff;
  border-color: #d97706;
  background: #d97706;
}

@media (max-width: 640px) {
  .filter-grid {
    grid-template-columns: 1fr;
  }
}
</style>
