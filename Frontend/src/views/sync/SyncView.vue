<template>
  <div class="sync-view">
    <AppPageHeader
      title="Sincronización de Datos"
      subtitle="Cargas masivas desde Excel con comparación contra la base de datos"
    >
      <template #actions>
        <AppButton variant="secondary" @click="$router.back()">
          <i class="fa fa-arrow-left" aria-hidden="true"></i>
          Volver
        </AppButton>
      </template>
    </AppPageHeader>

    <div class="sync-grid">
      <section v-for="card in CARDS" :key="card.kind" class="sync-card">
        <div class="sync-card__head">
          <span class="sync-card__icon" :class="card.icon"></span>
          <div>
            <h3 class="sync-card__title">{{ card.title }}</h3>
            <p class="sync-card__desc">{{ card.description }}</p>
          </div>
          <AppButton variant="outline" @click="downloadTemplate(card.kind)">
            <i class="fa fa-download" aria-hidden="true" style="margin-right:.4rem"></i>
            Plantilla
          </AppButton>
        </div>

        <ul class="sync-legend">
          <li v-for="l in LEGENDS[card.kind]" :key="l.col">
            <strong>{{ l.col }}:</strong> {{ l.desc }}
          </li>
        </ul>

        <div class="sync-actions">
          <label class="sync-file-btn">
            <input
              type="file"
              accept=".xlsx"
              style="display:none"
              @change="(e) => onFileSelected(card.kind, e)"
            />
            <i class="fa fa-file-excel-o" aria-hidden="true" style="margin-right:.45rem"></i>
            {{ states[card.kind].file ? states[card.kind].file!.name : 'Elegir archivo .xlsx' }}
          </label>
          <button
            v-if="states[card.kind].file"
            type="button"
            class="sync-reset"
            title="Quitar archivo"
            @click="resetCard(card.kind)"
          >
            <i class="fa fa-times" aria-hidden="true"></i>
          </button>
          <AppButton
            :disabled="!states[card.kind].preview || states[card.kind].preview!.newCount === 0 || states[card.kind].importing"
            @click="onImport(card.kind)"
          >
            {{ states[card.kind].importing
              ? 'Importando…'
              : states[card.kind].preview
                ? `Importar ${states[card.kind].preview!.newCount} nuevo${states[card.kind].preview!.newCount === 1 ? '' : 's'}`
                : 'Importar' }}
          </AppButton>
        </div>

        <p v-if="states[card.kind].validating" class="sync-note">Comparando con la base de datos…</p>

        <div v-if="states[card.kind].error" class="sync-alert sync-alert--error">
          {{ states[card.kind].error }}
        </div>

        <div v-if="states[card.kind].summary" class="sync-alert sync-alert--ok">
          <strong>Carga completada.</strong> {{ summaryText(states[card.kind].summary!) }}
        </div>

        <div v-if="states[card.kind].preview" class="sync-preview">
          <div class="sync-chips">
            <span class="chip chip--new">{{ states[card.kind].preview!.newCount }} nuevos</span>
            <span class="chip chip--dup">{{ states[card.kind].preview!.duplicateCount }} duplicados (omitidos)</span>
            <span class="chip chip--err">{{ states[card.kind].preview!.errorCount }} con error</span>
          </div>
          <table class="sync-table">
            <thead>
              <tr><th>Fila</th><th>Estado</th><th>Detalle</th></tr>
            </thead>
            <tbody>
              <tr v-for="r in states[card.kind].preview!.rows.slice(0, 50)" :key="r.row">
                <td>{{ r.row }}</td>
                <td>
                  <span class="chip" :class="`chip--${r.status === 'nuevo' ? 'new' : r.status === 'duplicado' ? 'dup' : 'err'}`">
                    {{ r.status.charAt(0).toUpperCase() + r.status.slice(1) }}
                  </span>
                </td>
                <td>{{ r.message }}</td>
              </tr>
            </tbody>
          </table>
          <p v-if="states[card.kind].preview!.rows.length > 50" class="sync-note">
            Mostrando 50 de {{ states[card.kind].preview!.rows.length }} filas.
          </p>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive } from 'vue'
import AppPageHeader from '@/components/AppPageHeader.vue'
import AppButton from '@/components/AppButton.vue'
import {
  downloadTemplate,
  validateFile,
  runImport,
  type SyncKind,
  type SyncPreview,
  type SyncImportSummary,
  type SyncRowResult
} from '@/api/sync'

