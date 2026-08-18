<template>
  <div class="autocomplete" ref="rootRef">
    <div class="autocomplete-control">
      <i class="fa fa-search autocomplete-icon" aria-hidden="true"></i>
      <input
        ref="inputRef"
        type="text"
        class="tecnm-form-control autocomplete-input"
        :placeholder="placeholder"
        v-model="query"
        :disabled="disabled"
        autocomplete="off"
        role="combobox"
        :aria-expanded="open ? 'true' : 'false'"
        @focus="open = true"
        @input="onInput"
        @keydown="onKeydown"
        @blur="onBlur"
      />
      <span v-if="loading" class="autocomplete-spinner" aria-hidden="true"></span>
      <button v-else-if="modelValue" type="button" class="autocomplete-clear" title="Limpiar" @mousedown.prevent="clearSelection">
        <i class="fa fa-times" aria-hidden="true"></i>
      </button>
    </div>

    <ul v-if="open" class="autocomplete-list" role="listbox">
      <li
        v-for="(item, index) in displayItems"
        :key="item.id"
        class="autocomplete-item"
        :class="{ active: index === highlightedIndex }"
        role="option"
        :aria-selected="index === highlightedIndex"
        @mousedown.prevent="select(item)"
        @mouseenter="highlightedIndex = index"
      >
        <span class="autocomplete-item-label">{{ item.label }}</span>
        <span v-if="item.sublabel" class="autocomplete-item-sublabel">{{ item.sublabel }}</span>
      </li>
      <li v-if="displayItems.length === 0" class="autocomplete-empty">
        {{ loading ? 'Buscando...' : 'Sin resultados' }}
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, ref, watch } from 'vue'

export interface AutocompleteItem {
  id: number
  label: string
  sublabel?: string
}

// search?: función de búsqueda server-side. Si se provee, se usa con debounce;
// si no, el componente filtra localmente sobre `items`.
type SearchFn = (term: string) => Promise<AutocompleteItem[]>

const props = withDefaults(
  defineProps<{
    modelValue?: number | null
    items?: AutocompleteItem[]
    search?: SearchFn
    minChars?: number
    debounceMs?: number
    placeholder?: string
    disabled?: boolean
  }>(),
  { items: () => [], minChars: 2, debounceMs: 300, placeholder: 'Buscar...', disabled: false }
)

const emit = defineEmits<{ (e: 'update:modelValue', value: number | null): void }>()

const rootRef = ref<HTMLElement | null>(null)
const inputRef = ref<HTMLInputElement | null>(null)
const query = ref('')
const open = ref(false)
const loading = ref(false)
const highlightedIndex = ref(0)
const remoteItems = ref<AutocompleteItem[]>([])
let debounceTimer: ReturnType<typeof setTimeout> | undefined

const isRemote = computed(() => typeof props.search === 'function')

const filteredItems = computed<AutocompleteItem[]>(() => {
  const q = query.value.trim().toLowerCase()
  if (!q) return props.items
  return props.items.filter(
    (i) => i.label.toLowerCase().includes(q) || (i.sublabel ?? '').toLowerCase().includes(q)
  )
})

const displayItems = computed<AutocompleteItem[]>(() => (isRemote.value ? remoteItems.value : filteredItems.value))

watch(
  () => props.modelValue,
  (val) => {
    if (val == null) {
      query.value = ''
    } else {
      const item = [...props.items, ...remoteItems.value].find((i) => i.id === val)
      query.value = item ? item.label : ''
    }
  },
  { immediate: true }
)

function onInput() {
  open.value = true
  highlightedIndex.value = 0
  if (isRemote.value) {
    scheduleSearch()
  }
}

function scheduleSearch() {
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    void runSearch(query.value)
  }, props.debounceMs)
}

async function runSearch(term: string) {
  if (!term.trim() || term.trim().length < props.minChars) {
    remoteItems.value = []
    loading.value = false
    return
  }
  loading.value = true
  try {
    const results = await props.search!(term.trim())
    remoteItems.value = results
  } catch (err) {
    console.error('Error en la búsqueda:', err)
    remoteItems.value = []
  } finally {
    loading.value = false
  }
}

