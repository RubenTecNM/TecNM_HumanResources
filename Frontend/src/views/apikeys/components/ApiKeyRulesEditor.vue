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
import type { ApiPermissionItem, ApiTableItem } from '@/api/apikeys'

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
  max-height: 320px;
  overflow-y: auto;
}
</style>
