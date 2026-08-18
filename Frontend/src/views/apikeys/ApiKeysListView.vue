<template>
  <div>
    <AppPageHeader title="Gestión de API Keys" subtitle="Administra las claves de acceso para consumo de API REST" icon="fa-key">
      <template #actions>
        <AppButton variant="primary" icon="fa-plus" @click="openCreateModal">
          Nueva API Key
        </AppButton>
      </template>
    </AppPageHeader>

    <AppSpinner v-if="loading" />
    <template v-else>
      <section class="tecnm-card">
        <div class="tecnm-card-header tecnm-card-header--premium">
          <h4 class="filter-title">Listado de API Keys</h4>
          <span class="count-badge">{{ keys.length }} registro(s)</span>
        </div>

        <div class="tecnm-table-responsive">
          <table class="tecnm-table tecnm-table-striped">
            <thead>
              <tr>
                <th class="text-center">ID</th>
                <th class="text-center">Token / Llave de Acceso</th>
                <th class="text-center">Usuario Propietario</th>
                <th class="text-center">Reglas Asignadas</th>
                <th class="text-center">Estado</th>
                <th class="text-center">Creación</th>
                <th class="text-center">Modificación</th>
                <th class="text-center">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="k in keys" :key="k.id">
                <td class="text-center"><strong>#{{ k.id }}</strong></td>
                <td class="text-center">
                  <div class="key-token-row">
                    <code>{{ maskKey(k.key) }}</code>
                    <button type="button" class="icon-copy-btn" title="Copiar Token" @click="copyKey(k.key)">
                      <i class="fa fa-copy" aria-hidden="true"></i>
                    </button>
                  </div>
                </td>
                <td class="text-center"><strong>{{ k.user }}</strong></td>
                <td class="text-center">
                  <span class="rules-badge"><i class="fa fa-shield" aria-hidden="true"></i> {{ k.permissionsCount }} reglas</span>
                </td>
                <td class="text-center">
                  <StatusBadge :variant="k.status ? 'approved' : 'rejected'">
                    {{ k.status ? 'Activa' : 'Inactiva' }}
                  </StatusBadge>
                </td>
                <td class="text-center">
                  <div class="datetime-cell">
                    <span class="datetime-date">{{ formatDateCell(k.createdAt) }}</span>
                    <span class="datetime-time">{{ formatTimeCell(k.createdAt) }}</span>
                  </div>
                </td>
                <td class="text-center">
                  <div v-if="k.updatedAt" class="datetime-cell">
                    <span class="datetime-date">{{ formatDateCell(k.updatedAt) }}</span>
                    <span class="datetime-time">{{ formatTimeCell(k.updatedAt) }}</span>
                  </div>
                  <span v-else class="text-muted small">Sin modificar</span>
                </td>
                <td class="text-center">
                  <div class="tecnm-table-actions">
                    <!-- Editar: cambiar usuario propietario (solo admin) -->
                    <button
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--edit"
                      title="Editar propietario de API Key"
                      @click="openEditModal(k)"
                    >
                      <i class="fa fa-edit" aria-hidden="true"></i>
                    </button>
                    <!-- Desactivar -->
                    <button
                      v-if="k.status"
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--delete"
                      title="Desactivar API Key"
                      @click="deactivateKey(k)"
                    >
                      <i class="fa fa-trash" aria-hidden="true"></i>
                    </button>
                    <!-- Reglas por tabla -->
                    <button
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--edit"
                      title="Editar reglas de acceso por tabla"
                      @click="openRulesModal(k)"
                    >
                      <i class="fa fa-shield" aria-hidden="true"></i>
                    </button>
                    <!-- Ver detalles / token -->
                    <button
                      type="button"
                      class="tecnm-action-btn tecnm-action-btn--view"
                      title="Ver detalles de la API Key"
                      @click="viewKeyDetails(k)"
                    >
                      <i class="fa fa-eye" aria-hidden="true"></i>
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="!keys.length">
                <td colspan="8"><EmptyState message="No hay API Keys registradas"><span>Genera una nueva API Key para autorizar accesos externos</span></EmptyState></td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>

    <!-- Modal Formulario Reactivo (Nueva API Key) -->
    <AppModal
      v-model:show="showCreateModal"
      title="Nueva API Key"
      icon="fa-key"
      :loading="submitting"
      confirm-text="Generar Clave"
      @confirm="generateApiKey"
    >
      <div class="tecnm-form-group">
        <label class="tecnm-label" for="apiKeyUserId">Usuario Propietario <span class="text-danger">*</span></label>
        <select id="apiKeyUserId" v-model="selectedUserId" class="tecnm-form-control">
          <option :value="null">Seleccione un usuario...</option>
          <option v-for="u in users" :key="u.id" :value="u.id">
            {{ u.name }} ({{ u.email }})
          </option>
        </select>
        <p class="small text-muted mt-2 mb-0">Asigna las reglas de acceso por tabla. Si no asignas ninguna, la clave nace sin permisos.</p>
        <div class="mt-3">
          <button type="button" class="btn btn-sm btn-outline-secondary" @click="toggleCreateRules">
            <i class="fa" :class="showCreateRules ? 'fa-chevron-up' : 'fa-chevron-down'" aria-hidden="true"></i>
            {{ showCreateRules ? 'Ocultar' : 'Asignar' }} reglas por tabla
          </button>
          <div v-if="showCreateRules" class="mt-2">
            <ApiKeyRulesEditor v-if="catalogsReady" v-model="createRules" :tables="tables" :permissions="apiPermissions" />
          </div>
        </div>
      </div>
    </AppModal>

    <!-- Modal de Resultado / Exhibición del Token Generado -->
    <AppModal
      v-model:show="showResultModal"
      title="API Key Generada Exitosamente"
      icon="fa-check-circle"
      size="md"
      :show-cancel="false"
      confirm-text="Entendido"
      @confirm="showResultModal = false"
    >
      <div class="text-center py-2">
        <p class="mb-3">Copia y guarda esta clave. Por seguridad, no se podrá visualizar completa nuevamente:</p>
        <div class="token-result-box">
          <code>{{ generatedToken }}</code>
          <AppButton size="sm" variant="outline" icon="fa-copy" @click="copyKey(generatedToken)">Copiar Token</AppButton>
        </div>
      </div>
    </AppModal>

    <!-- Modal de Edición de Reglas por Tabla -->
    <AppModal
      v-model:show="showRulesModal"
      :title="rulesKey ? `Reglas de acceso — API Key #${rulesKey.id}` : 'Reglas de acceso'"
      icon="fa-shield"
      size="lg"
      :loading="submitting"
      confirm-text="Guardar Reglas"
      @confirm="saveRules"
    >
      <p class="small text-muted mb-2">Marca qué operaciones (Leer / Escribir / Actualizar / Eliminar) se permiten sobre cada tabla para esta API Key.</p>
      <ApiKeyRulesEditor v-if="catalogsReady" v-model="editRules" :tables="tables" :permissions="apiPermissions" />
    </AppModal>

    <!-- Modal de Edición de Propietario -->
    <AppModal
      v-model:show="showEditModal"
      title="Editar Propietario de API Key"
      icon="fa-edit"
      :loading="submitting"
      confirm-text="Guardar Cambios"
      @confirm="saveEditKey"
    >
      <div class="tecnm-form-group">
        <label class="tecnm-label" for="editKeyUserId">Nuevo Usuario Propietario <span class="text-danger">*</span></label>
        <select id="editKeyUserId" v-model="selectedUserId" class="tecnm-form-control">
          <option :value="null">Seleccione un usuario...</option>
          <option v-for="u in users" :key="u.id" :value="u.id">
            {{ u.name }} ({{ u.email }})
          </option>
        </select>
      </div>
    </AppModal>

    <!-- Modal de Ver Detalles -->
    <AppModal
      v-model:show="showDetailsModal"
      :title="selectedKey ? `API Key #${selectedKey.id} — ${selectedKey.user}` : 'Detalles de API Key'"
      icon="fa-eye"
      size="md"
      :show-cancel="false"
      confirm-text="Cerrar"
      @confirm="showDetailsModal = false"
    >
      <div v-if="selectedKey" class="key-details-content">
        <div class="audit-info-grid mb-3">
          <div><strong>Propietario:</strong> {{ selectedKey.user }}</div>
          <div><strong>Estado:</strong> {{ selectedKey.status ? 'Activa' : 'Inactiva' }}</div>
          <div><strong>Creación:</strong> {{ formatDateCell(selectedKey.createdAt) }}</div>
          <div><strong>Modificación:</strong> {{ formatDateCell(selectedKey.updatedAt) }}</div>
        </div>
        <div class="json-box">
          <span class="json-box-title">Token (enmascarado):</span>
          <div class="token-result-box">
            <code>{{ maskKey(selectedKey.key) }}</code>
            <AppButton size="sm" variant="outline" icon="fa-copy" @click="copyKey(selectedKey.key)">Copiar</AppButton>
          </div>
        </div>
        <div class="json-box mt-3">
          <span class="json-box-title">Reglas por Tabla ({{ detailPermissions.length }}):</span>
          <div v-if="loadingDetail" class="text-center py-3">
            <i class="fas fa-spinner fa-spin text-primary"></i>
          </div>
          <div v-else-if="detailPermissions.length === 0" class="small text-muted">
            Esta API Key no tiene reglas de acceso asignadas.
          </div>
          <table v-else class="tecnm-table tecnm-table-sm mt-2">
            <thead>
              <tr>
                <th>Tabla</th>
                <th class="text-center">Permiso</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(r, i) in detailPermissions" :key="i">
                <td class="fw-semibold">{{ r.tableName }}</td>
                <td class="text-center">
                  <span class="rules-badge"><i class="fa fa-check-circle text-success" aria-hidden="true"></i> {{ r.permissionName }}</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </AppModal>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { http } from '@/api/client'
