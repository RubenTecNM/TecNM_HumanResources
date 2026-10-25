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
        <AppButton v-if="canCancel" variant="danger" icon="fa-ban" @click="onCancel">
          Cancelar
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
          <section class="detail-section">
            <h5 class="detail-section-header"><i class="fa fa-user" aria-hidden="true"></i> Identificación</h5>
            <dl class="detail-fields-grid">
              <div class="detail-field field-span-6">
                <dt class="detail-field-label">Solicitante</dt>
                <dd class="field-value-emphasis">{{ permission?.requester || '—' }}</dd>
              </div>
              <div class="detail-field field-span-6">
                <dt class="detail-field-label">Tipo de permiso</dt>
                <dd class="field-value-emphasis">{{ permission?.type || '—' }}</dd>
              </div>
            </dl>
          </section>

          <!-- Periodo -->
          <section class="detail-section">
            <h5 class="detail-section-header"><i class="fa fa-calendar" aria-hidden="true"></i> Periodo y condiciones</h5>
            <dl class="detail-fields-grid">
              <div class="detail-field field-span-3">
                <dt class="detail-field-label">Inicio</dt>
                <dd class="field-value">{{ formatTecNMDateTime(permission?.startDate) }}</dd>
              </div>
              <div class="detail-field field-span-3">
                <dt class="detail-field-label">Fin</dt>
                <dd class="field-value">{{ formatTecNMDateTime(permission?.endDate) }}</dd>
              </div>
              <div class="detail-field field-span-3">
                <dt class="detail-field-label">Días</dt>
                <dd :class="permission?.days == null ? 'field-value-empty' : 'field-value'">{{ permission?.days ?? 'Sin registrar' }}</dd>
              </div>
              <div class="detail-field field-span-3">
                <dt class="detail-field-label">Con goce</dt>
                <dd>
                  <StatusBadge :variant="permission?.withPay ? 'approved' : 'info'">
                    {{ permission?.withPay ? 'Sí' : 'No' }}
                  </StatusBadge>
                </dd>
              </div>
              <template v-if="permission?.permissionTypeId === 5">
                <div class="detail-field field-span-4">
                  <dt class="detail-field-label">Turno</dt>
                  <dd :class="permission?.shiftDescription ? 'field-value' : 'field-value-empty'">{{ permission?.shiftDescription || 'Sin registrar' }}</dd>
                </div>
                <div class="detail-field field-span-4">
                  <dt class="detail-field-label">Horas en aula</dt>
                  <dd :class="permission?.hoursInClassroom == null ? 'field-value-empty' : 'field-value'">{{ permission?.hoursInClassroom ?? '—' }}</dd>
                </div>
                <div class="detail-field field-span-4">
                  <dt class="detail-field-label">Horas de descarga</dt>
                  <dd :class="permission?.hoursDischarge == null ? 'field-value-empty' : 'field-value'">{{ permission?.hoursDischarge ?? '—' }}</dd>
                </div>
              </template>
            </dl>
          </section>

          <!-- Estado y revisión -->
          <section class="detail-section">
            <h5 class="detail-section-header"><i class="fa fa-gavel" aria-hidden="true"></i> Estado y revisión</h5>
            <dl class="detail-fields-grid">
              <div class="detail-field field-span-3">
                <dt class="detail-field-label">Estado</dt>
                <dd>
                  <StatusBadge :variant="statusVariant(permission?.status)">
                    {{ statusLabel(permission?.status) }}
                  </StatusBadge>
                </dd>
              </div>
              <template v-if="permission?.status !== PERMISSION_STATUS.PENDING">
                <div class="detail-field field-span-4">
                  <dt class="detail-field-label">Autorizado por</dt>
                  <dd class="field-value">{{ permission?.authorizedBy || 'Sistema' }}</dd>
                </div>
                <div class="detail-field field-span-4">
                  <dt class="detail-field-label">Fecha de revisión</dt>
                  <dd :class="formatTecNMDateTime(permission?.authorizedAt) ? 'field-value' : 'field-value-empty'">{{ formatTecNMDateTime(permission?.authorizedAt) || '—' }}</dd>
                </div>
                <div class="detail-field field-span-12">
                  <dt class="detail-field-label">Comentario de revisión</dt>
                  <dd :class="permission?.authorizationComment ? 'note-text field-value' : 'note-text field-value-empty'">
                    {{ permission?.authorizationComment || 'Sin comentario de revisión' }}
                  </dd>
                </div>
              </template>
            </dl>
          </section>

          <!-- Motivo -->
          <section class="detail-section">
            <h5 class="detail-section-header"><i class="fa fa-file-text-o" aria-hidden="true"></i> Motivo</h5>
            <p :class="permission?.reason ? 'reason-text field-value' : 'reason-text field-value-empty'">{{ permission?.reason || 'Sin motivo registrado' }}</p>
          </section>

          <!-- Metadata -->
          <section class="detail-section detail-section--meta">
            <h5 class="detail-section-header"><i class="fa fa-database" aria-hidden="true"></i> Metadata del registro</h5>
            <dl class="detail-fields-grid">
              <div class="detail-field field-span-4">
                <dt class="detail-field-label">Fecha de creación</dt>
                <dd class="field-value">{{ formatTecNMDateTime(permission?.createdAt) }}</dd>
              </div>
              <div class="detail-field field-span-4">
                <dt class="detail-field-label">Corte</dt>
                <dd :class="permission?.cutoff ? 'field-value' : 'field-value-empty'">{{ permission?.cutoff || 'Sin corte asignado' }}</dd>
              </div>
            </dl>
          </section>

          <div v-if="permission?.evidence" class="tecnm-form-group">
            <h4 class="detail-section-header"><i class="fa fa-paperclip" aria-hidden="true"></i> Evidencia</h4>
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
import AppPageHeader from '@/shared/components/AppPageHeader.vue'
import AppButton from '@/shared/components/AppButton.vue'
import AppSpinner from '@/shared/components/AppSpinner.vue'
import StatusBadge from '@/shared/components/StatusBadge.vue'
import { fetchPermission, cancelPermission, statusLabel, statusVariant, PERMISSION_STATUS, type PermissionDetail } from "@/features/permissions/api/permissions"'
import { askConfirmation, showToast } from '@/shared/composables/useAlerts'
import { formatTecNMDate, formatTecNMDateTime } from '@/shared/composables/useDates'
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

