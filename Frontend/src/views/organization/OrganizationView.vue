<template>
  <div>
    <AppPageHeader title="Áreas y Departamentos" subtitle="Administra el catálogo organizacional y sus relaciones" icon="fa-object-group">
      <template #actions>
        <AppButton variant="primary" icon="fa-plus" @click="openAreaModal(null)">
          Nuevo Área
        </AppButton>
        <AppButton variant="secondary" icon="fa-plus" @click="openDeptModal(null)">
          Nuevo Departamento
        </AppButton>
      </template>
    </AppPageHeader>

    <!-- ════════════════ ÁREAS ════════════════ -->
    <section class="tecnm-card tecnm-mb-4">
      <div class="tecnm-card-header tecnm-card-header--premium">
        <h4 class="section-title">
          <i class="fa fa-folder-open" aria-hidden="true"></i> Áreas
          <span class="count-badge">{{ areasPaging.totalItems }} registro(s)</span>
        </h4>
      </div>
      <div class="tecnm-card-body">
        <form class="filter-grid" @submit.prevent="areasPage = 1; fetchAreas()">
          <div class="tecnm-form-group">
            <label class="tecnm-label" for="areaSearch">Buscar área</label>
            <input id="areaSearch" v-model="areaFilters.search" type="text" class="tecnm-form-control" placeholder="Nombre del área..." />
          </div>
          <div class="tecnm-form-group filter-actions">
            <AppButton type="submit" variant="primary" icon="fa-search">Buscar</AppButton>
            <AppButton type="button" variant="secondary" icon="fa-undo" @click="clearAreaFilters">Limpiar</AppButton>
          </div>
        </form>
      </div>

      <AppSpinner v-if="areasLoading" />
      <template v-else>
        <div class="tecnm-table-responsive">
          <table class="tecnm-table tecnm-table-striped">
            <thead>
              <tr>
                <th class="text-center">ID</th>
                <th class="text-center">Descripción</th>
                <th class="text-center">Departamentos</th>
                <th class="text-center">Estado</th>
                <th class="text-center">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="a in areas" :key="a.id">
                <td class="text-center"><strong>#{{ a.id }}</strong></td>
                <td><strong>{{ a.description }}</strong></td>
                <td class="text-center">{{ a.departmentCount }}</td>
                <td class="text-center">
                  <StatusBadge :variant="a.status ? 'approved' : 'rejected'">
                    {{ a.status ? 'Activa' : 'Inactiva' }}
                  </StatusBadge>
                </td>
                <td class="text-center">
                  <div class="tecnm-table-actions">
                    <button type="button" class="tecnm-action-btn tecnm-action-btn--edit" title="Editar nombre" @click="openAreaModal(a)">
                      <i class="fa fa-edit" aria-hidden="true"></i>
                    </button>
                    <button v-if="a.status" type="button" class="tecnm-action-btn tecnm-action-btn--delete" title="Desactivar área" @click="deactivateArea(a)">
                      <i class="fa fa-trash" aria-hidden="true"></i>
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="!areas.length">
                <td colspan="5"><EmptyState message="No hay áreas registradas"><span>Crea un área para agrupar departamentos</span></EmptyState></td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="areas.length" class="tecnm-card-footer">
          <div class="pagination-wrap">
            <span class="pagination-info">
              <i class="fa fa-list-ol" aria-hidden="true"></i>
              Página <strong>{{ areasPaging.page }}</strong> de <strong>{{ areasPaging.totalPages }}</strong>
            </span>
            <PaginationBar :page="areasPaging.page" :total-pages="areasPaging.totalPages" @update:page="(v: number) => { areasPaging.page = v; fetchAreas() }" />
          </div>
        </div>
      </template>
    </section>

    <!-- ════════════════ DEPARTAMENTOS ════════════════ -->
    <section class="tecnm-card">
      <div class="tecnm-card-header tecnm-card-header--premium">
        <h4 class="section-title">
          <i class="fa fa-sitemap" aria-hidden="true"></i> Departamentos
          <span class="count-badge">{{ deptsPaging.totalItems }} registro(s)</span>
        </h4>
      </div>
      <div class="tecnm-card-body">
        <form class="filter-grid" @submit.prevent="deptsPaging.page = 1; fetchDepts()">
          <div class="tecnm-form-group">
            <label class="tecnm-label" for="deptSearch">Buscar departamento</label>
            <input id="deptSearch" v-model="deptFilters.search" type="text" class="tecnm-form-control" placeholder="Nombre del departamento..." />
          </div>
          <div class="tecnm-form-group">
            <label class="tecnm-label" for="deptAreaFilter">Filtrar por área</label>
            <select id="deptAreaFilter" v-model="deptFilters.areaId" class="tecnm-form-control" @change="deptsPaging.page = 1; fetchDepts()">
              <option value="">Todas las áreas</option>
              <option v-for="a in areaOptions" :key="a.id" :value="String(a.id)">{{ a.description }}</option>
            </select>
          </div>
          <div class="tecnm-form-group filter-actions">
            <AppButton type="submit" variant="primary" icon="fa-search">Buscar</AppButton>
            <AppButton type="button" variant="secondary" icon="fa-undo" @click="clearDeptFilters">Limpiar</AppButton>
          </div>
        </form>
      </div>

      <AppSpinner v-if="deptsLoading" />
      <template v-else>
        <div class="tecnm-table-responsive">
          <table class="tecnm-table tecnm-table-striped">
            <thead>
              <tr>
                <th class="text-center">ID</th>
                <th class="text-center">Descripción</th>
                <th class="text-center">Área</th>
                <th class="text-center">Vigencia</th>
                <th class="text-center">Estado</th>
                <th class="text-center">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="d in depts" :key="d.id">
                <td class="text-center"><strong>#{{ d.id }}</strong></td>
                <td><strong>{{ d.description }}</strong></td>
                <td>{{ d.areaDescription || '—' }}</td>
                <td class="text-center">
                  <StatusBadge :variant="d.isActive ? 'approved' : 'info'">
                    {{ d.isActive ? 'Vigente' : 'Reemplazado' }}
                  </StatusBadge>
                </td>
                <td class="text-center">
                  <StatusBadge :variant="d.status ? 'approved' : 'rejected'">
                    {{ d.status ? 'Activo' : 'Inactivo' }}
                  </StatusBadge>
                </td>
                <td class="text-center">
                  <div class="tecnm-table-actions">
                    <button type="button" class="tecnm-action-btn tecnm-action-btn--edit" title="Editar / asignar área" @click="openDeptModal(d)">
                      <i class="fa fa-edit" aria-hidden="true"></i>
                    </button>
                    <button
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--toggle"
                      :title="d.isActive ? 'Marcar como Reemplazado' : 'Marcar como Vigente'"
                      @click="toggleDeptActive(d)"
                    >
                      <i :class="d.isActive ? 'fa fa-toggle-on' : 'fa fa-toggle-off'" aria-hidden="true"></i>
                    </button>
                    <button v-if="d.status" type="button" class="tecnm-action-btn tecnm-action-btn--delete" title="Desactivar departamento" @click="deactivateDept(d)">
                      <i class="fa fa-trash" aria-hidden="true"></i>
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="!depts.length">
                <td colspan="6"><EmptyState message="No hay departamentos registrados"><span>Crea un departamento y asígnalo a un área</span></EmptyState></td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="depts.length" class="tecnm-card-footer">
          <div class="pagination-wrap">
            <span class="pagination-info">
              <i class="fa fa-list-ol" aria-hidden="true"></i>
              Página <strong>{{ deptsPaging.page }}</strong> de <strong>{{ deptsPaging.totalPages }}</strong>
            </span>
            <PaginationBar :page="deptsPaging.page" :total-pages="deptsPaging.totalPages" @update:page="(v: number) => { deptsPaging.page = v; fetchDepts() }" />
          </div>
        </div>
      </template>
    </section>

    <!-- Modal Área -->
    <AppModal
      v-model:show="areaModal.show"
      :title="areaModal.editing ? `Editar Área #${areaModal.editing.id}` : 'Nueva Área'"
      icon="fa-folder"
      :loading="areaModal.saving"
      :confirm-text="areaModal.editing ? 'Actualizar' : 'Guardar'"
      @confirm="saveArea"
    >
      <div class="tecnm-form-group">
        <label class="tecnm-label" for="areaDescription">Nombre del Área <span class="text-danger">*</span></label>
        <input id="areaDescription" v-model="areaForm.description" type="text" class="tecnm-form-control" placeholder="Ej. Dirección, Recursos Humanos..." maxlength="100" required />
      </div>
    </AppModal>

    <!-- Modal Departamento -->
    <AppModal
      v-model:show="deptModal.show"
      :title="deptModal.editing ? `Editar Departamento #${deptModal.editing.id}` : 'Nuevo Departamento'"
      icon="fa-sitemap"
      :loading="deptModal.saving"
      :confirm-text="deptModal.editing ? 'Actualizar' : 'Guardar'"
      @confirm="saveDept"
    >
      <div class="tecnm-form-group">
        <label class="tecnm-label" for="deptDescription">Nombre del Departamento <span class="text-danger">*</span></label>
        <input id="deptDescription" v-model="deptForm.description" type="text" class="tecnm-form-control" placeholder="Ej. Recursos Humanos..." maxlength="50" required />
      </div>
      <div class="tecnm-form-group">
        <label class="tecnm-label" for="deptAreaSelect">Área a la que pertenece</label>
        <select id="deptAreaSelect" v-model="deptForm.areaId" class="tecnm-form-control">
          <option value="">— Sin área asignada —</option>
          <option v-for="a in areaOptions" :key="a.id" :value="String(a.id)">{{ a.description }}</option>
        </select>
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