import AppPageHeader from '@/components/AppPageHeader.vue'
import AppButton from '@/components/AppButton.vue'
import AppSpinner from '@/components/AppSpinner.vue'
import EmptyState from '@/components/EmptyState.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import AppModal from '@/components/AppModal.vue'
import ApiKeyRulesEditor from './components/ApiKeyRulesEditor.vue'
import { formatTecNMDate } from '@/composables/useDates'
import { askConfirmation, showToast } from '@/composables/useAlerts'
import {
  fetchApiKeys,
  fetchApiKey,
  createApiKey,
  updateApiKeyOwner,
  updateApiKeyPermissions,
  deactivateApiKey,
  fetchDatabaseTables,
  fetchApiPermissions,
  type ApiKeyItem,
  type ApiKeyRule,
  type ApiPermissionItem,
  type ApiTableItem,
  type PermissionAssignment
} from '@/api/apikeys'

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

const keys = ref<ApiKeyItem[]>([])
const users = ref<any[]>([])
const loading = ref(true)
const submitting = ref(false)
const loadingDetail = ref(false)

// Modales reactivos
const showCreateModal = ref(false)
const showResultModal = ref(false)
const showEditModal = ref(false)
const showDetailsModal = ref(false)
const showRulesModal = ref(false)
const showCreateRules = ref(false)
const selectedUserId = ref<number | null>(null)
const selectedKey = ref<ApiKeyItem | null>(null)
const editingKeyId = ref<number | null>(null)
const generatedToken = ref('')

