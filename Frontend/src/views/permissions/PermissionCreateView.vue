<template>
  <div>
    <AppPageHeader title="Nuevo Permiso" subtitle="Registra una nueva solicitud de permiso" icon="fa-plus-circle" />

    <section class="tecnm-card">
      <div class="tecnm-card-header tecnm-card-header--premium">
        <h4 class="filter-title"><i class="fa fa-edit" aria-hidden="true"></i> Datos del Permiso</h4>
      </div>
      <div class="tecnm-card-body">
        <form @submit.prevent="submit" novalidate>
          <div v-if="canChooseRequester" class="tecnm-form-group">
            <label class="tecnm-label">
              Solicitante <span class="required-mark">*</span>
            </label>
            <AppAutocomplete
              :model-value="form.requestingUserId"
              :search="searchRequester"
              :disabled="submitting"
              placeholder="Buscar usuario por nombre o correo..."
              @update:model-value="onRequesterChange"
            />
            <span v-if="errors.requestingUserId" class="tecnm-form-error">{{ errors.requestingUserId }}</span>
          </div>

          <div class="tecnm-form-group">
            <label class="tecnm-label">Tipo de permiso <span class="required-mark">*</span></label>
            <div class="permission-types">
              <button
                v-for="t in visiblePermissionTypes"
                :key="t.id"
                type="button"
                class="permission-type-card"
                :class="{ selected: form.permissionTypeId === t.id }"
                :data-type="t.id"
                @click="selectType(t.id)"
              >
                <span class="type-icon"><i :class="`fa ${typeIcon(t)}`" aria-hidden="true"></i></span>
                <span class="type-title">{{ t.description }}</span>
                <span class="type-subtitle">{{ typeSubtitle(t) }}</span>
              </button>
            </div>
            <span v-if="errors.permissionTypeId" class="tecnm-form-error">{{ errors.permissionTypeId }}</span>
          </div>

          <div v-if="selectedType !== null" class="datetime-grid">
            <AppFormField v-if="date1Config.show" :label="date1Config.label" :error="errors.startDate">
              <input v-model="form.startDate" type="date" class="tecnm-form-control" :required="date1Config.required" />
            </AppFormField>

            <AppFormField v-if="time1Config.show" :label="time1Config.label" :error="errors.startTime">
              <input v-model="form.startTime" type="time" class="tecnm-form-control" :required="time1Config.required" />
            </AppFormField>

            <AppFormField v-if="date2Config.show" :label="date2Config.label" :error="errors.endDate">
              <input v-model="form.endDate" type="date" class="tecnm-form-control" :required="date2Config.required" />
              <span v-if="datesInverted" class="tecnm-form-error">{{ errors.datesInverted }}</span>
            </AppFormField>

            <AppFormField v-if="time2Config.show" :label="time2Config.label" :error="errors.endTime">
              <input v-model="form.endTime" type="time" class="tecnm-form-control" :required="time2Config.required" />
            </AppFormField>

            <template v-if="isTeacherAbsence">
              <AppFormField label="Horas en aula" :error="errors.hours">
                <input v-model.number="form.hoursInClassroom" type="number" min="0" max="8" class="tecnm-form-control" placeholder="0" />
              </AppFormField>

              <AppFormField label="Horas de descarga">
                <input v-model.number="form.hoursDischarge" type="number" min="0" max="8" class="tecnm-form-control" placeholder="0" />
              </AppFormField>

              <div v-if="totalHours > 0" class="tecnm-form-group">
                <small class="char-counter" :class="totalHours > 8 ? 'error' : 'success'">
                  Total: {{ totalHours }} / 8 hrs
                </small>
              </div>
            </template>
          </div>

          <div class="tecnm-form-group">
            <label class="tecnm-label" for="reason">
              Motivo <span class="required-mark">*</span>
            </label>
            <textarea
              id="reason"
              v-model="form.reason"
              class="tecnm-form-control"
              rows="3"
              maxlength="255"
              placeholder="Ej: Cita médica en el IMSS o Trámite personal..."
            ></textarea>
            <span v-if="errors.reason" class="tecnm-form-error">{{ errors.reason }}</span>
            <small class="char-counter" :class="counterClass">{{ form.reason.length }}/255</small>
          </div>

          <div class="tecnm-form-group">
            <label class="tecnm-label" for="evidenceFile">Evidencia (PDF opcional)</label>
            <div class="file-upload-area" :class="{ 'drag-over': dragOver }" role="button" tabindex="0" aria-label="Subir evidencia PDF" @click="triggerFilePicker" @keydown.enter.prevent="triggerFilePicker" @dragover.prevent="dragOver = true" @dragleave.prevent="dragOver = false" @drop.prevent="onDrop">
              <input id="evidenceFile" ref="fileInputRef" type="file" accept=".pdf,application/pdf" class="file-input" @change="onFileChange" />
              <div v-if="!evidenceFile" class="file-upload-content">
                <div class="file-upload-icon"><i class="fa fa-cloud-upload-alt" aria-hidden="true"></i></div>
                <p class="file-upload-text">Subir PDF</p>
                <p class="file-upload-hint">Haz clic o arrastra un archivo PDF aquí</p>
              </div>
              <div v-else class="file-preview">
                <span class="file-info"><i class="fa fa-file-pdf file-icon" aria-hidden="true"></i>{{ evidenceFile.name }}</span>
                <button type="button" class="remove-file" title="Eliminar archivo" @click.stop="removeFile">
                  <i class="fa fa-times" aria-hidden="true"></i>
                </button>
              </div>
            </div>
            <span v-if="errors.evidence" class="tecnm-form-error">{{ errors.evidence }}</span>
          </div>

          <div v-if="days !== null && !datesInverted" class="tecnm-alert tecnm-alert-info tecnm-mb-4">
            <i class="fa fa-info-circle" aria-hidden="true"></i>
            <span>El permiso abarcará <strong>{{ days }}</strong> día(s).</span>
          </div>

          <div v-if="errorGeneral" class="tecnm-alert tecnm-alert-danger tecnm-mb-4">
            <i class="fa fa-times-circle" aria-hidden="true"></i>
            <span>{{ errorGeneral }}</span>
          </div>

          <div class="form-actions">
            <AppButton type="submit" variant="primary" icon="fa-paper-plane" :loading="submitting" :disabled="!formValid">
              Enviar Solicitud
            </AppButton>
            <AppButton type="button" variant="secondary" icon="fa-arrow-left" @click="$router.back()">
              Cancelar
            </AppButton>
          </div>
        </form>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import AppPageHeader from '@/components/AppPageHeader.vue'