interface CardState {
  file: File | null
  validating: boolean
  importing: boolean
  preview: SyncPreview | null
  summary: SyncImportSummary | null
  error: string
}

function emptyState(): CardState {
  return { file: null, validating: false, importing: false, preview: null, summary: null, error: '' }
}

const states = reactive<Record<SyncKind, CardState>>({
  usuarios: emptyState(),
  permisos: emptyState(),
  autorizadores: emptyState()
})

function resetCard(kind: SyncKind) {
  states[kind] = emptyState()
}

async function onFileSelected(kind: SyncKind, e: Event) {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  resetCard(kind)
  const st = states[kind]
  if (!file) return
  if (!file.name.toLowerCase().endsWith('.xlsx')) {
    st.error = 'El archivo debe ser .xlsx'
    return
  }
  st.file = file
  st.validating = true
  try {
    st.preview = await validateFile(kind, file)
  } catch (err: any) {
    st.error = err.response?.data?.message || 'No se pudo leer el archivo.'
  } finally {
    st.validating = false
  }
}

async function onImport(kind: SyncKind) {
  const st = states[kind]
  if (!st.file || st.importing) return
  st.importing = true
  try {
    st.summary = await runImport(kind, st.file)
    st.preview = null
    st.file = null
  } catch (err: any) {
    st.error = err.response?.data?.message || 'La importación falló.'
  } finally {
    st.importing = false
  }
}

function summaryText(s: SyncImportSummary): string {
  const parts: string[] = []
  if (s.createdUsers) parts.push(`${s.createdUsers} usuarios creados`)
  if (s.promotedAuthorizers) parts.push(`${s.promotedAuthorizers} promovidos a autorizador`)
  if (s.createdCutoffs) parts.push(`${s.createdCutoffs} periodos cerrados creados`)
  if (s.createdPermissions) parts.push(`${s.createdPermissions} permisos aprobados`)
  parts.push(`${s.duplicates} duplicados omitidos`)
  if (s.errors.length) parts.push(`${s.errors.length} filas con error`)
  return parts.join(' · ') + '.'
}

interface CardDef {
  kind: SyncKind
  title: string
  icon: string
  description: string
}

const CARDS: CardDef[] = [
  {
    kind: 'usuarios',
    title: 'Usuarios',
    icon: 'fa fa-users',
    description:
      'Da de alta personal en bloque. Los correos ya existentes se omiten; los nuevos reciben una clave temporal que RH puede consultar hasta que el usuario ingrese.'
  },
  {
    kind: 'permisos',
    title: 'Permisos históricos',
    icon: 'fa fa-file-text-o',
    description:
      'Excel con dos hojas (Periodos y Permisos). Los periodos faltantes se crean cerrados; los permisos entran Aprobados (sin goce salvo indicación) respetando las reglas por tipo.'
  },
  {
    kind: 'autorizadores',
    title: 'Autorizadores',
    icon: 'fa fa-user-secret',
    description:
      'Promueve usuarios existentes o crea nuevos jefes de área. Su área define qué solicitudes pueden autorizar.'
  }
]

const LEGENDS: Record<SyncKind, { col: string; desc: string }[]> = {
  usuarios: [
    { col: 'Nombre', desc: 'Nombre completo (obligatorio).' },
    { col: 'Correo', desc: 'Institucional y único. Es la llave: si ya existe, no se vuelve a crear.' },
    { col: 'Tipo', desc: 'Trabajador | Autorizador | HumRes | Administrador. Vacío = Trabajador.' },
    { col: 'Área / Departamento / Puesto', desc: 'Nombres exactos de los catálogos.' }
  ],
  permisos: [
    { col: 'Hoja Periodos', desc: 'Fechas dd/mm/aaaa. Los periodos faltantes se crean como CERRADOS.' },
    { col: 'Correo Solicitante', desc: 'Debe existir en Usuarios. Cargue primero esa plantilla si falta.' },
    { col: 'Tipo Permiso / Fechas', desc: 'Tipo exacto del catálogo. Fecha Fin vacía = mismo día.' },
    { col: 'Con Goce', desc: 'Sí/No o 1/0. Vacío = No. Todos entran como Aprobados.' },
    { col: 'Horas Aula / Descarga', desc: 'Solo Falta Docente: suma 1–7h matutino o 6h vespertino.' }
  ],
  autorizadores: [
    { col: 'Nombre / Correo', desc: 'Si el correo existe → se promueve a Autorizador y se actualiza su Área. Si no existe → se crea nuevo.' },
    { col: 'Área', desc: 'Área cuyas solicitudes autorizará (nombre exacto del catálogo).' },
    { col: 'Departamento / Puesto', desc: 'Obligatorios solo si el usuario es nuevo.' }
  ]
}
</script>

