<template>
  <div class="rules-editor">
    <div v-if="tables.length === 0" class="small text-muted py-2">
      No hay tablas disponibles para asignar reglas.
    </div>
    <div v-else class="tecnm-table-responsive">
      <table class="tecnm-table tecnm-table-sm">
        <thead>
          <tr>
            <th>Tabla</th>
            <th v-for="p in permissions" :key="p.id" class="text-center">
              {{ p.description }}
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="t in tables" :key="t.id">
            <td class="fw-semibold">{{ t.description }}</td>
            <td v-for="p in permissions" :key="p.id" class="text-center">
              <input
                type="checkbox"
                class="form-check-input"
                :checked="isChecked(t.id, p.id)"
                @change="toggle(t.id, p.id)"
              />
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { ApiPermissionItem, ApiTableItem } from "@/features/apikeys/api/apikeys"'

const props = defineProps<{
  tables: ApiTableItem[]
  permissions: ApiPermissionItem[]
  modelValue: Record<number, number[]>
}>()

const emit = defineEmits<{ (e: 'update:modelValue', value: Record<number, number[]>): void }>()

function isChecked(tableId: number, permissionId: number): boolean {
  return (props.modelValue[tableId] || []).includes(permissionId)
}

function toggle(tableId: number, permissionId: number) {
  const current = props.modelValue[tableId] || []
  const next = current.includes(permissionId)
    ? current.filter(p => p !== permissionId)
    : [...current, permissionId]
  emit('update:modelValue', { ...props.modelValue, [tableId]: next })
}
</script>

<style scoped>
.rules-editor {
  max-height: 340px;
  overflow-y: auto;
  border-radius: var(--tecnm-radius-sm);
  border: 1px solid var(--tecnm-border-color);
  scrollbar-width: thin;
  scrollbar-color: var(--tecnm-border-color) transparent;
}

/* Make table cells clearly readable inside the modal */
.rules-editor :deep(.tecnm-table) th,
.rules-editor :deep(.tecnm-table) td {
  color: var(--tecnm-text-primary);
  background: var(--tecnm-surface-white);
}

.rules-editor :deep(.tecnm-table) thead th {
  background: var(--tecnm-table-head-bg);
  color: var(--tecnm-table-head-text);
  font-weight: 600;
  font-size: 0.8rem;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  padding: 0.5rem 0.6rem;
  position: sticky;
  top: 0;
  z-index: 1;
}

.rules-editor :deep(.tecnm-table) tbody tr:nth-child(even) td {
  background: var(--tecnm-soft-hover);
}

.rules-editor :deep(.tecnm-table) td {
  padding: 0.4rem 0.6rem;
  vertical-align: middle;
  font-size: 0.85rem;
  border-bottom: 1px solid var(--tecnm-border-color);
}

/* Checkbox styling — make it visible in dark mode */
.rules-editor :deep(input[type='checkbox']) {
  accent-color: var(--tecnm-blue-primary);
  width: 1rem;
  height: 1rem;
  cursor: pointer;
}
</style>
