<template>
  <div>
    <AppPageHeader title="Editar Permiso" subtitle="Modifica los datos de la solicitud" icon="fa-edit">
      <template #actions>
        <AppButton variant="outline" icon="fa-eye" @click="$router.push({ name: 'permissions.details', params: $route.params })">
          Ver detalle
        </AppButton>
      </template>
    </AppPageHeader>

    <AppSpinner v-if="loading" />
    <template v-else>
      <div v-if="config && !config.canEdit" class="tecnm-alert tecnm-alert-warning tecnm-mb-4">
        <i class="fa fa-lock" aria-hidden="true"></i>
        <span>{{ config.blockReason || 'Este permiso no se encuentra en estado editable.' }}</span>
      </div>

      <section class="tecnm-card">
        <div class="tecnm-card-header tecnm-card-header--premium">
          <h4 class="filter-title"><i class="fa fa-edit" aria-hidden="true"></i> Datos del Permiso</h4>
        </div>
        <div class="tecnm-card-body">
          <form @submit.prevent="submit" novalidate>
            <div class="tecnm-form-group">
              <label class="tecnm-label">Tipo de permiso <span class="required-mark">*</span></label>
              <div class="permission-types">
                <button
                  v-for="t in permissionTypes"
                  :key="t.id"
                  type="button"
                  class="permission-type-card"
                  :class="{ selected: form.permissionTypeId === t.id }"
                  :disabled="true"
                  title="El tipo de permiso no es editable"
                >
                  <span class="type-icon"><i :class="`fa ${typeIcon(t)}`" aria-hidden="true"></i></span>
                  <span class="type-title">{{ t.description }}</span>
                  <span class="type-subtitle">{{ typeSubtitle(t) }}</span>
                </button>
              </div>
              <small class="tecnm-form-hint">El tipo de permiso no se puede modificar. Si te equivocaste, cancélalo y crea uno nuevo.</small>
            </div>

            <div class="datetime-grid">
              <AppFormField v-if="date1Config.show" :label="date1Config.label" :error="errors.startDate">
                <input v-model="form.startDate" type="date" class="tecnm-form-control" :required="date1Config.required" :disabled="config && !config.canEdit" />
              </AppFormField>

              <AppFormField v-if="time1Config.show" :label="time1Config.label" :error="errors.startTime">
                <input v-model="form.startTime" type="time" class="tecnm-form-control" :required="time1Config.required" :disabled="config && !config.canEdit" />
              </AppFormField>

              <AppFormField v-if="date2Config.show" :label="date2Config.label" :error="errors.endDate">
                <input v-model="form.endDate" type="date" class="tecnm-form-control" :required="date2Config.required" :disabled="config && !config.canEdit" />
                <span v-if="datesInverted" class="tecnm-form-error">{{ errors.datesInverted }}</span>
              </AppFormField>

              <AppFormField v-if="time2Config.show" :label="time2Config.label" :error="errors.endTime">
                <input v-model="form.endTime" type="time" class="tecnm-form-control" :required="time2Config.required" :disabled="config && !config.canEdit" />
              </AppFormField>

              <AppFormField v-if="canSeeWithPay" label="Con goce de sueldo">
                <label class="switch-row">
                  <input v-model="form.withPay" type="checkbox" :disabled="config && !config.canEdit" />
                  <span>El permiso es con goce de sueldo</span>
                </label>
              </AppFormField>

              <template v-if="isTeacherAbsence">
                <AppFormField label="Horas en aula" :error="errors.hours">
                  <input v-model.number="form.hoursInClassroom" type="number" min="0" max="8" class="tecnm-form-control" placeholder="0" :disabled="config && !config.canEdit" />
                </AppFormField>

                <AppFormField label="Horas de descarga">
                  <input v-model.number="form.hoursDischarge" type="number" min="0" max="8" class="tecnm-form-control" placeholder="0" :disabled="config && !config.canEdit" />
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
                :disabled="config && !config.canEdit"
              ></textarea>
              <span v-if="errors.reason" class="tecnm-form-error">{{ errors.reason }}</span>
              <small class="char-counter" :class="counterClass">{{ form.reason.length }}/255</small>
            </div>

            <div class="tecnm-form-group">
              <label class="tecnm-label" for="evidenceFile">Evidencia (PDF opcional)</label>
              <div class="file-upload-area" :class="{ 'drag-over': dragOver }" @dragover.prevent="dragOver = true" @dragleave.prevent="dragOver = false" @drop.prevent="onDrop">
                <input id="evidenceFile" ref="fileInputRef" type="file" accept=".pdf,application/pdf" class="file-input" :disabled="config && !config.canEdit" @change="onFileChange" />
                <div v-if="!evidenceFile && !existingEvidence" class="file-upload-content">
                  <div class="file-upload-icon"><i class="fa fa-cloud-upload-alt" aria-hidden="true"></i></div>
                  <p class="file-upload-text">Subir PDF</p>
                  <p class="file-upload-hint">Haz clic o arrastra un archivo PDF aquí</p>
                </div>
                <div v-else class="file-preview">
                  <span class="file-info">
                    <i class="fa fa-file-pdf file-icon" aria-hidden="true"></i>
                    {{ evidenceFile ? evidenceFile.name : existingEvidenceName }}
                  </span>
                  <button v-if="!config || config.canEdit" type="button" class="remove-file" title="Eliminar archivo" @click="removeFile">
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
              <AppButton type="submit" variant="primary" icon="fa-save" :loading="submitting" :disabled="!formValid || (config && !config.canEdit)">Guardar cambios</AppButton>
              <AppButton type="button" variant="secondary" icon="fa-arrow-left" @click="$router.push({ name: 'permissions.index' })">Volver</AppButton>
              <AppButton type="button" variant="danger" icon="fa-ban" :disabled="config && !config.canEdit" @click="remove">Cancelar</AppButton>
            </div>
          </form>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppPageHeader from '@/shared/components/AppPageHeader.vue'
