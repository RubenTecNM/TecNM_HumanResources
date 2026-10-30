<template>
  <div>
    <AppPageHeader
      title="Gestión de Puestos"
      subtitle="Administra el catálogo tabular de puestos y cargos institucionales"
      icon="fa-id-badge"
    >
      <template #actions>
        <AppButton variant="primary" icon="fa-plus" @click="openCreateModal">
          Nuevo Puesto
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
              <label class="tecnm-label" for="searchQuery">Nombre o Clave Tabular</label>
              <input
                id="searchQuery"
                v-model="filters.search"
                type="text"
                class="tecnm-form-control"
                placeholder="Buscar por puesto o clave tabular..."
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
          <h3 class="tecnm-card-title">Listado de Puestos</h3>
          <span class="count-badge">{{ totalItems }} registro(s)</span>
        </div>

        <div class="tecnm-table-responsive">
          <table class="tecnm-table tecnm-table-striped">
            <thead>
              <tr>
                <th class="text-center" style="width: 70px;">ID</th>
                <th style="width: 150px;">Clave Tabular</th>
                <th>Descripción / Nombre del Puesto</th>
                <th class="text-center" style="width: 160px;">Colaboradores</th>
                <th class="text-center" style="width: 130px;">Estado</th>
                <th class="text-center" style="width: 120px;">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="p in positions" :key="p.id">
                <td class="text-center"><strong>#{{ p.id }}</strong></td>
                <td>
                  <span class="tabular-code">
                    <i class="fa fa-tag" aria-hidden="true"></i> {{ p.tabularCode }}
                  </span>
                </td>
                <td><strong>{{ p.description }}</strong></td>
                <td class="text-center">
                  <span class="collab-badge">
                    <i class="fa fa-users" aria-hidden="true"></i> {{ p.userCount ?? 0 }}
                  </span>
                </td>
                <td class="text-center">
                  <StatusBadge :variant="p.status ? 'approved' : 'rejected'">
                    {{ p.status ? 'Activo' : 'Inactivo' }}
                  </StatusBadge>
                </td>
                <td class="text-center">
                  <div class="tecnm-table-actions">
                    <button
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--edit"
                      title="Editar puesto"
                      @click="openEditModal(p)"
                    >
                      <i class="fa fa-edit" aria-hidden="true"></i>
                    </button>

                    <!-- Borrado Lógico: Desactivar si activo / Reactivar si inactivo -->
                    <button
                      v-if="p.status"
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--delete"
                      title="Desactivar puesto (borrado lógico)"
                      @click="deactivatePosition(p)"
                    >
                      <i class="fa fa-trash" aria-hidden="true"></i>
                    </button>
                    <button
                      v-else
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--reactivate"
                      title="Reactivar puesto"
                      @click="reactivatePosition(p)"
                    >
                      <i class="fa fa-undo" aria-hidden="true"></i>
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="!positions.length">
                <td colspan="6">
                  <EmptyState message="No hay puestos registrados">
                    <span>Crea un nuevo puesto tabular para asignarlo a colaboradores</span>
                  </EmptyState>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="positions.length" class="tecnm-card-footer">
          <div class="pagination-wrap">
            <span class="pagination-info">
              <i class="fa fa-list-ol" aria-hidden="true"></i>
              Mostrando página <strong>{{ page }}</strong> de <strong>{{ totalPages }}</strong>
            </span>
            <PaginationBar
              :page="page"
              :total-pages="totalPages"
              @update:page="(v: number) => { page = v; fetchPositions() }"
            />
          </div>
        </div>
      </section>
    </template>

    <!-- Modal Formulario Reactivo (Crear / Editar Puesto - Guía TecNM 4.2 / 4.3) -->
    <AppModal
      v-model:show="showModal"
      :title="editingPosition ? `Editar Puesto #${editingPosition.id}` : 'Nuevo Puesto'"
      :eyebrow="editingPosition ? `PUESTO TABULAR #${editingPosition.id}` : 'CATÁLOGO DE PUESTOS'"
      header-variant="icon"
      icon="fa-id-badge"
      size="md"
      :loading="submitting"
      :confirm-text="editingPosition ? 'Actualizar Puesto' : 'Guardar Puesto'"
      @confirm="savePosition"
    >
      <div v-if="editingPosition" class="sec tecnm-mb-3">
        <div class="row">
          <i class="fa fa-users ic" aria-hidden="true"></i>
          <div class="tx">
            <strong>Colaboradores Asignados</strong>
            <small>{{ editingPosition.userCount ?? 0 }} colaborador(es) activos en esta plaza</small>
          </div>
          <span :class="editingPosition.status ? 'badge--ok' : 'badge--no'">
            {{ editingPosition.status ? 'Activo' : 'Inactivo' }}
          </span>
        </div>
      </div>

      <label class="field tecnm-mb-3">
        <span class="lbl">CLAVE TABULAR *</span>
        <input
          id="posTabularCode"
          v-model="modalForm.tabularCode"
          type="text"
          class="in"
          placeholder="Ej. A03004, CF12027, E13001..."
          maxlength="10"
          autofocus
        />
        <span class="help">Clave tabular oficial registrada ante el TecNM (máx. 10 caracteres).</span>
      </label>

      <label class="field">
        <span class="lbl">NOMBRE / DESCRIPCIÓN DEL PUESTO *</span>
        <input
          id="posDescription"
          v-model="modalForm.description"
          type="text"
          class="in"
          placeholder="Ej. ALMACENISTA, INGENIERO EN SISTEMAS..."
          maxlength="50"
          @keyup.enter="savePosition"
        />
        <span class="help">Denominación oficial del cargo o función (máx. 50 caracteres).</span>
      </label>
    </AppModal>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { http } from '@/shared/api/client'
