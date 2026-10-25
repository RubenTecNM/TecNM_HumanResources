<template>
  <div>
    <AppPageHeader
      title="Editar Periodo Quincenal"
      :subtitle="cutoffId ? `Ajuste de fechas del periodo #${cutoffId}` : ''"
      icon="fa-calendar-edit"
    >
      <template #actions>
        <router-link to="/cutoffs" class="btn btn-outline-secondary">
          <i class="fas fa-arrow-left me-1"></i> Volver a la Lista
        </router-link>
      </template>
    </AppPageHeader>

    <div class="row justify-content-center">
      <div class="col-md-8 col-lg-6">
        <section class="tecnm-card">
          <div class="tecnm-card-body">
            <AppSpinner v-if="loading" />

            <div v-else-if="errorMessage" class="alert alert-danger">
              <i class="fas fa-exclamation-triangle me-2"></i> {{ errorMessage }}
            </div>

            <form v-else-if="cutoff" @submit.prevent="handleSubmit" class="row g-3">
              <div class="col-12">
                <StatusBadge :variant="cutoffStatusVariant(cutoff.cutoffStatusTypeId)">
                  {{ cutoff.statusDescription }}
                </StatusBadge>
                <div class="alert alert-info mt-2 mb-0">
                  <i class="fas fa-info-circle me-2"></i>
                  Solo se pueden editar periodos en estado <strong>abierto</strong>. El periodo no debe encimarse con otro existente.
                </div>
              </div>

              <div class="col-md-6">
                <label for="startDate" class="form-label font-weight-bold">Fecha de Inicio</label>
                <input
                  id="startDate"
                  type="date"
                  v-model="startDate"
                  class="form-control"
                  required
                  :disabled="submitting || !editable"
                />
              </div>

              <div class="col-md-6">
                <label for="endDate" class="form-label font-weight-bold">Fecha de Término</label>
                <input
                  id="endDate"
                  type="date"
                  v-model="endDate"
                  class="form-control"
                  required
                  :disabled="submitting || !editable"
                />
              </div>

              <div v-if="formError" class="col-12">
                <div class="alert alert-danger">
                  <i class="fas fa-exclamation-triangle me-2"></i> {{ formError }}
                </div>
              </div>

              <div class="col-12 text-end mt-4">
                <router-link to="/cutoffs" class="btn btn-secondary me-2">
                  Cancelar
                </router-link>
                <button type="submit" class="btn btn-primary" :disabled="submitting || !editable">
                  <i v-if="submitting" class="fas fa-spinner fa-spin me-2"></i>
                  <i v-else class="fas fa-save me-2"></i>
                  Guardar Cambios
                </button>
              </div>
            </form>
          </div>
        </section>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppPageHeader from '@/shared/components/AppPageHeader.vue'
import AppSpinner from '@/shared/components/AppSpinner.vue'
import StatusBadge from '@/shared/components/StatusBadge.vue'
import { fetchCutoff, updateCutoff, cutoffStatusVariant, type CutoffDetail } from "@/features/cutoffs/api/cutoffs"'
import { showToast } from '@/shared/composables/useAlerts'

const route = useRoute()
const router = useRouter()
const cutoffId = Number(route.params.id)

const loading = ref(true)
const editable = ref(false)
const submitting = ref(false)
const errorMessage = ref('')
const formError = ref('')
const cutoff = ref<CutoffDetail | null>(null)
const startDate = ref('')
const endDate = ref('')

onMounted(load)

async function load() {
  loading.value = true
  errorMessage.value = ''
  try {
    cutoff.value = await fetchCutoff(cutoffId)
    startDate.value = cutoff.value.startDate.split('T')[0]
    endDate.value = cutoff.value.endDate.split('T')[0]
    editable.value = cutoff.value.cutoffStatusTypeId === 1
    if (!editable.value) {
      errorMessage.value = 'Este periodo no se puede editar porque no está en estado abierto.'
    }
  } catch (e: any) {
    errorMessage.value = e.response?.data?.message || 'No se pudo cargar el periodo.'
  } finally {
    loading.value = false
  }
}

async function handleSubmit() {
  if (!startDate.value || !endDate.value) return

  if (new Date(endDate.value) <= new Date(startDate.value)) {
    formError.value = 'La fecha de término debe ser posterior a la fecha de inicio.'
    return
  }

  submitting.value = true
  formError.value = ''

  try {
    await updateCutoff(cutoffId, { startDate: startDate.value, endDate: endDate.value })
    showToast('Periodo actualizado correctamente.', 'success')
    router.push('/cutoffs')
  } catch (err: any) {
    formError.value = err.response?.data?.message || 'Error al actualizar el periodo.'
  } finally {
    submitting.value = false
  }
}
</script>