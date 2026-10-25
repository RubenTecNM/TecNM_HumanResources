<template>
  <div>
    <AppPageHeader
      title="Apertura de Nuevo Periodo"
      subtitle="Creación de periodo quincenal de gestión de permisos"
      icon="fa-calendar-plus"
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
            <div v-if="loadingDates" class="text-center py-4">
              <i class="fas fa-spinner fa-spin fa-2x text-primary"></i>
              <p class="mt-2 text-muted">Calculando fechas sugeridas...</p>
            </div>

            <form v-else @submit.prevent="handleSubmit" class="row g-3">
              <div class="col-12">
                <div class="alert alert-info">
                  <i class="fas fa-info-circle me-2"></i>
                  La fecha de inicio ha sido sugerida en función del fin del último periodo registrado.
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
                  :disabled="submitting"
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
                  :disabled="submitting"
                />
              </div>

              <div v-if="errorMessage" class="col-12">
                <div class="alert alert-danger">
                  <i class="fas fa-exclamation-triangle me-2"></i> {{ errorMessage }}
                </div>
              </div>

              <div class="col-12 text-end mt-4">
                <router-link to="/cutoffs" class="btn btn-secondary me-2">
                  Cancelar
                </router-link>
                <button type="submit" class="btn btn-primary" :disabled="submitting">
                  <i v-if="submitting" class="fas fa-spinner fa-spin me-2"></i>
                  <i v-else class="fas fa-save me-2"></i>
                  Guardar Periodo
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
import { useRouter } from 'vue-router'
import { http } from "@/shared/api/client"'
import AppPageHeader from '@/shared/components/AppPageHeader.vue'
import { showToast } from '@/shared/composables/useAlerts'

const router = useRouter()

const startDate = ref('')
const endDate = ref('')
const loadingDates = ref(true)
const submitting = ref(false)
const errorMessage = ref('')

onMounted(async () => {
  try {
    const res = await http.get('/api/Cutoffs/suggested-dates')
    const data = res.data
    startDate.value = data.startDate ? data.startDate.split('T')[0] : ''
    endDate.value = data.endDate ? data.endDate.split('T')[0] : ''
  } catch (e) {
    console.error(e)
  } finally {
    loadingDates.value = false
  }
})

async function handleSubmit() {
  if (!startDate.value || !endDate.value) return

  if (new Date(endDate.value) <= new Date(startDate.value)) {
    errorMessage.value = 'La fecha de término debe ser posterior a la fecha de inicio.'
    return
  }

  submitting.value = true
  errorMessage.value = ''

  try {
    const res = await http.post('/api/Cutoffs', {
      startDate: startDate.value,
      endDate: endDate.value
    })

    if (res.status === 200 || res.status === 201) {
      showToast('Nuevo periodo registrado correctamente.', 'success')
      router.push('/cutoffs')
    }
  } catch (err: any) {
    errorMessage.value = err.response?.data?.message || 'Error al registrar el nuevo periodo.'
  } finally {
    submitting.value = false
  }
}
</script>