interface AreaItem {
  id: number
  description: string
  status: boolean
  departmentCount?: number
}

interface DeptItem {
  id: number
  description: string
  areaId: number | null
  areaDescription: string | null
  status: boolean
  isActive: boolean
}

interface Paged<T> {
  items: T[]
  totalItems: number
  totalPages: number
  page: number
}

const areas = ref<AreaItem[]>([])
const areaOptions = ref<AreaItem[]>([])
const areasLoading = ref(true)
const areasPaging = reactive({ page: 1, totalPages: 1, totalItems: 0 })
const areaFilters = reactive({ search: '' })

const depts = ref<DeptItem[]>([])
const deptsLoading = ref(true)
const deptsPaging = reactive({ page: 1, totalPages: 1, totalItems: 0 })
const deptFilters = reactive({ search: '', areaId: '' })

const areaModal = reactive<{ show: boolean; saving: boolean; editing: AreaItem | null }>({
  show: false,
  saving: false,
  editing: null
})
const areaForm = reactive({ description: '' })

const deptModal = reactive<{ show: boolean; saving: boolean; editing: DeptItem | null }>({
  show: false,
  saving: false,
  editing: null
})
const deptForm = reactive({ description: '', areaId: '' })

async function fetchAreas() {
  areasLoading.value = true
  try {
    const params = new URLSearchParams({
      page: String(areasPaging.page),
      pageSize: '10'
    })
    if (areaFilters.search.trim()) params.append('search', areaFilters.search.trim())
    const res = await http.get<Paged<AreaItem>>('/api/Areas', { params })
    areas.value = res.data.items
    areasPaging.totalPages = res.data.totalPages || 1
    areasPaging.totalItems = res.data.totalItems
  } catch {
    showToast('Error cargando áreas.', 'error')
  } finally {
    areasLoading.value = false
  }
}

