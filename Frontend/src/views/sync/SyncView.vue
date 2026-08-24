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

    <div v-if="!isAdmin && !isRH" class="sync-denied">
      No tiene permisos para usar este módulo.
    </div>

    <div v-else class="sync-grid">
      <SyncCard
        v-for="card in CARDS"
        :key="card.kind"
        v-bind="card.props as any"
        :state="states[card.kind]"
        @download="downloadTemplate(card.kind)"
        @file-selected="(file: File) => onFileSelected(card.kind, file)"
        @import="onImport(card.kind)"
        @reset="resetCard(card.kind)"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, defineComponent, h, reactive } from 'vue'
import AppPageHeader from '@/components/AppPageHeader.vue'
import AppButton from '@/components/AppButton.vue'
import {
  downloadTemplate,
  validateFile,
  runImport,
  type SyncKind,
  type SyncPreview,
  type SyncImportSummary
} from '@/api/sync'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const isAdmin = computed(() => auth.user?.userType === 1)
const isRH = computed(() => auth.user?.userType === 3)

interface CardState {
  file: File | null
  validating: boolean
  importing: boolean
  preview: SyncPreview | null
  summary: SyncImportSummary | null
  error: string
}

const states = reactive<Record<SyncKind, CardState>>({
  usuarios: emptyState(),
  permisos: emptyState(),
  autorizadores: emptyState()
})

function emptyState(): CardState {
  return { file: null, validating: false, importing: false, preview: null, summary: null, error: '' }
}

function resetCard(kind: SyncKind) {
  states[kind] = emptyState()
}

async function onFileSelected(kind: SyncKind, file: File) {
  resetCard(kind)
  const st = states[kind]
  if (!file.name.toLowerCase().endsWith('.xlsx')) {
    st.error = 'El archivo debe ser .xlsx'
    return
  }
  st.file = file
  st.validating = true
  try {
    st.preview = await validateFile(kind, file)
  } catch (e: any) {
    st.error = e.response?.data?.message || 'No se pudo leer el archivo.'
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
  } catch (e: any) {
    st.error = e.response?.data?.message || 'La importación falló.'
  } finally {
    st.importing = false
  }
}

// ── Tarjeta por tipo de carga (componente local; evita un archivo más) ──
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

const SyncCard = defineComponent({
  name: 'SyncCard',
  props: {
    kind: { type: String as () => SyncKind, required: true },
    title: { type: String, required: true },
    icon: { type: String, required: true },
    description: { type: String, required: true },
    state: { type: Object, required: true }
  },
  emits: ['download', 'file-selected', 'import', 'reset'],
  setup(props, { emit }) {
    return () => {
      const st = props.state as CardState
      const legend = LEGENDS[props.kind]
      return h('section', { class: 'sync-card' }, [
        h('div', { class: 'sync-card__head' }, [
          h('span', { class: `sync-card__icon ${props.icon}` }),
          h('div', [
            h('h3', { class: 'sync-card__title' }, props.title),
            h('p', { class: 'sync-card__desc' }, props.description)
          ]),
          h(AppButton, { variant: 'outline', onClick: () => emit('download') }, () => [
            h('i', { class: 'fa fa-download', style: 'margin-right:.4rem' }), 'Plantilla'
          ])
        ]),
        h('ul', { class: 'sync-legend' }, legend.map(l =>
          h('li', { key: l.col }, [h('strong', l.col + ': '), l.desc])
        )),
        h('div', { class: 'sync-actions' }, [
          h('label', { class: 'sync-file-btn' }, [
            h('input', {
              type: 'file',
              accept: '.xlsx',
              style: 'display:none',
              onChange: (e: Event) => {
                const f = (e.target as HTMLInputElement).files?.[0]
                if (f) emit('file-selected', f)
              }
            }),
            h('i', { class: 'fa fa-file-excel-o', style: 'margin-right:.45rem' }),
            st.file ? st.file.name : 'Elegir archivo .xlsx'
          ]),
          st.file ? h('button', {
            class: 'sync-reset',
            onClick: () => emit('reset'),
            title: 'Quitar archivo'
          }, h('i', { class: 'fa fa-times' })) : null,
          h(AppButton, {
            variant: 'primary',
            disabled: !st.preview || st.preview.newCount === 0 || st.importing,
            onClick: () => emit('import')
          }, () => st.importing ? 'Importando…'
            : st.preview ? `Importar ${st.preview.newCount} nuevo${st.preview.newCount === 1 ? '' : 's'}`
            : 'Importar')
        ]),
        st.validating ? h('p', { class: 'sync-note' }, 'Comparando con la base de datos…') : null,
        st.error ? h('div', { class: 'sync-alert sync-alert--error' }, st.error) : null,
        st.summary ? h(SummaryPanel, { summary: st.summary }) : null,
        st.preview ? h(PreviewPanel, { preview: st.preview }) : null
      ])
    }
  }
})

