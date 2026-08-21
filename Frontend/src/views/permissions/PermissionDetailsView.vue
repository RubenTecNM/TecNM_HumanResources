<template>
  <div>
    <AppPageHeader title="Detalle de Permiso" subtitle="Información de la solicitud" icon="fa-file-text-o">
      <template #actions>
        <AppButton variant="secondary" icon="fa-arrow-left" @click="$router.push({ name: 'permissions.index' })">
          Volver al Listado
        </AppButton>
        <AppButton v-if="canEdit" variant="outline" icon="fa-edit" @click="$router.push({ name: 'permissions.edit', params: $route.params })">
          Editar
        </AppButton>
        <AppButton v-if="canAuthorize" variant="accent" icon="fa-check" @click="$router.push({ name: 'permissions.authorize', params: $route.params })">
          Autorizar
        </AppButton>
      </template>
    </AppPageHeader>

    <AppSpinner v-if="loading" />
    <template v-else>
      <section class="tecnm-card">
        <div class="tecnm-card-header tecnm-card-header--premium">
          <div class="details-avatar">{{ permission?.requester?.charAt(0) || 'U' }}</div>
          <div>
            <h4 class="filter-title">Resumen de la Solicitud</h4>
            <small class="details-id">Permiso #{{ permission?.id }}</small>
          </div>
        </div>
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
              <dt>Días</dt>
              <dd>{{ permission?.days ?? '—' }}</dd>
            </div>
            <div class="detail-item">
              <dt>Inicio</dt>
              <dd>{{ formatTecNMDateTime(permission?.startDate) }}</dd>
            </div>
            <div class="detail-item">
              <dt>Fin</dt>
              <dd>{{ formatTecNMDateTime(permission?.endDate) }}</dd>
            </div>
            <div class="detail-item">
              <dt>Con goce</dt>
              <dd>
                <StatusBadge :variant="permission?.withPay ? 'approved' : 'info'">
                  {{ permission?.withPay ? 'Sí' : 'No' }}
                </StatusBadge>
              </dd>
            </div>
            <div class="detail-item">
              <dt>Estado</dt>
              <dd>
                <StatusBadge :variant="statusVariant(permission?.status)">
                  {{ statusLabel(permission?.status) }}
                </StatusBadge>
              </dd>
            </div>
            <div class="detail-item">
              <dt>Corte</dt>
              <dd>{{ permission?.cutoff || '—' }}</dd>
            </div>
            <template v-if="permission?.permissionTypeId === 5">
              <div class="detail-item">
                <dt>Turno</dt>
                <dd>{{ permission?.shiftDescription || '—' }}</dd>
              </div>
              <div class="detail-item">
                <dt>Horas en aula</dt>
                <dd>{{ permission?.hoursInClassroom ?? '—' }}</dd>
              </div>
              <div class="detail-item">
                <dt>Horas de descarga</dt>
                <dd>{{ permission?.hoursDischarge ?? '—' }}</dd>
              </div>
            </template>
            <div class="detail-item">
              <dt>Fecha de creación</dt>
              <dd>{{ formatTecNMDateTime(permission?.createdAt) }}</dd>
            </div>
            <template v-if="permission?.status !== PERMISSION_STATUS.PENDING">
              <div class="detail-item">
                <dt>Autorizado por</dt>
                <dd>{{ permission?.authorizedBy || 'Sistema' }}</dd>
              </div>
              <div class="detail-item">
                <dt>Fecha de revisión</dt>
                <dd>{{ formatTecNMDateTime(permission?.authorizedAt) || '—' }}</dd>
              </div>
              <div class="detail-item">
                <dt>Comentario de revisión</dt>
                <dd>{{ permission?.authorizationComment || 'Sin comentario' }}</dd>
              </div>
            </template>
          </dl>

          <div class="tecnm-form-group">
            <h4 class="detail-section">Motivo</h4>
            <p class="reason-text">{{ permission?.reason || '—' }}</p>
          </div>

          <div v-if="permission?.evidence" class="tecnm-form-group">
            <h4 class="detail-section">Evidencia</h4>
            <a :href="`/api/Permissions/${permission?.id}/evidence`" target="_blank" class="tecnm-btn tecnm-btn-outline tecnm-btn-sm">
              <i class="fa fa-paperclip" aria-hidden="true"></i> Ver evidencia
            </a>
          </div>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import AppPageHeader from '@/components/AppPageHeader.vue'
import AppButton from '@/components/AppButton.vue'
import AppSpinner from '@/components/AppSpinner.vue'
import StatusBadge from '@/components/StatusBadge.vue'
import { fetchPermission, statusLabel, statusVariant, PERMISSION_STATUS, type PermissionDetail } from '@/api/permissions'
import { formatTecNMDate, formatTecNMDateTime } from '@/composables/useDates'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const authStore = useAuthStore()
const id = Number(route.params.id)

const permission = ref<PermissionDetail | null>(null)
const loading = ref(true)

const canEdit = computed(() => {
  // Los jefes de área no editan solicitudes (solo revisan/autorizan); el resto sí si está pendiente
  return permission.value?.status === PERMISSION_STATUS.PENDING && !authStore.isAuthorizer
})

const canAuthorize = computed(() => {
  return permission.value?.status === PERMISSION_STATUS.PENDING && (authStore.isAdmin || authStore.isAuthorizer)
})

onMounted(async () => {
  try {
    permission.value = await fetchPermission(id)
  } catch (err) {
    console.error('Error cargando detalle:', err)
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.detail-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1rem 1.5rem;
  margin-bottom: 1.5rem;
}

.details-avatar {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: linear-gradient(135deg, var(--tecnm-blue-primary), var(--tecnm-blue-dark));
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.2rem;
  font-weight: 700;
  flex-shrink: 0;
  box-shadow: var(--tecnm-shadow-md);
}

.details-id {
  color: var(--tecnm-text-secondary);
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
</style>