import AppButton from '@/shared/components/AppButton.vue'
import AppFormField from '@/shared/components/AppFormField.vue'
import AppSpinner from '@/shared/components/AppSpinner.vue'
import {
  cancelPermission,
  fetchEditConfig,
  updatePermission,
  type PermissionCreatePayload,
  type PermissionEditConfigDto,
  type PermissionTypeEditConfigDto,
  type PermissionTypeFieldConfigDto
} from '@/features/permissions/api/permissions'
import { combineDateTimeLocal, calculateDays, toDateInputValue, toTimeInputValue } from '@/shared/composables/useDates'
import { askConfirmation, handleAlertResponse, showToast } from '@/shared/composables/useAlerts'
import { useAuthStore } from '@/features/auth/stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const id = Number(route.params.id)

const config = ref<PermissionEditConfigDto | null>(null)
const loading = ref(true)
const submitting = ref(false)
const errorGeneral = ref('')
const errors = reactive<Record<string, string>>({})
const evidenceFile = ref<File | null>(null)
const existingEvidence = ref<string | null>(null)
const clearEvidence = ref(false)
const fileInputRef = ref<HTMLInputElement | null>(null)
const dragOver = ref(false)

const form = reactive({
  permissionTypeId: undefined as number | undefined,
  startDate: '',
  startTime: '',
  endDate: '',
  endTime: '',
  withPay: true,
  reason: '',
  hoursInClassroom: undefined as number | undefined,
  hoursDischarge: undefined as number | undefined
})

const permissionTypes = computed<PermissionTypeEditConfigDto[]>(() => config.value?.availableTypes ?? [])

const selectedConfig = computed<PermissionTypeEditConfigDto | null>(() => {
  return config.value?.availableTypes.find((t) => t.id === form.permissionTypeId) ?? null
})

const isTeacherAbsence = computed(() => form.permissionTypeId === 5)

const totalHours = computed(() => (form.hoursInClassroom ?? 0) + (form.hoursDischarge ?? 0))

const canSeeWithPay = computed(() => authStore.isAuthorizer || authStore.isAdmin || authStore.isRH)