const PreviewPanel = defineComponent({
  props: { preview: { type: Object as () => SyncPreview, required: true } },
  setup(props) {
    return () => {
      const p = props.preview
      const visible = p.rows.slice(0, 50)
      return h('div', { class: 'sync-preview' }, [
        h('div', { class: 'sync-chips' }, [
          h('span', { class: 'chip chip--new' }, `${p.newCount} nuevos`),
          h('span', { class: 'chip chip--dup' }, `${p.duplicateCount} duplicados (omitidos)`),
          h('span', { class: 'chip chip--err' }, `${p.errorCount} con error`)
        ]),
        h('table', { class: 'sync-table' }, [
          h('thead', null, h('tr', null, [
            h('th', 'Fila'), h('th', 'Estado'), h('th', 'Detalle')
          ])),
          h('tbody', null, visible.map(r => h('tr', { key: r.row }, [
            h('td', String(r.row)),
            h('td', h('span', { class: `chip chip--${r.status === 'nuevo' ? 'new' : r.status === 'duplicado' ? 'dup' : 'err'}` },
              r.status.charAt(0).toUpperCase() + r.status.slice(1))),
            h('td', r.message)
          ])))
        ]),
        p.rows.length > 50 ? h('p', { class: 'sync-note' }, `Mostrando 50 de ${p.rows.length} filas.`) : null
      ])
    }
  }
})

const SummaryPanel = defineComponent({
  props: { summary: { type: Object as () => SyncImportSummary, required: true } },
  setup(props) {
    return () => {
      const s = props.summary
      const parts: string[] = []
      if (s.createdUsers) parts.push(`${s.createdUsers} usuarios creados`)
      if (s.promotedAuthorizers) parts.push(`${s.promotedAuthorizers} promovidos a autorizador`)
      if (s.createdCutoffs) parts.push(`${s.createdCutoffs} periodos cerrados creados`)
      if (s.createdPermissions) parts.push(`${s.createdPermissions} permisos aprobados`)
      parts.push(`${s.duplicates} duplicados omitidos`)
      return h('div', { class: 'sync-alert sync-alert--ok' }, [
        h('strong', 'Carga completada. '), parts.join(' · ') + '.',
        s.errors.length ? h('p', { class: 'sync-note' }, `${s.errors.length} filas no pudieron cargarse.`) : null
      ])
    }
  }
})

const CARDS: { kind: SyncKind; props: Record<string, string> }[] = [
  {
    kind: 'usuarios',
    props: {
      title: 'Usuarios',
      icon: 'fa fa-users',
      description: 'Da de alta personal en bloque. Los correos ya existentes se omiten; los nuevos reciben una clave temporal que RH puede consultar hasta que el usuario ingrese.'
    }
  },
  {
    kind: 'permisos',
    props: {
      title: 'Permisos históricos',
      icon: 'fa fa-file-text-o',
      description: 'Excel con dos hojas (Periodos y Permisos). Los periodos faltantes se crean cerrados; los permisos entran Aprobados (sin goce salvo indicación) respetando las reglas por tipo.'
    }
  },
  {
    kind: 'autorizadores',
    props: {
      title: 'Autorizadores',
      icon: 'fa fa-user-secret',
      description: 'Promueve usuarios existentes o crea nuevos jefes de área. Su área define qué solicitudes pueden autorizar.'
    }
  }
]
</script>

<style scoped>
.sync-view > * + * { margin-top: 1.4rem; }

.sync-denied {
  padding: 2rem;
  text-align: center;
  color: var(--tecnm-text-secondary);
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
.sync-card__desc { margin: 0; font-size: 0.78rem; line-height: 1.35; color: var(--tecnm-text-secondary); }

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
