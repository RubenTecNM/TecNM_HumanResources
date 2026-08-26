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
          <!-- Identificación -->
          <section class="detail-group">
            <h5 class="detail-group-title"><i class="fa fa-user" aria-hidden="true"></i> Identificación</h5>
            <dl class="detail-grid">
              <div class="detail-item detail-item--wide detail-item--strong">
                <dt><i class="fa fa-id-card-o" aria-hidden="true"></i> Solicitante</dt>
                <dd>{{ permission?.requester || '—' }}</dd>
              </div>
              <div class="detail-item detail-item--strong">
                <dt><i class="fa fa-tag" aria-hidden="true"></i> Tipo de permiso</dt>
                <dd>{{ permission?.type || '—' }}</dd>
              </div>
            </dl>
          </section>

          <!-- Periodo -->
          <section class="detail-group">
            <h5 class="detail-group-title"><i class="fa fa-calendar" aria-hidden="true"></i> Periodo y condiciones</h5>
            <dl class="detail-grid">
              <div class="detail-item">
                <dt><i class="fa fa-play" aria-hidden="true"></i> Inicio</dt>
                <dd>{{ formatTecNMDateTime(permission?.startDate) }}</dd>
              </div>
              <div class="detail-item">
                <dt><i class="fa fa-stop" aria-hidden="true"></i> Fin</dt>
                <dd>{{ formatTecNMDateTime(permission?.endDate) }}</dd>
              </div>
              <div class="detail-item">
                <dt><i class="fa fa-clock-o" aria-hidden="true"></i> Días</dt>
                <dd :class="{ 'is-empty': permission?.days == null }">{{ permission?.days ?? 'Sin registrar' }}</dd>
              </div>
              <div class="detail-item">
                <dt><i class="fa fa-money" aria-hidden="true"></i> Con goce</dt>
                <dd>
                  <StatusBadge :variant="permission?.withPay ? 'approved' : 'info'">
                    {{ permission?.withPay ? 'Sí' : 'No' }}
                  </StatusBadge>
                </dd>
              </div>
              <template v-if="permission?.permissionTypeId === 5">
                <div class="detail-item">
                  <dt><i class="fa fa-sun-o" aria-hidden="true"></i> Turno</dt>
                  <dd :class="{ 'is-empty': !permission?.shiftDescription }">{{ permission?.shiftDescription || 'Sin registrar' }}</dd>
                </div>
                <div class="detail-item">
                  <dt><i class="fa fa-graduation-cap" aria-hidden="true"></i> Horas en aula</dt>
                  <dd :class="{ 'is-empty': permission?.hoursInClassroom == null }">{{ permission?.hoursInClassroom ?? '—' }}</dd>
                </div>
                <div class="detail-item">
                  <dt><i class="fa fa-book" aria-hidden="true"></i> Horas de descarga</dt>
                  <dd :class="{ 'is-empty': permission?.hoursDischarge == null }">{{ permission?.hoursDischarge ?? '—' }}</dd>
                </div>
              </template>
            </dl>
          </section>

          <!-- Estado y revisión -->
          <section class="detail-group">
            <h5 class="detail-group-title"><i class="fa fa-gavel" aria-hidden="true"></i> Estado y revisión</h5>
            <dl class="detail-grid">
              <div class="detail-item">
                <dt><i class="fa fa-flag" aria-hidden="true"></i> Estado</dt>
                <dd>
                  <StatusBadge :variant="statusVariant(permission?.status)">
                    {{ statusLabel(permission?.status) }}
                  </StatusBadge>
                </dd>
              </div>
              <template v-if="permission?.status !== PERMISSION_STATUS.PENDING">
                <div class="detail-item">
                  <dt><i class="fa fa-user-secret" aria-hidden="true"></i> Autorizado por</dt>
                  <dd>{{ permission?.authorizedBy || 'Sistema' }}</dd>
                </div>
                <div class="detail-item">
                  <dt><i class="fa fa-calendar-check-o" aria-hidden="true"></i> Fecha de revisión</dt>
                  <dd :class="{ 'is-empty': !formatTecNMDateTime(permission?.authorizedAt) }">{{ formatTecNMDateTime(permission?.authorizedAt) || '—' }}</dd>
                </div>
                <div class="detail-item detail-item--wide">
                  <dt><i class="fa fa-comment-o" aria-hidden="true"></i> Comentario de revisión</dt>
                  <dd :class="permission?.authorizationComment ? 'note-text' : 'is-empty note-text--empty'">
                    {{ permission?.authorizationComment || 'Sin comentario de revisión' }}
                  </dd>
                </div>
              </template>
            </dl>
          </section>

          <!-- Motivo -->
          <section class="detail-group">
            <h5 class="detail-group-title"><i class="fa fa-file-text-o" aria-hidden="true"></i> Motivo</h5>
            <p :class="permission?.reason ? 'reason-text' : 'reason-text is-empty'">{{ permission?.reason || 'Sin motivo registrado' }}</p>
          </section>

          <!-- Metadata -->
          <section class="detail-group detail-group--meta">
            <h5 class="detail-group-title"><i class="fa fa-database" aria-hidden="true"></i> Metadata del registro</h5>
            <dl class="detail-grid">
              <div class="detail-item">
                <dt><i class="fa fa-calendar-plus-o" aria-hidden="true"></i> Fecha de creación</dt>
                <dd>{{ formatTecNMDateTime(permission?.createdAt) }}</dd>
              </div>
              <div class="detail-item">
                <dt><i class="fa fa-folder-open-o" aria-hidden="true"></i> Corte</dt>
                <dd :class="{ 'is-empty': !permission?.cutoff }">{{ permission?.cutoff || 'Sin corte asignado' }}</dd>
              </div>
            </dl>
          </section>

          <div v-if="permission?.evidence" class="tecnm-form-group">
            <h4 class="detail-section"><i class="fa fa-paperclip" aria-hidden="true"></i> Evidencia</h4>
            <a :href="`/api/Permissions/${permission?.id}/evidence`" target="_blank" class="tecnm-btn tecnm-btn-outline tecnm-btn-sm">
              <i class="fa fa-download" aria-hidden="true"></i> Ver evidencia
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
  gap: 0.9rem 1.5rem;
}