// Catálogos para el editor de reglas
const tables = ref<ApiTableItem[]>([])
const apiPermissions = ref<ApiPermissionItem[]>([])
const catalogsReady = ref(false)
const createRules = ref<Record<number, number[]>>({})
const editRules = ref<Record<number, number[]>>({})
const rulesKey = ref<ApiKeyItem | null>(null)
const detailPermissions = ref<ApiKeyRule[]>([])

function maskKey(key: string) {
  if (!key) return ''
  if (key.length <= 8) return key
  return key.substring(0, 4) + '••••••••' + key.substring(key.length - 4)
}

function copyKey(key: string) {
  if (!key) return
  navigator.clipboard.writeText(key)
  showToast('Token copiado al portapapeles.', 'success')
}

async function loadCatalogs() {
  try {
    const [t, p] = await Promise.all([fetchDatabaseTables(), fetchApiPermissions()])
    tables.value = t
    apiPermissions.value = p
    catalogsReady.value = true
  } catch (e) {
    showToast('No se pudieron cargar los catálogos de reglas.', 'error')
    catalogsReady.value = false
  }
}

function rulesToAssignments(rules: Record<number, number[]>): PermissionAssignment[] {
  const result: PermissionAssignment[] = []
  for (const [tableId, perms] of Object.entries(rules)) {
    for (const permissionId of perms) {
      result.push({ tableId: Number(tableId), permissionId })
    }
  }
  return result
}

async function fetchKeys() {
  loading.value = true
  try {
    const res = await fetchApiKeys({ pageSize: 100 })
    keys.value = res.items
  } catch (e) {
    showToast('Error cargando las API Keys.', 'error')
  } finally {
    loading.value = false
  }
}

async function openCreateModal() {
  try {
    const uRes = await httpGetUsers()
    users.value = uRes
    await loadCatalogs()
    selectedUserId.value = null
    createRules.value = {}
    showCreateRules.value = false
    showCreateModal.value = true
  } catch (e) {
    showToast('Error cargando usuarios.', 'error')
  }
}

async function httpGetUsers() {
  const res = await http.get('/api/Users?pageSize=100')
  return res.data.items || []
}

function toggleCreateRules() {
  showCreateRules.value = !showCreateRules.value
}

async function generateApiKey() {
  if (!selectedUserId.value) {
    showToast('Debes seleccionar un usuario propietario.', 'warning')
    return
  }

  submitting.value = true
  try {
    const res = await createApiKey(selectedUserId.value, rulesToAssignments(createRules.value))
    generatedToken.value = res.key
    showCreateModal.value = false
    showResultModal.value = true
    fetchKeys()
  } catch (e: any) {
    showToast(e.response?.data?.message || 'No se pudo generar la API Key.', 'error')
  } finally {
    submitting.value = false
  }
}