async function fetchAreaOptions() {
  try {
    const res = await http.get<Paged<AreaItem>>('/api/Areas', {
      params: { page: 1, pageSize: 100, status: true }
    })
    areaOptions.value = res.data.items
  } catch {
    showToast('Error cargando catálogo de áreas.', 'error')
  }
}

async function fetchDepts() {
  deptsLoading.value = true
  try {
    const params = new URLSearchParams({
      page: String(deptsPaging.page),
      pageSize: '10'
    })
    if (deptFilters.search.trim()) params.append('search', deptFilters.search.trim())
    if (deptFilters.areaId) params.append('areaId', deptFilters.areaId)
    const res = await http.get<Paged<DeptItem>>('/api/Departments', { params })
    depts.value = res.data.items
    deptsPaging.totalPages = res.data.totalPages || 1
    deptsPaging.totalItems = res.data.totalItems
  } catch {
    showToast('Error cargando departamentos.', 'error')
  } finally {
    deptsLoading.value = false
  }
}

function clearAreaFilters() {
  areaFilters.search = ''
  areasPaging.page = 1
  fetchAreas()
}

function clearDeptFilters() {
  deptFilters.search = ''
  deptFilters.areaId = ''
  deptsPaging.page = 1
  fetchDepts()
}

// ── Áreas ──
function openAreaModal(a: AreaItem | null) {
  areaModal.editing = a
  areaForm.description = a?.description ?? ''
  areaModal.show = true
}

