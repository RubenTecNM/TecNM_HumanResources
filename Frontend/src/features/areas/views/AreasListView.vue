<template>
  <div>
    <AppPageHeader title="Gestión de Áreas" subtitle="Administra el catálogo de áreas organizacionales" icon="fa-layer-group">
      <template #actions>
        <AppButton variant="primary" icon="fa-plus" @click="openCreateModal">
          Nueva Área
        </AppButton>
      </template>
    </AppPageHeader>

    <!-- Panel de búsqueda estilo TecNM (con footer interno estandarizado) -->
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
              <label class="tecnm-label" for="searchQuery">Nombre del Área</label>
              <input
                id="searchQuery"
                v-model="filters.search"
                type="text"
                class="tecnm-form-control"
                placeholder="Buscar por área..."
              />
            </div>

            <div class="tecnm-form-group">
              <label class="tecnm-label" for="statusFilter">Estado</label>
              <select id="statusFilter" v-model="filters.status" class="tecnm-form-control">
                <option value="">Cualquiera</option>
                <option value="true">Activa</option>
                <option value="false">Inactiva</option>
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

    <AppSpinner v-if="loading" />
    <template v-else>
      <section class="tecnm-card">
        <div class="tecnm-card-header tecnm-d-flex tecnm-justify-between tecnm-items-center">
          <h3 class="tecnm-card-title">Listado de Áreas</h3>
          <span class="count-badge">{{ totalItems }} registro(s)</span>
        </div>

        <div class="tecnm-table-responsive">
          <table class="tecnm-table tecnm-table-striped">
            <thead>
              <tr>
                <th class="text-center" style="width: 80px;">ID</th>
                <th>Nombre del Área</th>
                <th class="text-center" style="width: 170px;">Departamentos</th>
                <th class="text-center" style="width: 140px;">Estado</th>
                <th class="text-center" style="width: 120px;">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="a in areas" :key="a.id">
                <td class="text-center"><strong>#{{ a.id }}</strong></td>
                <td><strong>{{ a.description }}</strong></td>
                <td class="text-center">
                  <span class="dept-badge">
                    <i class="fa fa-sitemap" aria-hidden="true"></i> {{ a.departmentCount ?? 0 }}
                  </span>
                </td>
                <td class="text-center">
                  <StatusBadge :variant="a.status ? 'approved' : 'rejected'">
                    {{ a.status ? 'Activa' : 'Inactiva' }}
                  </StatusBadge>
                </td>
                <td class="text-center">
                  <div class="tecnm-table-actions">
                    <button
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--edit"
                      title="Editar área"
                      @click="openEditModal(a)"
                    >
                      <i class="fa fa-edit" aria-hidden="true"></i>
                    </button>

                    <!-- Borrado Lógico: Desactivar si está activa / Reactivar si está inactiva -->
                    <button
                      v-if="a.status"
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--delete"
                      title="Desactivar área (borrado lógico)"
                      @click="deactivateArea(a)"
                    >
                      <i class="fa fa-trash" aria-hidden="true"></i>
                    </button>
                    <button
                      v-else
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--reactivate"
                      title="Reactivar área"
                      @click="reactivateArea(a)"
                    >
                      <i class="fa fa-undo" aria-hidden="true"></i>
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="!areas.length">
                <td colspan="5">
                  <EmptyState message="No hay áreas registradas">
                    <span>Crea una nueva área para organizar y agrupar los departamentos institucionales</span>
                  </EmptyState>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="areas.length" class="tecnm-card-footer">
          <div class="pagination-wrap">
            <span class="pagination-info">
              <i class="fa fa-list-ol" aria-hidden="true"></i>
              Mostrando página <strong>{{ page }}</strong> de <strong>{{ totalPages }}</strong>
            </span>
            <PaginationBar
              :page="page"
              :total-pages="totalPages"
              @update:page="(v: number) => { page = v; fetchAreas() }"
            />
          </div>
        </div>
      </section>
    </template>

    <!-- Modal Formulario Reactivo (Crear / Editar Área - Guía TecNM 4.2 / 4.3) -->
    <AppModal
      v-model:show="showModal"
      :title="editingArea ? `Editar Área #${editingArea.id}` : 'Nueva Área'"
      :eyebrow="editingArea ? `ÁREA ORGANIZACIONAL #${editingArea.id}` : 'CATÁLOGO DE ÁREAS'"
      header-variant="icon"
      icon="fa-layer-group"
      size="md"
      :loading="submitting"
      :confirm-text="editingArea ? 'Actualizar Área' : 'Guardar Área'"
      @confirm="saveArea"
    >
      <!-- Resumen en edición -->
      <div v-if="editingArea" class="sec tecnm-mb-3">
        <div class="row">
          <i class="fa fa-sitemap ic" aria-hidden="true"></i>
          <div class="tx">
            <strong>Departamentos Vinculados</strong>
            <small>{{ editingArea.departmentCount ?? 0 }} departamento(s) activos asociados a esta área</small>
          </div>
          <span :class="editingArea.status ? 'badge--ok' : 'badge--no'">
            {{ editingArea.status ? 'Activa' : 'Inactiva' }}
          </span>
        </div>
      </div>

      <label class="field">
        <span class="lbl">NOMBRE DEL ÁREA *</span>
        <input
          id="areaDescription"
          v-model="modalForm.description"
          type="text"
          class="in"
          placeholder="Ej. ADMINISTRACIÓN Y FINANZAS, DIRECCIÓN ACADÉMICA..."
          maxlength="100"
          autofocus
          @keyup.enter="saveArea"
        />
        <span class="help">Ingresa el nombre oficial del área organizacional (máx. 100 caracteres).</span>
      </label>
    </AppModal>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { http } from "@/shared/api/client"