import AppButton from '@/components/AppButton.vue'
import AppFormField from '@/components/AppFormField.vue'
import AppAutocomplete, { type AutocompleteItem } from '@/components/AppAutocomplete.vue'
import {
  fetchPermissionTypes,
  searchUsers,
  type PermissionTypeCardConfig,
  type PermissionTypeFieldConfig,
  type PermissionTypeItem
} from '@/api/catalogs'
import { createPermission, type PermissionCreatePayload } from '@/api/permissions'
import { combineDateTimeLocal, calculateDays } from '@/composables/useDates'
import { handleAlertResponse, showToast } from '@/composables/useAlerts'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const permissionTypes = ref<PermissionTypeItem[]>([])
// Falta Docente (5): solo visible para personal DOCENCIA; el backend revalida.
const TEACHER_ABSENCE_TYPE_ID = 5
const visiblePermissionTypes = computed(() =>
  authStore.user?.isDocente ? permissionTypes.value : permissionTypes.value.filter((t) => t.id !== TEACHER_ABSENCE_TYPE_ID)
)
const submitting = ref(false)
const errorGeneral = ref('')
const errors = reactive<Record<string, string>>({})
const evidenceFile = ref<File | null>(null)
const fileInputRef = ref<HTMLInputElement | null>(null)
const dragOver = ref(false)

const form = reactive({
  permissionTypeId: undefined as number | undefined,
  requestingUserId: undefined as number | undefined,
  startDate: '',
  startTime: '',
  endDate: '',
  endTime: '',
  withPay: true,
  reason: '',
  hoursInClassroom: undefined as number | undefined,
  hoursDischarge: undefined as number | undefined
})

const isTeacherAbsence = computed(() => form.permissionTypeId === 5)

const totalHours = computed(() => (form.hoursInClassroom ?? 0) + (form.hoursDischarge ?? 0))

const canChooseRequester = computed(() => authStore.isAdmin || authStore.isRH)

async function searchRequester(term: string): Promise<AutocompleteItem[]> {
  const results = await searchUsers(term)
  return results.map((u) => ({
    id: u.id,
    label: u.name,
    sublabel: u.email ? u.email : u.department
  }))
}