async function saveArea() {
  if (!areaForm.description.trim()) {
    showToast('Debes ingresar un nombre válido para el área.', 'warning')
    return
  }
  areaModal.saving = true
  try {
    if (areaModal.editing) {
      await http.put(`/api/Areas/${areaModal.editing.id}`, {
        id: areaModal.editing.id,
        description: areaForm.description.trim()
      })
      showToast('Área actualizada correctamente.', 'success')
    } else {
      await http.post('/api/Areas', { description: areaForm.description.trim() })
      showToast('Área registrada correctamente.', 'success')
    }
    areaModal.show = false
    fetchAreas()
    fetchAreaOptions()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo guardar el área.', 'error')
  } finally {
    areaModal.saving = false
  }
}

async function deactivateArea(a: AreaItem) {
  const ok = await askConfirmation({
    title: '¿Desactivar Área?',
    text: `El área "${a.description}" quedará inactiva y no podrá asignarse a nuevos departamentos.`,
    confirmText: 'Sí, desactivar',
    danger: true
  })
  if (!ok) return
  try {
    await http.put(`/api/Areas/${a.id}/deactivate`)
    showToast('Área desactivada.', 'success')
    fetchAreas()
    fetchAreaOptions()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo desactivar el área.', 'error')
  }
}

// ── Departamentos ──
function openDeptModal(d: DeptItem | null) {
  deptModal.editing = d
  deptForm.description = d?.description ?? ''
  deptForm.areaId = d?.areaId != null ? String(d.areaId) : ''
  deptModal.show = true
}

async function saveDept() {
  if (!deptForm.description.trim()) {
    showToast('Debes ingresar un nombre válido para el departamento.', 'warning')
    return
  }
  deptModal.saving = true
  try {
    const payload = {
      description: deptForm.description.trim(),
      areaId: deptForm.areaId ? Number(deptForm.areaId) : null
    }
    if (deptModal.editing) {
      await http.put(`/api/Departments/${deptModal.editing.id}`, { id: deptModal.editing.id, ...payload })
      showToast('Departamento actualizado correctamente.', 'success')
    } else {
      await http.post('/api/Departments', payload)
      showToast('Departamento registrado correctamente.', 'success')
    }
    deptModal.show = false
    fetchDepts()
    fetchAreas()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo guardar el departamento.', 'error')
  } finally {
    deptModal.saving = false
  }
}

async function toggleDeptActive(d: DeptItem) {
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
    fetchDepts()
    fetchAreas()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo cambiar la vigencia.', 'error')
  }
}

async function deactivateDept(d: DeptItem) {
  const ok = await askConfirmation({
    title: '¿Desactivar Departamento?',
    text: `El departamento "${d.description}" quedará inactivo.`,
    confirmText: 'Sí, desactivar',
    danger: true
  })
  if (!ok) return
  try {
    await http.put(`/api/Departments/${d.id}/deactivate`)
    showToast('Departamento desactivado.', 'success')
    fetchDepts()
    fetchAreas()
  } catch (e: any) {
    showToast(errMessage(e) || 'No se pudo desactivar el departamento.', 'error')
  }
}

function errMessage(e: any): string {
  const d = e?.response?.data
  if (typeof d === 'string') return d
  return d?.message || ''
}

onMounted(() => {
  fetchAreas()
  fetchAreaOptions()
  fetchDepts()
})
</script>

<style scoped>
.section-title {
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