import AppPageHeader from '@/shared/components/AppPageHeader.vue'
import AppButton from '@/shared/components/AppButton.vue'
import AppSpinner from '@/shared/components/AppSpinner.vue'
import EmptyState from '@/shared/components/EmptyState.vue'
import StatusBadge from '@/shared/components/StatusBadge.vue'
import AppModal from '@/shared/components/AppModal.vue'
import PaginationBar from '@/shared/components/PaginationBar.vue'
import { askConfirmation, showToast } from '@/shared/composables/useAlerts'

interface AreaItem {
  id: number
  description: string
  status: boolean
  departmentCount?: number
}

interface Paged<T> {
  items: T[]
  totalItems: number
  totalPages: number
  page: number
}

const areas = ref<AreaItem[]>([])
const loading = ref(true)
const submitting = ref(false)

const page = ref(1)
const totalPages = ref(1)
const totalItems = ref(0)

const filters = reactive({
  search: '',
  status: ''
})

// Modal reactive state
const showModal = ref(false)
const editingArea = ref<AreaItem | null>(null)
const modalForm = reactive({
  description: ''
})

async function fetchAreas() {
  loading.value = true
  try {
    const params: Record<string, string | number> = {
      page: page.value,
      pageSize: 10
    }
    if (filters.search.trim()) params.search = filters.search.trim()
    if (filters.status) params.status = filters.status

    const res = await http.get<Paged<AreaItem>>('/api/Areas', { params })
    areas.value = res.data.items
    totalPages.value = res.data.totalPages || 1
    totalItems.value = res.data.totalItems
  } catch {
    showToast('Error cargando áreas organizacionales.', 'error')
  } finally {
    loading.value = false
  }
}

function applyFilters() {
  page.value = 1
  fetchAreas()
}

function clearFilters() {
  filters.search = ''
  filters.status = ''
  applyFilters()
}

function openCreateModal() {
  editingArea.value = null
  modalForm.description = ''
  showModal.value = true
}

function openEditModal(a: AreaItem) {
  editingArea.value = a
  modalForm.description = a.description
  showModal.value = true
}

async function saveArea() {
  const desc = modalForm.description.trim()
  if (!desc) {
    showToast('Debes ingresar un nombre válido para el área.', 'warning')
    return
  }

  submitting.value = true
  try {
    if (editingArea.value) {
      await http.put(`/api/Areas/${editingArea.value.id}`, {
        id: editingArea.value.id,
        description: desc
      })
      showToast('Área actualizada correctamente.', 'success')
    } else {
      await http.post('/api/Areas', {
        description: desc
      })
      showToast('Área registrada correctamente.', 'success')
    }
    showModal.value = false
    fetchAreas()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo guardar el área.', 'error')
  } finally {
    submitting.value = false
  }
}

async function deactivateArea(a: AreaItem) {
  const hasDepts = (a.departmentCount && a.departmentCount > 0)
  const message = hasDepts
    ? `Atención: Esta área tiene ${a.departmentCount} departamento(s) activos vinculados. Al desactivarla, permanecerán en la base de datos pero el área figurará como Inactiva.`
    : `Esta acción marcará inactiva el área "${a.description}". Podrás reactivarla en cualquier momento.`

  const ok = await askConfirmation({
    title: '¿Desactivar Área?',
    text: message,
    confirmText: 'Sí, desactivar',
    danger: true
  })

  if (!ok) return
  try {
    await http.put(`/api/Areas/${a.id}/deactivate`)
    showToast('Área desactivada exitosamente.', 'success')
    fetchAreas()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo desactivar el área.', 'error')
  }
}

async function reactivateArea(a: AreaItem) {
  const ok = await askConfirmation({
    title: '¿Reactivar Área?',
    text: `El área "${a.description}" volverá a estar activa y disponible para vincular departamentos y usuarios.`,
    confirmText: 'Sí, reactivar',
    danger: false
  })

  if (!ok) return
  try {
    await http.put(`/api/Areas/${a.id}/activate`)
    showToast('Área reactivada exitosamente.', 'success')
    fetchAreas()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo reactivar el área.', 'error')
  }
}

function errMessage(e: any): string {
  const d = e?.response?.data
  if (typeof d === 'string') return d
  return d?.message || d?.error || ''
}

onMounted(() => {
  fetchAreas()
})
</script>

<style scoped>
.filter-title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin: 0;
}

.count-badge {
  font-size: 0.82rem;
  font-weight: 600;
  color: var(--tecnm-accent-text);
  background: var(--tecnm-soft-hover);
  padding: 0.2rem 0.6rem;
  border-radius: 9999px;
}

.dept-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--tecnm-text-secondary);
  background: var(--tecnm-soft-hover);
  padding: 0.2rem 0.6rem;
  border-radius: var(--tecnm-radius-sm);
}

.text-center {
  text-align: center;
}
</style>