const defaultTypeConfig: PermissionTypeCardConfig = {
  icon: 'fa-file-alt',
  subtitle: '',
  date1: { show: true, label: 'Fecha de inicio', required: true },
  date2: { show: true, label: 'Fecha de fin', required: false },
  time1: { show: false, label: 'Hora de entrada', required: false },
  time2: { show: false, label: 'Hora de salida', required: false }
}

const selectedConfig = computed<PermissionTypeCardConfig>(() => {
  const type = permissionTypes.value.find((t) => t.id === form.permissionTypeId)
  return type?.config ?? defaultTypeConfig
})

const selectedType = computed<number | null>(() => form.permissionTypeId ?? null)

const date1Config = computed<PermissionTypeFieldConfig>(() => selectedConfig.value.date1)
const date2Config = computed<PermissionTypeFieldConfig>(() => selectedConfig.value.date2)
const time1Config = computed<PermissionTypeFieldConfig>(() => selectedConfig.value.time1)
const time2Config = computed<PermissionTypeFieldConfig>(() => selectedConfig.value.time2)

const days = computed(() => {
  const start = combineDateTimeLocal(form.startDate, form.startTime)
  const end = combineDateTimeLocal(form.endDate, form.endTime, true)
  if (!start || !end) return null
  return calculateDays(start, end)
})

// FR8A: validación de fechas invertidas (fin no anterior a inicio)
const datesInverted = computed(() => {
  if (!date1Config.value.show || !date2Config.value.show) return false
  if (!form.startDate || !form.endDate) return false
  return new Date(form.startDate) > new Date(form.endDate)
})

const counterClass = computed(() => {
  const len = form.reason.length
  if (len > 255) return 'error'
  if (len > 200) return 'warning'
  return 'success'
})

function typeIcon(t: PermissionTypeItem): string {
  return t.config?.icon || 'fa-file-alt'
}

function typeSubtitle(t: PermissionTypeItem): string {
  return t.config?.subtitle || `Permiso de tipo ${t.description}`
}

function onRequesterChange(value: number | null) {
  form.requestingUserId = value ?? undefined
  validate()
}

function selectType(id: number) {
  form.permissionTypeId = id
  clearDynamicFields()
  validate()
}

function clearDynamicFields() {
  if (!date1Config.value.show) form.startDate = ''
  if (!date2Config.value.show) form.endDate = ''
  if (!time1Config.value.show) form.startTime = ''
  if (!time2Config.value.show) form.endTime = ''
  if (!isTeacherAbsence.value) {
    form.hoursInClassroom = undefined
    form.hoursDischarge = undefined
  }
}

const formValid = computed(() => {
  if (canChooseRequester.value && !form.requestingUserId) return false
  if (!form.permissionTypeId) return false
  if (date1Config.value.required && !form.startDate) return false
  if (date2Config.value.required && !form.endDate) return false
  if (time1Config.value.required && !form.startTime) return false
  if (time2Config.value.required && !form.endTime) return false
  if (datesInverted.value) return false
  if (isTeacherAbsence.value) {
    const th = totalHours.value
    if (th < 1 || th > 8) return false
  }
  const reason = form.reason.trim()
  if (!reason || reason.length > 255) return false
  return true
})

function validate() {
  Object.keys(errors).forEach((k) => delete errors[k])
  if (canChooseRequester.value && !form.requestingUserId) {
    errors.requestingUserId = 'Seleccione el usuario solicitante.'
  }
  if (!form.permissionTypeId) {
    errors.permissionTypeId = 'Seleccione el tipo de permiso.'
  }
  if (date1Config.value.show) {
    if (date1Config.value.required && !form.startDate) errors.startDate = 'Este campo es obligatorio.'
    else delete errors.startDate
  }
  if (date2Config.value.show) {
    if (date2Config.value.required && !form.endDate) errors.endDate = 'Este campo es obligatorio.'
    else delete errors.endDate
  }
  if (time1Config.value.show) {
    if (time1Config.value.required && !form.startTime) errors.startTime = 'Este campo es obligatorio.'
    else delete errors.startTime
  }
  if (time2Config.value.show) {
    if (time2Config.value.required && !form.endTime) errors.endTime = 'Este campo es obligatorio.'
    else delete errors.endTime
  }
  if (datesInverted.value) {
    errors.datesInverted = 'La fecha de fin no puede ser anterior a la de inicio.'
  }
  if (isTeacherAbsence.value) {
    const th = totalHours.value
    if (th < 1 || th > 8) errors.hours = 'Las horas totales deben ser entre 1 y 8.'
    else delete errors.hours
  }
  const reason = form.reason.trim()
  if (!reason) errors.reason = 'El motivo es obligatorio.'
  else if (reason.length > 255) errors.reason = 'El motivo no puede exceder los 255 caracteres.'
  else delete errors.reason
}