async function openEditModal(k: ApiKeyItem) {
  try {
    const uRes = await httpGetUsers()
    users.value = uRes
    selectedUserId.value = null
    editingKeyId.value = k.id
    showEditModal.value = true
  } catch (e) {
    showToast('Error cargando usuarios.', 'error')
  }
}

async function saveEditKey() {
  if (!selectedUserId.value || !editingKeyId.value) {
    showToast('Debes seleccionar un usuario.', 'warning')
    return
  }
  submitting.value = true
  try {
    await updateApiKeyOwner(editingKeyId.value, selectedUserId.value)
    showToast('Propietario actualizado correctamente.', 'success')
    showEditModal.value = false
    fetchKeys()
  } catch (e: any) {
    showToast(e.response?.data?.message || 'No se pudo actualizar el propietario.', 'error')
  } finally {
    submitting.value = false
  }
}

async function openRulesModal(k: ApiKeyItem) {
  rulesKey.value = k
  editRules.value = {}
  await loadCatalogs()
  try {
    const detail = await fetchApiKey(k.id)
    const rules: Record<number, number[]> = {}
    for (const r of detail.permissions || []) {
      if (!rules[r.tableId]) rules[r.tableId] = []
      rules[r.tableId].push(r.apiPermissionId)
    }
    editRules.value = rules
  } catch (e: any) {
    showToast(e.response?.data?.message || 'No se pudieron cargar las reglas.', 'error')
  }
  showRulesModal.value = true
}

async function saveRules() {
  if (!rulesKey.value) return
  submitting.value = true
  try {
    await updateApiKeyPermissions(rulesKey.value.id, 0, rulesToAssignments(editRules.value))
    showToast('Reglas de acceso actualizadas correctamente.', 'success')
    showRulesModal.value = false
    fetchKeys()
  } catch (e: any) {
    showToast(e.response?.data?.message || 'No se pudieron guardar las reglas.', 'error')
  } finally {
    submitting.value = false
  }
}

async function viewKeyDetails(k: ApiKeyItem) {
  selectedKey.value = k
  detailPermissions.value = []
  loadingDetail.value = true
  showDetailsModal.value = true
  try {
    const detail = await fetchApiKey(k.id)
    detailPermissions.value = detail.permissions || []
  } catch (e: any) {
    showToast(e.response?.data?.message || 'No se pudieron cargar las reglas de la clave.', 'error')
  } finally {
    loadingDetail.value = false
  }
}

async function deactivateKey(key: ApiKeyItem) {
  const ok = await askConfirmation({
    title: '¿Desactivar API Key?',
    text: `Esta acción revocará el acceso de la clave con ID #${key.id}.`,
    confirmText: 'Sí, desactivar',
    danger: true
  })

  if (ok) {
    try {
      await deactivateApiKey(key.id)
      showToast('La API Key ha sido desactivada.', 'success')
      fetchKeys()
    } catch (e: any) {
      showToast(e.response?.data?.message || 'No se pudo desactivar la clave.', 'error')
    }
  }
}

onMounted(() => {
  fetchKeys()
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

.key-token-row {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  background: var(--tecnm-bg-light);
  padding: 0.25rem 0.6rem;
  border-radius: 4px;
  border: 1px solid var(--tecnm-border);
}

.key-token-row code {
  font-family: monospace;
  font-size: 0.85rem;
  color: var(--tecnm-navy);
}

.token-result-box {
  background: var(--tecnm-soft-hover);
  border: 1px solid var(--tecnm-border-color);
  padding: 1rem;
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.75rem;
}

.token-result-box code {
  font-family: monospace;
  font-size: 1rem;
  font-weight: 700;
  color: var(--tecnm-navy);
  word-break: break-all;
}

.icon-copy-btn {
  background: transparent;
  border: none;
  color: var(--tecnm-text-secondary);
  cursor: pointer;
  padding: 0.1rem 0.3rem;

  &:hover {
    color: var(--tecnm-navy);
  }
}

.rules-badge {
  font-size: 0.82rem;
  background: var(--tecnm-bg-light);
  color: var(--tecnm-text);
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
  border: 1px solid var(--tecnm-border);
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

.audit-info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 0.75rem;
  font-size: 0.9rem;
  background: var(--tecnm-soft-hover);
  padding: 0.75rem 1rem;
  border-radius: 6px;
  border: 1px solid var(--tecnm-border-color, #e2e8f0);
}

.json-box-title {
  display: block;
  font-weight: 600;
  font-size: 0.85rem;
  margin-bottom: 0.5rem;
}

.mb-3 {
  margin-bottom: 0.75rem;
}
</style>