const date1Config = computed<PermissionTypeFieldConfigDto>(() => selectedConfig.value?.date1 ?? { show: true, label: 'Fecha de inicio', required: true })
const date2Config = computed<PermissionTypeFieldConfigDto>(() => selectedConfig.value?.date2 ?? { show: true, label: 'Fecha de fin', required: false })
const time1Config = computed<PermissionTypeFieldConfigDto>(() => selectedConfig.value?.time1 ?? { show: false, label: 'Hora de entrada', required: false })
const time2Config = computed<PermissionTypeFieldConfigDto>(() => selectedConfig.value?.time2 ?? { show: false, label: 'Hora de salida', required: false })

const days = computed(() => {
  const start = combineDateTimeLocal(form.startDate, form.startTime)
  const end = combineDateTimeLocal(form.endDate, form.endTime, true)
  if (!start || !end) return null
  return calculateDays(start, end)
})

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

const existingEvidenceName = computed(() => {
  if (!existingEvidence.value) return 'Evidencia adjunta'
  const parts = existingEvidence.value.split(/[\\/]/)
  return parts[parts.length - 1]
})

const formValid = computed(() => {
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
  if (!form.permissionTypeId) errors.permissionTypeId = 'Seleccione el tipo de permiso.'
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
    errors.datesInverted = 'La fecha de fin debe ser posterior a la fecha de inicio.'
  }
  if (isTeacherAbsence.value) {
    const th = totalHours.value
    if (th < 1) errors.hours = 'Debes capturar al menos 1 hora.'
    else if (th > 8) errors.hours = 'El total de horas no puede superar 8.'
    else delete errors.hours
  }
  const reason = form.reason.trim()
  if (!reason) errors.reason = 'El motivo es obligatorio.'
  else if (reason.length > 255) errors.reason = 'El motivo no puede exceder 255 caracteres.'
  else delete errors.reason
}

function typeIcon(t: PermissionTypeEditConfigDto): string {
  return t.icon || 'fa-file-alt'
}

function typeSubtitle(t: PermissionTypeEditConfigDto): string {
  return t.subtitle || `Permiso de tipo ${t.description}`
}

function validateEvidence(file: File): string | null {
  const maxMb = config.value?.maxFileSizeMB ?? 5
  const allowed = config.value?.allowedEvidenceExtensions ?? ['.pdf']
  const isAllowed = allowed.some((ext) => file.name.toLowerCase().endsWith(ext.toLowerCase()))
  if (!isAllowed) {
    return `Solo se permiten archivos: ${allowed.join(', ')}`
  }
  if (file.size > maxMb * 1024 * 1024) {
    return `El archivo no debe superar los ${maxMb}MB.`
  }
  return null
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
  existingEvidence.value = null
  clearEvidence.value = true
  if (fileInputRef.value) fileInputRef.value.value = ''
  delete errors.evidence
}

onMounted(async () => {
  try {
    const editConfig = await fetchEditConfig(id)
    config.value = editConfig

    if (!editConfig.canEdit && editConfig.blockReason) {
      showToast(editConfig.blockReason, 'warning')
    }

    form.permissionTypeId = editConfig.permissionTypeId
    form.startDate = toDateInputValue(editConfig.startDate)
    form.startTime = toTimeInputValue(editConfig.startDate)
    form.endDate = toDateInputValue(editConfig.endDate)
    form.endTime = toTimeInputValue(editConfig.endDate)
    form.withPay = editConfig.withPay
    form.reason = editConfig.reason || ''
    form.hoursInClassroom = editConfig.hoursInClassroom || undefined
    form.hoursDischarge = editConfig.hoursDischarge || undefined
    existingEvidence.value = editConfig.evidenceUrl || null
    clearEvidence.value = false
  } catch (err) {
    console.error(err)
    showToast('No se pudo cargar la configuración del permiso.', 'error')
    router.push({ name: 'permissions.index' })
  } finally {
    loading.value = false
  }
})