function validateEvidence(file: File): string | null {
  if (!/\.pdf$/i.test(file.name) && file.type && file.type !== 'application/pdf') {
    return 'Solo se permiten archivos PDF.'
  }
  if (file.size > 5 * 1024 * 1024) {
    return 'El archivo no debe superar los 5MB.'
  }
  return null
}

function triggerFilePicker() {
  fileInputRef.value?.click()
}

function onFileChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (!file) return
  const err = validateEvidence(file)
  if (err) {
    errors.evidence = err
    evidenceFile.value = null
    if (fileInputRef.value) fileInputRef.value.value = ''
    showToast(err, 'error')
    return
  }
  delete errors.evidence
  evidenceFile.value = file
}

function onDrop(e: DragEvent) {
  dragOver.value = false
  const file = e.dataTransfer?.files?.[0]
  if (!file) return
  const err = validateEvidence(file)
  if (err) {
    errors.evidence = err
    showToast(err, 'error')
    return
  }
  delete errors.evidence
  evidenceFile.value = file
  if (fileInputRef.value) {
    const dt = new DataTransfer()
    dt.items.add(file)
    fileInputRef.value.files = dt.files
  }
}

function removeFile() {
  evidenceFile.value = null
  if (fileInputRef.value) fileInputRef.value.value = ''
  delete errors.evidence
}

async function submit() {
  errorGeneral.value = ''
  validate()
  if (Object.keys(errors).length > 0) return

  const payload: PermissionCreatePayload = {
    reason: form.reason,
    startDate: combineDateTimeLocal(form.startDate, form.startTime) as string,
    endDate: date2Config.value.show ? (combineDateTimeLocal(form.endDate, form.endTime, true) as string) : undefined,
    permissionTypeId: form.permissionTypeId as number,
    withPay: form.withPay,
    requestingUserId: canChooseRequester.value ? form.requestingUserId : authStore.user?.id,
    evidence: evidenceFile.value,
    hoursInClassroom: isTeacherAbsence.value ? form.hoursInClassroom : undefined,
    hoursDischarge: isTeacherAbsence.value ? form.hoursDischarge : undefined
  }

  submitting.value = true
  try {
    const result = await createPermission(payload)
    if (result.showConfirmation) {
      await handleAlertResponse({
        success: false,
        title: result.title || 'Excede el periodo',
        message: result.message,
        icon: (result.icon as 'warning') || 'warning',
        confirmButtonText: result.confirmButtonText
      })
      const confirmedResult = await createPermission({ ...payload, confirmed: true })
      handleCreated(confirmedResult)
    } else {
      handleCreated(result)
    }
  } catch (err) {
    console.error(err)
    const data = (err as { response?: { data?: { message?: string; title?: string } } })?.response?.data
    if (data?.message) {
      await handleAlertResponse({ success: false, title: data.title || 'Error', message: data.message })
    } else {
      errorGeneral.value = 'No se pudo registrar el permiso. Verifique los datos e intente nuevamente.'
      showToast('Error al registrar el permiso', 'error')
    }
  } finally {
    submitting.value = false
  }
}

function handleCreated(result: { id?: number }) {
  if (result?.id) {
    router.push({ name: 'permissions.details', params: { id: String(result.id) } })
    showToast('Permiso creado correctamente', 'success')
  } else {
    errorGeneral.value = 'No se pudo registrar el permiso.'
  }
}

onMounted(async () => {
  try {
    permissionTypes.value = await fetchPermissionTypes()
  } catch (err) {
    console.error('Error cargando catálogos:', err)
    showToast('No se pudieron cargar los datos de apoyo.', 'error')
  }
})
</script>

<style scoped>
.filter-title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin: 0;
}

.required-mark {
  color: var(--tecnm-danger);
}