const canCancel = computed(() => {
  if (permission.value?.status !== PERMISSION_STATUS.PENDING) return false
  const isOwner = permission.value.requestingUserId != null && permission.value.requestingUserId === authStore.user?.id
  return isOwner || authStore.isRHOrAdmin
})

async function onCancel() {
  if (!permission.value) return
  const ok = await askConfirmation({
    title: '¿Cancelar permiso?',
    text: 'Esta acción no puede revertirse.',
    confirmText: 'Sí, cancelar',
    danger: true
  })
  if (!ok) return
  try {
    await cancelPermission(permission.value.id)
    showToast('Permiso cancelado correctamente', 'success')
    permission.value = await fetchPermission(permission.value.id)
  } catch (err) {
    console.error('Error al cancelar permiso:', err)
    showToast('No se pudo cancelar el permiso.', 'error')
  }
}

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
/* ── Grid estandarizado de 12 columnas ─────────────────── */
.detail-fields-grid {
  display: grid;
  grid-template-columns: repeat(12, 1fr);
  gap: 24px 32px;
}

.field-span-3 { grid-column: span 3; }
.field-span-4 { grid-column: span 4; }
.field-span-6 { grid-column: span 6; }
.field-span-12 { grid-column: span 12; }

/* ── Secciones lógicas ─────────────────────────────────── */
.detail-section {
  padding-bottom: 32px;
  margin-bottom: 32px;
  border-bottom: 1px solid rgba(0, 0, 0, 0.06); /* separador más sutil que el header */
}
.detail-section:last-of-type {
  border-bottom: none;
  margin-bottom: 0;
  padding-bottom: 0;
}

.detail-section-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  color: #8b6f2f; /* dorado/ámbar, mismo tono que el borde superior de la card */
  margin-bottom: 16px;
}
.detail-section-header i {
  width: 14px;
  height: 14px;
  font-size: 14px;
  line-height: 14px;
  color: #b8935a;
}

/* ── Campos ────────────────────────────────────────────── */
.detail-field {
  margin-bottom: 0; /* el gap del grid maneja el espaciado */
}

.detail-field-label {
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  color: #6b7280;
  margin-bottom: 4px;
}

dl.detail-fields-grid dt { font-size: inherit; } /* neutraliza estilos globales de dl */
dd.field-value,
dd.field-value-emphasis,
dd.field-value-empty,
.note-text,
.reason-text { margin: 0; }

/* ── Jerarquía de valores ──────────────────────────────── */
.field-value {
  font-size: 14px;
  font-weight: 500;
  color: #1f2937;
}

.field-value-emphasis {
  font-size: 15px;
  font-weight: 700;
  color: #1e3a5f; /* mismo azul oscuro del header, continuidad visual */
}

.field-value-empty {
  color: #9ca3af;
  font-style: italic;
  font-weight: 400;
}

/* ── Texto libre largo (motivo, comentarios) ───────────── */
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

.detail-section--meta .field-value { font-size: 13px; }

/* ── Responsive: colapsar spans en pantallas chicas ────── */
@media (max-width: 992px) {
  .field-span-3, .field-span-4 { grid-column: span 6; }
}
@media (max-width: 600px) {
  .detail-fields-grid { gap: 20px 16px; }
  .field-span-3, .field-span-4, .field-span-6 { grid-column: span 12; }
}

/* ── Avatar del header de card (intacto) ───────────────── */
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
</style>