import AppPageHeader from '@/shared/components/AppPageHeader.vue'
import AppButton from '@/shared/components/AppButton.vue'
import AppSpinner from '@/shared/components/AppSpinner.vue'
import EmptyState from '@/shared/components/EmptyState.vue'
import StatusBadge from '@/shared/components/StatusBadge.vue'
import AppModal from '@/shared/components/AppModal.vue'
import PaginationBar from '@/shared/components/PaginationBar.vue'
import { askConfirmation, showToast } from '@/shared/composables/useAlerts'

interface PositionItem {
  id: number
  tabularCode: string
  description: string
  status: boolean
  userCount?: number
}

interface Paged<T> {
  items: T[]
  totalItems: number
  totalPages: number
  page: number
}

const positions = ref<PositionItem[]>([])
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
const editingPosition = ref<PositionItem | null>(null)
const modalForm = reactive({
  tabularCode: '',
  description: ''
})

async function fetchPositions() {
  loading.value = true
  try {
    const params: Record<string, string | number> = {
      page: page.value,
      pageSize: 10
    }
    if (filters.search.trim()) params.search = filters.search.trim()
    if (filters.status) params.status = filters.status
    params.all = 'true' // Permite ver activos e inactivos en la vista de gestión

    const res = await http.get<Paged<PositionItem>>('/api/Positions', { params })
    positions.value = res.data.items || []
    totalPages.value = res.data.totalPages || 1
    totalItems.value = res.data.totalItems || 0
  } catch {
    showToast('Error cargando puestos institucionales.', 'error')
  } finally {
    loading.value = false
  }
}

function applyFilters() {
  page.value = 1
  fetchPositions()
}

function clearFilters() {
  filters.search = ''
  filters.status = ''
  applyFilters()
}

function openCreateModal() {
  editingPosition.value = null
  modalForm.tabularCode = ''
  modalForm.description = ''
  showModal.value = true
}

function openEditModal(p: PositionItem) {
  editingPosition.value = p
  modalForm.tabularCode = p.tabularCode
  modalForm.description = p.description
  showModal.value = true
}

async function savePosition() {
  const code = modalForm.tabularCode.trim().toUpperCase()
  const desc = modalForm.description.trim()

  if (!code) {
    showToast('La clave tabular es obligatoria.', 'warning')
    return
  }
  if (!desc) {
    showToast('La descripción del puesto es obligatoria.', 'warning')
    return
  }

  submitting.value = true
  try {
    const payload = {
      tabularCode: code,
      description: desc
    }

    if (editingPosition.value) {
      await http.put(`/api/Positions/${editingPosition.value.id}`, {
        id: editingPosition.value.id,
        ...payload
      })
      showToast('Puesto actualizado correctamente.', 'success')
    } else {
      await http.post('/api/Positions', payload)
      showToast('Puesto registrado correctamente.', 'success')
    }
    showModal.value = false
    fetchPositions()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo guardar el puesto.', 'error')
  } finally {
    submitting.value = false
  }
}

async function deactivatePosition(p: PositionItem) {
  const hasUsers = (p.userCount && p.userCount > 0)
  const message = hasUsers
    ? `Atención: Este puesto tiene ${p.userCount} colaborador(es) activos asignados. Al desactivarlo, permanecerán en la base de datos pero el puesto figurará como Inactivo.`
    : `Esta acción marcará inactivo el puesto "${p.description}". Podrás reactivarlo en cualquier momento.`

  const ok = await askConfirmation({
    title: '¿Desactivar Puesto?',
    text: message,
    confirmText: 'Sí, desactivar',
    danger: true
  })

  if (!ok) return
  try {
    await http.put(`/api/Positions/${p.id}/deactivate`)
    showToast('Puesto desactivado exitosamente.', 'success')
    fetchPositions()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo desactivar el puesto.', 'error')
  }
}

async function reactivatePosition(p: PositionItem) {
  const ok = await askConfirmation({
    title: '¿Reactivar Puesto?',
    text: `El puesto "${p.description}" (${p.tabularCode}) volverá a estar activo y elegible para asignación.`,
    confirmText: 'Sí, reactivar',
    danger: false
  })

  if (!ok) return
  try {
    await http.put(`/api/Positions/${p.id}/activate`)
    showToast('Puesto reactivado exitosamente.', 'success')
    fetchPositions()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo reactivar el puesto.', 'error')
  }
}

function errMessage(e: any): string {
  const d = e?.response?.data
  if (typeof d === 'string') return d
  return d?.message || d?.error || ''
}

onMounted(() => {
  fetchPositions()
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

.tabular-code {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  font-size: 0.82rem;
  font-weight: 700;
  color: var(--tecnm-primary, #1B396A);
  background: rgba(27, 57, 106, 0.08);
  padding: 0.15rem 0.5rem;
  border-radius: 4px;
}

.collab-badge {
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