async function submit() {
  if (config.value && !config.value.canEdit) {
    showToast(config.value.blockReason || 'Este permiso no es editable.', 'error')
    return
  }
  errorGeneral.value = ''
  validate()
  if (Object.keys(errors).length > 0) return

  const payload: PermissionCreatePayload = {
    reason: form.reason,
    startDate: combineDateTimeLocal(form.startDate, form.startTime) as string,
    endDate: date2Config.value.show ? (combineDateTimeLocal(form.endDate, form.endTime, true) as string) : undefined,
    permissionTypeId: form.permissionTypeId as number,
    withPay: form.withPay,
    evidence: evidenceFile.value,
    clearEvidence: clearEvidence.value,
    hoursInClassroom: isTeacherAbsence.value ? form.hoursInClassroom : undefined,
    hoursDischarge: isTeacherAbsence.value ? form.hoursDischarge : undefined
  }

  submitting.value = true
  try {
    const result = await updatePermission(id, payload)
    if (result?.showConfirmation) {
      await handleAlertResponse({
        success: false,
        title: result.title || 'Excede el periodo',
        message: result.message,
        icon: (result.icon as 'warning') || 'warning',
        confirmButtonText: result.confirmButtonText
      })
      await updatePermission(id, { ...payload, confirmed: true })
    }
    await handleAlertResponse({
      success: true,
      title: '¡Cambios guardados!',
      message: 'El permiso fue actualizado correctamente.'
    })
    router.push({ name: 'permissions.details', params: { id: String(id) } })
  } catch (err) {
    console.error(err)
    const data = (err as { response?: { data?: { message?: string; title?: string } } })?.response?.data
    if (data?.message) {
      await handleAlertResponse({ success: false, title: data.title || 'Error', message: data.message })
    } else {
      errorGeneral.value = 'No se pudo actualizar el permiso.'
    }
  } finally {
    submitting.value = false
  }
}

async function remove() {
  const ok = await askConfirmation({
    title: '¿Cancelar permiso?',
    text: 'Esta acción cancelará el permiso. No podrá revertirse.',
    confirmText: 'Sí, cancelar',
    danger: true
  })
  if (!ok) return
  try {
    await cancelPermission(id)
    await handleAlertResponse({ success: true, title: 'Cancelado', message: 'El permiso fue cancelado.' })
    router.push({ name: 'permissions.index' })
  } catch (err) {
    console.error(err)
    showToast('No se pudo cancelar el permiso.', 'error')
  }
}
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

/* Permission type cards (config-driven, like legacy edit.js) */
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
  color: var(--tecnm-success);
}

.char-counter.warning {
  color: var(--tecnm-warning);
}

.char-counter.error {
  color: var(--tecnm-danger);
}

.file-upload-area {
  border: 2px dashed var(--tecnm-border-color);
  border-radius: var(--tecnm-radius-md);
  padding: 1.5rem;
  text-align: center;
  cursor: pointer;
  position: relative;
  transition: var(--tecnm-transition);
  background-color: var(--tecnm-bg-main);
}

.file-upload-area:hover,
.file-upload-area.drag-over {
  border-color: var(--tecnm-accent-text);
  background-color: var(--tecnm-surface-white);
}

.file-input {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  opacity: 0;
  cursor: pointer;
}

.file-upload-icon {
  font-size: 2rem;
  color: var(--tecnm-accent-text);
  margin-bottom: 0.5rem;
}

.file-upload-text {
  font-weight: 600;
  margin: 0 0 0.25rem;
}

.file-upload-hint {
  font-size: 0.8rem;
  color: var(--tecnm-text-secondary);
  margin: 0;
}

.file-preview {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.5rem 0.75rem;
  background-color: var(--tecnm-surface-white);
  border-radius: var(--tecnm-radius-sm);
  border: 1px solid var(--tecnm-border-color);
}

.file-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.9rem;
  font-weight: 500;
}

.file-icon {
  color: var(--tecnm-danger);
  font-size: 1.1rem;
}

.remove-file {
  background: none;
  border: none;
  color: var(--tecnm-text-secondary);
  cursor: pointer;
  padding: 0.25rem 0.5rem;
  border-radius: var(--tecnm-radius-sm);
  transition: var(--tecnm-transition);
}

.remove-file:hover {
  color: var(--tecnm-danger);
}

.form-actions {
  display: flex;
  gap: 0.75rem;
  margin-top: 1.5rem;
  flex-wrap: wrap;
}
</style>