/* Secciones lógicas dentro de la card */
.detail-group {
  padding: 1rem 0;
}
.detail-group + .detail-group {
  border-top: 1px solid var(--tecnm-border-light, #eef1f5);
}
.detail-group--meta .detail-grid {
  font-size: 0.92rem;
}

.detail-group-title {
  font-size: 0.82rem;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--tecnm-text-secondary);
  margin-bottom: 0.7rem;
}
.detail-group-title i {
  color: var(--tecnm-gold-accent);
  width: 1.2rem;
  text-align: center;
}

.detail-item dt {
  font-size: 0.78rem;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  color: var(--tecnm-text-secondary);
  font-weight: 600;
  margin-bottom: 0.2rem;
}
.detail-item dt i {
  color: var(--tecnm-text-secondary);
  opacity: 0.75;
  width: 1.15rem;
  text-align: center;
  font-size: 0.85em;
}

.detail-item dd {
  margin: 0;
  font-weight: 500;
}

/* Campos clave destacan sobre los secundarios */
.detail-item--strong dd {
  font-weight: 700;
  font-size: 1.02rem;
  color: var(--tecnm-blue-dark);
}
.detail-item--wide {
  grid-column: 1 / -1;
}

/* Valores vacíos discretos */
.is-empty {
  color: var(--tecnm-text-muted, #a3adb8);
  font-style: italic;
  font-weight: 400 !important;
}

/* Texto libre largo (motivo, comentarios) */
.reason-text,
.note-text {
  background-color: var(--tecnm-bg-main);
  padding: 0.9rem 1.1rem;
  border-radius: var(--tecnm-radius-sm);
  border-left: 3px solid var(--tecnm-gold-accent);
  white-space: pre-wrap;
}
.note-text {
  border-left-color: var(--tecnm-blue-primary);
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

.detail-section {
  color: var(--tecnm-accent-text);
  margin-bottom: 0.5rem;
}
</style>