function onKeydown(e: KeyboardEvent) {
  if (!open.value && (e.key === 'ArrowDown' || e.key === 'Enter')) {
    if (displayItems.value.length > 0) open.value = true
    return
  }
  if (e.key === 'ArrowDown') {
    e.preventDefault()
    highlightedIndex.value = Math.min(highlightedIndex.value + 1, displayItems.value.length - 1)
  } else if (e.key === 'ArrowUp') {
    e.preventDefault()
    highlightedIndex.value = Math.max(highlightedIndex.value - 1, 0)
  } else if (e.key === 'Enter') {
    e.preventDefault()
    const item = displayItems.value[highlightedIndex.value]
    if (item) select(item)
  } else if (e.key === 'Escape') {
    open.value = false
  } else if (e.key === 'Tab') {
    open.value = false
  }
}

function onBlur() {
  // Retraso para permitir el click en la lista
  setTimeout(() => {
    open.value = false
    const item = [...props.items, ...remoteItems.value].find((i) => i.id === props.modelValue)
    query.value = item ? item.label : ''
  }, 150)
}

function select(item: AutocompleteItem) {
  emit('update:modelValue', item.id)
  query.value = item.label
  open.value = false
}

function clearSelection() {
  emit('update:modelValue', null)
  query.value = ''
  remoteItems.value = []
  open.value = false
  void nextTick(() => inputRef.value?.focus())
}

onBeforeUnmount(() => {
  if (debounceTimer) clearTimeout(debounceTimer)
})
</script>

<style scoped>
.autocomplete {
  position: relative;
}

.autocomplete-control {
  position: relative;
}

.autocomplete-icon {
  position: absolute;
  left: 0.85rem;
  top: 50%;
  transform: translateY(-50%);
  color: var(--tecnm-text-secondary);
  font-size: 0.85rem;
  pointer-events: none;
}

.autocomplete-input {
  padding-left: 2.4rem;
  padding-right: 2.2rem;
}

.autocomplete-clear {
  position: absolute;
  right: 0.5rem;
  top: 50%;
  transform: translateY(-50%);
  width: 26px;
  height: 26px;
  border: none;
  border-radius: 50%;
  background: transparent;
  color: var(--tecnm-text-secondary);
  cursor: pointer;
}

.autocomplete-clear:hover {
  color: var(--tecnm-danger);
}

.autocomplete-spinner {
  position: absolute;
  right: 0.7rem;
  top: 50%;
  transform: translateY(-50%);
  width: 16px;
  height: 16px;
  border: 2px solid var(--tecnm-border-color);
  border-top-color: var(--tecnm-accent-text);
  border-radius: 50%;
  animation: autocomplete-spin 0.7s linear infinite;
}

@keyframes autocomplete-spin {
  to {
    transform: translateY(-50%) rotate(360deg);
  }
}

.autocomplete-list {
  position: absolute;
  z-index: 50;
  top: calc(100% + 0.25rem);
  left: 0;
  right: 0;
  max-height: 260px;
  overflow-y: auto;
  margin: 0;
  padding: 0.35rem;
  list-style: none;
  background: var(--tecnm-surface-white);
  border: 1px solid var(--tecnm-border-color);
  border-radius: var(--tecnm-radius-md, 0.625rem);
  box-shadow: var(--tecnm-shadow-lg);
}

.autocomplete-item {
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
  padding: 0.5rem 0.75rem;
  border-radius: var(--tecnm-radius-sm, 0.5rem);
  cursor: pointer;
}

.autocomplete-item.active,
.autocomplete-item:hover {
  background: var(--tecnm-soft-hover);
}

.autocomplete-item-label {
  font-size: 0.88rem;
  font-weight: 600;
  color: var(--tecnm-text-primary);
}

.autocomplete-item-sublabel {
  font-size: 0.75rem;
  color: var(--tecnm-text-secondary);
}

.autocomplete-empty {
  padding: 0.6rem 0.75rem;
  font-size: 0.85rem;
  color: var(--tecnm-text-secondary);
  text-align: center;
}
</style>