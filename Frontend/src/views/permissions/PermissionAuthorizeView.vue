<template>
  <div>
    <AppPageHeader title="Autorizar Permiso" subtitle="Revisa el permiso y decide si se autoriza" icon="fa-check-circle" />

    <AppSpinner v-if="loading" />
    <template v-else>
      <section class="tecnm-card tecnm-mb-4">
        <div class="tecnm-card-body">
          <dl class="detail-grid">
            <div class="detail-item">
              <dt>Solicitante</dt>
              <dd>{{ permission?.requester }}</dd>
            </div>
            <div class="detail-item">
              <dt>Tipo</dt>
              <dd>{{ permission?.type }}</dd>
            </div>
            <div class="detail-item">
              <dt>Vigencia</dt>
              <dd>{{ formatTecNMDate(permission?.startDate) }} → {{ formatTecNMDate(permission?.endDate) }}</dd>
            </div>
            <div class="detail-item">
              <dt>Con goce</dt>
              <dd>{{ permission?.withPay ? 'Sí' : 'No' }}</dd>
            </div>
            <div class="detail-item">
              <dt>Corte</dt>
              <dd>{{ permission?.cutoff || '—' }}</dd>
            </div>
          </dl>

          <h4 class="detail-section">Motivo</h4>
          <p class="reason-text">{{ permission?.reason || '—' }}</p>
        </div>
      </section>

      <section class="tecnm-card">
        <div class="tecnm-card-header tecnm-card-header--premium">
          <h4 class="filter-title"><i class="fa fa-gavel" aria-hidden="true"></i> Decisión de Autorización</h4>
        </div>
        <div class="tecnm-card-body">
<form @submit.prevent="submit(true)">
            <div class="tecnm-form-group">
              <label class="tecnm-label">¿El permiso es con goce de sueldo?</label>
              <div class="radio-row">
                <label class="radio-item">
                  <input v-model="withPay" type="radio" :value="true" />
                  <span>Con goce</span>
                </label>
                <label class="radio-item">
                  <input v-model="withPay" type="radio" :value="false" />
                  <span>Sin goce</span>
                </label>
              </div>
            </div>

            <div class="tecnm-form-group">
              <label class="tecnm-label" for="comment">Comentario de la revisión</label>
              <textarea
                id="comment"
                v-model="comment"
                class="tecnm-form-control"
                rows="4"
                maxlength="500"
                placeholder="Observaciones para el solicitante (opcional)"
              ></textarea>
              <small>{{ comment.length }}/500</small>
            </div>

            <div v-if="errorGeneral" class="tecnm-alert tecnm-alert-danger tecnm-mb-4">
              <i class="fa fa-times-circle" aria-hidden="true"></i>
              <span>{{ errorGeneral }}</span>
            </div>

            <div class="form-actions">
              <AppButton type="submit" variant="primary" icon="fa-check" :loading="submitting && approving">
                Autorizar permiso
              </AppButton>
              <AppButton type="button" variant="danger" icon="fa-times" :loading="submitting && !approving" @click="submit(false)">
                Rechazar permiso
              </AppButton>
              <AppButton type="button" variant="secondary" icon="fa-arrow-left" @click="$router.back()">
                Cancelar
              </AppButton>
            </div>
          </form>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppPageHeader from '@/components/AppPageHeader.vue'
import AppButton from '@/components/AppButton.vue'
import AppSpinner from '@/components/AppSpinner.vue'
import { fetchPermission, authorizePermission, type PermissionDetail } from '@/api/permissions'
import { formatTecNMDate } from '@/composables/useDates'
import { showToast } from '@/composables/useAlerts'

const route = useRoute()
const router = useRouter()
const id = Number(route.params.id)

const permission = ref<PermissionDetail | null>(null)
const loading = ref(true)
const withPay = ref(true)
const comment = ref('')
const submitting = ref(false)
const approving = ref(true)
const errorGeneral = ref('')

onMounted(async () => {
  try {
    permission.value = await fetchPermission(id)
    withPay.value = !!permission.value?.withPay
  } catch (err) {
    console.error(err)
    showToast('No se pudo cargar el permiso.', 'error')
    router.push({ name: 'permissions.index' })
  } finally {
    loading.value = false
  }
})

async function submit(approved: boolean) {
  errorGeneral.value = ''
  approving.value = approved
  submitting.value = true
  try {
    const res = await authorizePermission(id, {
      approved,
      withPay: withPay.value,
      authorizationComment: comment.value || ''
    })
    showToast(res.message || (approved ? 'Permiso autorizado.' : 'Permiso rechazado.'), approved ? 'success' : 'error')
    router.push({ name: 'permissions.details', params: { id: String(id) } })
  } catch (err) {
    console.error(err)
    errorGeneral.value = 'No se pudo realizar la autorización.'
  } finally {
    submitting.value = false
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

.detail-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1rem 1.5rem;
  margin-bottom: 1.5rem;
}

.detail-item dt {
  font-size: 0.78rem;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  color: var(--tecnm-text-secondary);
  font-weight: 600;
  margin-bottom: 0.2rem;
}

.detail-item dd {
  margin: 0;
  font-weight: 500;
}

.detail-section {
  color: var(--tecnm-accent-text);
  margin-bottom: 0.5rem;
}

.reason-text {
  background-color: var(--tecnm-bg-main);
  padding: 0.9rem 1.1rem;
  border-radius: var(--tecnm-radius-sm);
  border-left: 3px solid var(--tecnm-gold-accent);
}

.radio-row {
  display: flex;
  gap: 1.5rem;
}

.radio-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
}

.form-actions {
  display: flex;
  gap: 0.75rem;
  flex-wrap: wrap;
}
</style>