<template>
  <div>
    <AppPageHeader title="Gestión de Áreas" subtitle="Administra el catálogo de áreas organizacionales" icon="fa-folder">
      <template #actions>
        <AppButton variant="primary" icon="fa-plus" @click="openCreateModal">
          Nuevo Área
        </AppButton>
      </template>
    </AppPageHeader>

    <AppSpinner v-if="loading" />
    <template v-else>
      <section class="tecnm-card">
        <div class="tecnm-card-header tecnm-card-header--premium">
          <h4 class="filter-title">Listado de Áreas</h4>
          <span class="count-badge">{{ areas.length }} registro(s)</span>
        </div>

        <div class="tecnm-table-responsive">
          <table class="tecnm-table tecnm-table-striped">
            <thead>
              <tr>
                <th class="text-center">ID</th>
                <th class="text-center">Descripción / Nombre del Área</th>
                <th class="text-center">Estado</th>
                <th class="text-center">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="a in areas" :key="a.id">
                <td class="text-center"><strong>#{{ a.id }}</strong></td>
                <td><strong>{{ a.description }}</strong></td>
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
                      title="Editar nombre"
                      @click="openEditModal(a)"
                    >
                      <i class="fa fa-edit" aria-hidden="true"></i>
                    </button>
                    <button
                      v-if="a.status"
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--delete"
                      title="Desactivar área"
                      @click="deactivateArea(a)"
                    >
                      <i class="fa fa-trash" aria-hidden="true"></i>
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="!areas.length">
                <td colspan="4"><EmptyState message="No hay áreas registradas"><span>Crea un nuevo área para organizar los departamentos</span></EmptyState></td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>

    <AppModal
      v-model:show="showModal"
      :title="editingArea ? `Editar Área #${editingArea.id}` : 'Nueva Área'"
      icon="fa-folder"
      :loading="submitting"
      :confirm-text="editingArea ? 'Actualizar' : 'Guardar'"
      @confirm="saveArea"
    >
      <div class="tecnm-form-group">
        <label class="tecnm-label" for="areaDescription">Nombre del Área <span class="text-danger">*</span></label>
        <input
          id="areaDescription"
          v-model="modalForm.description"
          type="text"
          class="tecnm-form-control"
          placeholder="Ej. Dirección, Recursos Humanos..."
          maxlength="100"
          required
        />
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
import { askConfirmation, showToast } from '@/composables/useAlerts'

interface AreaItem {
  id: number
  description: string
  status?: boolean
}

const areas = ref<AreaItem[]>([])
const loading = ref(true)
const submitting = ref(false)

const showModal = ref(false)
const editingArea = ref<AreaItem | null>(null)
const modalForm = reactive({
  description: ''
})

async function fetchAreas() {
  loading.value = true
  try {
    const res = await http.get('/api/Areas')
    areas.value = res.data.items || res.data
  } catch {
    showToast('Error cargando áreas.', 'error')
  } finally {
    loading.value = false
  }
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
  if (!modalForm.description.trim()) {
    showToast('Debes ingresar un nombre válido para el área.', 'warning')
    return
  }

  submitting.value = true
  try {
    if (editingArea.value) {
      await http.put(`/api/Areas/${editingArea.value.id}`, {
        id: editingArea.value.id,
        description: modalForm.description.trim()
      })
      showToast('Área actualizada correctamente.', 'success')
    } else {
      await http.post('/api/Areas', {
        description: modalForm.description.trim()
      })
      showToast('Área registrada correctamente.', 'success')
    }
    showModal.value = false
    fetchAreas()
  } catch (e: any) {
    showToast(e.response?.data?.message || e.response?.data || 'No se pudo guardar el área.', 'error')
  } finally {
    submitting.value = false
  }
}

async function deactivateArea(a: AreaItem) {
  const ok = await askConfirmation({
    title: '¿Desactivar Área?',
    text: `Esta acción marcará inactivo el área "${a.description}".`,
    confirmText: 'Sí, desactivar',
    danger: true
  })

  if (ok) {
    try {
      await http.put(`/api/Areas/${a.id}/deactivate`)
      showToast('Área inactivada.', 'success')
      fetchAreas()
    } catch (e: any) {
      showToast(e.response?.data?.message || e.response?.data || 'No se pudo desactivar.', 'error')
    }
  }
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

.text-center {
  text-align: center;
}
</style>
