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

    <div class="sync-intro">
      <i class="fa fa-info-circle" aria-hidden="true"></i>
      Orden recomendado: <strong>1) Usuarios</strong> → <strong>2) Autorizadores</strong> → <strong>3) Permisos</strong>.
      Cada card descarga su plantilla, valida el archivo contra la base de datos y te muestra un preview
      antes de importar — nada se modifica hasta que pulses <em>Importar</em>.
    </div>

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

        <ol class="sync-steps">
          <li v-for="(s, i) in card.steps" :key="i">{{ s }}</li>
        </ol>

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
import { confirmImportWithWarnings } from '../composables/useSyncWarnings'
import { showToast } from '@/shared/composables/useAlerts'
import AppPageHeader from '@/shared/components/AppPageHeader.vue'
import AppButton from '@/shared/components/AppButton.vue'
import {
  downloadTemplate,
  validateFile,
  runImport,
  type SyncKind,
  type SyncPreview,
  type SyncImportSummary,
  type SyncRowResult
} from "@/features/sync/api/sync"

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
  if (st.preview) {
    const proceed = await confirmImportWithWarnings(kind, st.preview)
    if (!proceed) return
  }
  st.importing = true
  try {
    st.summary = await runImport(kind, st.file)
    st.preview = null
    st.file = null
    showToast("Importación completada exitosamente", "success")
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
  steps: string[]
}

const CARDS: CardDef[] = [
  {
    kind: 'usuarios',
    title: 'Usuarios',
    icon: 'fa fa-users',
    description:
      'Da de alta personal en bloque. La llave es el correo institucional: los ya existentes se omiten sin modificarse; los nuevos se crean activos y con una clave temporal que RH debe entregarles.',
    steps: [
      'Descarga la plantilla con el botón «Plantilla» — trae las hojas de catálogo (Tipos, Áreas, Departamentos, Puestos).',
      'Llena la hoja Usuarios eligiendo los Id de los desplegables (coinciden con las hojas de catálogo).',
      'Regresa aquí, elige el archivo y revisa el resultado: verás qué filas se crearán, cuáles se omiten por existir y cuáles tienen errores antes de importar nada.',
      'Pulsa Importar. Entrega a cada usuario su clave temporal.'
    ]
  },
  {
    kind: 'autorizadores',
    title: 'Autorizadores',
    icon: 'fa fa-user-secret',
    description:
      'Define quién autoriza solicitudes. Si el correo ya existe en el sistema se promueve a Autorizador y se actualiza su área; si no existe, se crea nuevo. El área que registres define qué solicitudes podrá autorizar.',
    steps: [
      'Descarga la plantilla y llena la hoja Autorizadores (Id de Área, Departamento y Puesto desde los desplegables).',
      'Elige el archivo y revisa el preview: distingue entre «nuevo» y «promovido» (usuario existente que pasa a autorizar).',
      'Pulsa Importar. Los cambios aplican de inmediato.'
    ]
  },
  {
    kind: 'permisos',
    title: 'Permisos históricos',
    icon: 'fa fa-file-text-o',
    description:
      'Importa permisos ya autorizados desde papel o sistemas previos. Requiere que los solicitantes existan como usuarios: cárgalos primero en la card Usuarios. Todo lo importado entra como Aprobado.',
    steps: [
      'Asegúrate de haber cargado primero a los Usuarios (la hoja Solicitantes de esta plantilla solo lista los existentes).',
      'Descarga la plantilla: trae Periodos (con Id), Solicitantes y Tipos de Permiso de la base de datos real.',
      'En la hoja Permisos registra fecha y hora de inicio/fin por separado; Fecha Fin vacía = mismo día.',
      'Revisa el preview — cada fila con error indica exactamente qué celda falló y qué valor se leyó — y pulsa Importar.'
    ]
  }
]

const LEGENDS: Record<SyncKind, { col: string; desc: string }[]> = {
  usuarios: [
    { col: 'Nombre', desc: 'Nombre completo (obligatorio).' },
    { col: 'Correo', desc: 'Institucional (@monclova.tecnm.mx) y único. Es la llave: si ya existe, no se vuelve a crear.' },
    { col: 'TipoId', desc: 'Id del desplegable: 1 Admin · 2 Autorizador · 3 HumRes · 4 Trabajador.' },
    { col: 'ÁreaId / DepartamentoId / PuestoId', desc: 'Ids numéricos del desplegable; cada uno coincide con su hoja de catálogo.' },
    { col: 'Resultado', desc: 'Nuevos → creados con clave temporal. Existentes → omitidos (no se actualizan).' }
  ],
  permisos: [
    { col: 'Hoja Periodos', desc: 'Viene prellenada con Id 1–300; referencia esos Id en la columna Periodo (Id).' },
    { col: 'Correo Solicitante', desc: 'Desplegable con usuarios existentes. Si falta alguien, cárgalo antes en Usuarios.' },
    { col: 'TipoPermisoId', desc: 'Id del desplegable (hoja Tipos de Permiso). Falta Docente exige Horas Aula/Descarga.' },
    { col: 'Fechas y Horas', desc: 'Inicio y fin separados (dd/mm/aaaa + HH:mm). Hora Fin obligatoria; Fecha Fin vacía = misma fecha.' },
    { col: 'Con Goce', desc: 'Sí/No. Vacío = No. Todos los permisos importados quedan Aprobados.' }
  ],
  autorizadores: [
    { col: 'Nombre / Correo', desc: 'Correo existente → se promueve a Autorizador y se actualiza su Área. Correo nuevo → se crea usuario.' },
    { col: 'ÁreaId', desc: 'Área cuyas solicitudes autorizará (desplegable con Id).' },
    { col: 'DepartamentoId / PuestoId', desc: 'Obligatorios solo si el usuario es nuevo.' }
  ]
}
</script>

<style scoped>
.sync-view > * + * {
  margin-top: 1.4rem;
}

.sync-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  gap: 1.25rem;
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

.sync-intro {
  display: flex;
  align-items: baseline;
  gap: 0.5rem;
  padding: 0.75rem 1rem;
  border: 1px solid var(--tecnm-border, #d8dee9);
  border-left: 4px solid var(--tecnm-primary, #1b396a);
  border-radius: 8px;
  background: var(--tecnm-surface-muted, #f5f7fa);
  font-size: 0.92rem;
}

.sync-steps {
  margin: 0;
  padding-left: 1.3rem;
  font-size: 0.88rem;
  color: var(--tecnm-text-secondary, #55606e);
}
.sync-steps li { margin-top: 0.25rem; }
.sync-steps li::marker { color: var(--tecnm-primary, #1b396a); font-weight: 600; }

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