/* Permission type cards (config-driven, like legacy create.css) */
.permission-types {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(170px, 1fr));
  gap: 0.9rem;
  margin-top: 0.5rem;
}

.permission-type-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.25rem;
  padding: 1.4rem 1rem;
  background: var(--tecnm-surface-white);
  border: 2px solid var(--tecnm-border-color);
  border-radius: var(--tecnm-radius-lg, 1rem);
  text-align: center;
  cursor: pointer;
  font-family: inherit;
  color: var(--tecnm-text-primary);
  transition: var(--tecnm-transition);
}

.permission-type-card:hover {
  border-color: var(--tecnm-accent-text);
  transform: translateY(-3px);
  box-shadow: var(--tecnm-shadow-lg);
}

.permission-type-card.selected {
  border-color: var(--tecnm-gold-accent);
  background: var(--tecnm-gold-light);
  box-shadow: inset 0 0 0 1px var(--tecnm-gold-accent);
}

.type-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 46px;
  height: 46px;
  border-radius: 50%;
  background: linear-gradient(135deg, var(--tecnm-accent-text), var(--tecnm-blue-primary));
  color: var(--tecnm-text-on-blue);
  font-size: 1.1rem;
  margin-bottom: 0.4rem;
  transition: var(--tecnm-transition);
}

.permission-type-card.selected .type-icon {
  background: linear-gradient(135deg, var(--tecnm-gold-accent), var(--tecnm-gold-light));
  color: var(--tecnm-text-on-gold);
}

.type-title {
  font-weight: 700;
  font-size: 0.9rem;
  color: var(--tecnm-heading-color);
}

.type-subtitle {
  font-size: 0.75rem;
  color: var(--tecnm-text-secondary);
}

.datetime-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 0 1.25rem;
  margin-top: 1rem;
}

.switch-row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  font-size: 0.9rem;
  min-height: 2.5rem;
  color: var(--tecnm-text-primary);
}

.char-counter {
  display: block;
  text-align: right;
  margin-top: 0.25rem;
  font-size: 0.75rem;
  font-weight: 600;
}

.char-counter.success {
  color: var(--tecnm-text-secondary);
}

.char-counter.warning {
  color: var(--tecnm-status-pending-text);
}

.char-counter.error {
  color: var(--tecnm-danger);
}

/* Evidence upload */
.file-upload-area {
  position: relative;
  border: 2px dashed var(--tecnm-border-color);
  border-radius: var(--tecnm-radius-lg, 1rem);
  background: var(--tecnm-soft-hover);
  padding: 2rem 1.5rem;
  text-align: center;
  cursor: pointer;
  transition: var(--tecnm-transition);
}

.file-upload-area.drag-over {
  background: var(--tecnm-gold-light);
  border-color: var(--tecnm-gold-accent);
}

.file-input {
  position: absolute;
  width: 1px;
  height: 1px;
  opacity: 0;
  pointer-events: none;
}

.file-upload-icon {
  font-size: 2rem;
  color: var(--tecnm-accent-text);
  margin-bottom: 0.5rem;
}

.file-upload-text {
  font-weight: 700;
  font-size: 0.95rem;
  color: var(--tecnm-heading-color);
}

.file-upload-hint {
  font-size: 0.8rem;
  color: var(--tecnm-text-secondary);
}

.file-preview {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  background: var(--tecnm-surface-white);
  border: 1px solid var(--tecnm-border-color);
  border-radius: var(--tecnm-radius-sm);
  padding: 0.7rem 1rem;
}

.file-info {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  font-size: 0.85rem;
  color: var(--tecnm-text-primary);
  word-break: break-all;
  text-align: left;
}

.file-info .file-icon {
  color: var(--tecnm-danger);
  font-size: 1.2rem;
}

.remove-file {
  width: 36px;
  height: 36px;
  flex-shrink: 0;
  border-radius: 50%;
  background: var(--tecnm-status-rejected-bg);
  color: var(--tecnm-danger);
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: var(--tecnm-transition);
}

.remove-file:hover {
  background: var(--tecnm-danger);
  color: #ffffff;
}

.form-actions {
  display: flex;
  gap: 0.75rem;
  margin-top: 0.5rem;
  flex-wrap: wrap;
}

@media (max-width: 640px) {
  .datetime-grid {
    grid-template-columns: 1fr;
  }
}
</style>
