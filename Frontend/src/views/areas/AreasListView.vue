<template>
  <div>
    <AppPageHeader title="Gestión de Áreas" subtitle="Administra el catálogo de áreas organizacionales" icon="fa-folder">
      <template #actions>
        <AppButton variant="primary" icon="fa-plus" @click="openCreateModal">
          Nuevo Área
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
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--toggle"
                      :title="a.isActive ? 'Marcar como Inactiva' : 'Marcar como Vigente'"
                      @click="toggleAreaActive(a)"
                    >
                      <i :class="a.isActive ? 'fa fa-toggle-on' : 'fa fa-toggle-off'" aria-hidden="true"></i>
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

    interface AreaItem {
      id: number
      description: string
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

    const areas = ref<AreaItem[]>([])
    const loading = ref(true)
    const submitting = ref(false)

    const filters = reactive({
      search: ''
      , status: ''
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
        const params = new URLSearchParams()
        if (filters.search) params.append('search', filters.search)
        if (filters.status) params.append('status', filters.status)

        const res = await http.get(`/api/Areas?${params.toString()}`)
        areas.value = res.data.items || res.data
      } catch (e) {
        showToast('Error cargando áreas.', 'error')
      } finally {
        loading.value = false
      }
    }

    function applyFilters() {
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
        showToast(e.response?.data?.message || 'No se pudo guardar el área.', 'error')
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
          showToast(e.response?.data?.message || 'No se pudo desactivar.', 'error')
        }
      }
    }

    async function toggleAreaActive(a: AreaItem) {
      const action = a.isActive ? 'Inactiva' : 'Vigente'
      const ok = await askConfirmation({
        title: `¿Marcar como ${action}?`,
        text: `El área "${a.description}" se marcará como ${action}.`,
        confirmText: `Sí, marcar como ${action}`,
        danger: false
      })
      if (!ok) return
      try {
        await http.put(`/api/Areas/${a.id}/toggle-active`)
        showToast(`Área marcada como ${action}.`, 'success')
        fetchAreas()
      } catch (e: any) {
        showToast(e.response?.data?.message || 'No se pudo cambiar el estado.', 'error')
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