<style scoped>
.sync-view > * + * {
  margin-top: 1.4rem;
}

.sync-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1.25rem;
}

@media (min-width: 1100px) {
  .sync-grid { grid-template-columns: repeat(3, minmax(0, 1fr)); align-items: start; }
}

.sync-card {
  display: flex;
  flex-direction: column;
  gap: 0.9rem;
  background: var(--tecnm-surface-white);
  border: 1px solid var(--tecnm-border-color);
  border-radius: var(--tecnm-radius-md, 12px);
  padding: 1.15rem;
  box-shadow: var(--tecnm-shadow-sm, 0 1px 3px rgba(0, 0, 0, 0.08));
}

.sync-card__head {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
}

.sync-card__icon {
  font-size: 1.25rem;
  width: 40px;
  height: 40px;
  flex-shrink: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 10px;
  background: rgba(27, 57, 106, 0.12);
  color: var(--tecnm-blue-primary);
}

.sync-card__title { margin: 0 0 0.15rem; font-size: 1rem; font-weight: 800; }

.sync-card__desc {
  margin: 0;
  font-size: 0.78rem;
  line-height: 1.35;
  color: var(--tecnm-text-secondary);
}

.sync-card__head > :last-child { margin-left: auto; align-self: center; }

.sync-legend {
  list-style: none;
  margin: 0;
  padding: 0.7rem 0.8rem;
  font-size: 0.75rem;
  line-height: 1.5;
  color: var(--tecnm-text-secondary);
  background: var(--tecnm-bg-main);
  border: 1px dashed var(--tecnm-border-color);
  border-radius: 8px;
}
.sync-legend strong { color: var(--tecnm-text-primary); }

.sync-actions {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  margin-top: auto;
}

.sync-file-btn {
  display: inline-flex;
  align-items: center;
  cursor: pointer;
  padding: 0.42rem 0.8rem;
  border-radius: 999px;
  border: 1px solid var(--tecnm-border-color);
  font-size: 0.76rem;
  font-weight: 700;
  max-width: 60%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  transition: background 0.15s ease;
}
.sync-file-btn:hover { background: var(--tecnm-bg-main); }

.sync-reset {
  border: none;
  background: transparent;
  color: var(--tecnm-text-secondary);
  cursor: pointer;
  font-size: 0.9rem;
}

.sync-note { font-size: 0.74rem; color: var(--tecnm-text-secondary); margin: 0; }

.sync-alert {
  border-radius: 8px;
  padding: 0.7rem 0.85rem;
  font-size: 0.78rem;
  line-height: 1.4;
}
.sync-alert--error {
  background: rgba(239, 68, 68, 0.12);
  color: #b91c1c;
  border: 1px solid rgba(239, 68, 68, 0.35);
}
.sync-alert--ok {
  background: rgba(16, 185, 129, 0.12);
  color: #047857;
  border: 1px solid rgba(16, 185, 129, 0.35);
}

.sync-preview { display: flex; flex-direction: column; gap: 0.55rem; }

.sync-chips { display: flex; flex-wrap: wrap; gap: 0.4rem; }

.chip {
  display: inline-flex;
  padding: 0.18rem 0.6rem;
  border-radius: 999px;
  font-size: 0.7rem;
  font-weight: 700;
}
.chip--new { background: rgba(16, 185, 129, 0.14); color: #047857; }
.chip--dup { background: rgba(100, 116, 139, 0.14); color: #475569; }
.chip--err { background: rgba(239, 68, 68, 0.14); color: #b91c1c; }

.sync-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.72rem;
}
.sync-table th, .sync-table td {
  text-align: left;
  padding: 0.32rem 0.45rem;
  border-bottom: 1px solid var(--tecnm-border-color);
  vertical-align: top;
}
.sync-table th { font-weight: 700; color: var(--tecnm-text-secondary); }
</style>
