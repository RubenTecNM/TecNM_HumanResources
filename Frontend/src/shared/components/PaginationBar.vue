<template>
  <div v-if="totalPages > 1" class="tecnm-pagination-v2" aria-label="Paginación">
    <span class="tecnm-pagination-info">Página {{ page }} de {{ totalPages }}</span>
    <div class="tecnm-pagination-buttons">
      <button
        class="tecnm-page-btn"
        :disabled="page <= 1"
        aria-label="Página anterior"
        @click="$emit('update:page', page - 1)"
      >
        <i class="fa fa-chevron-left" aria-hidden="true"></i>
      </button>
      <button
        v-for="n in pageNumbers"
        :key="n"
        class="tecnm-page-btn"
        :class="{ 'is-current': n === page }"
        :disabled="n === page"
        @click="$emit('update:page', n)"
      >
        {{ n }}
      </button>
      <button
        class="tecnm-page-btn"
        :disabled="page >= totalPages"
        aria-label="Página siguiente"
        @click="$emit('update:page', page + 1)"
      >
        <i class="fa fa-chevron-right" aria-hidden="true"></i>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{ page: number; totalPages: number }>(), {
  totalPages: 1
})
defineEmits<{ 'update:page': [value: number] }>()

const pageNumbers = computed(() => {
  const total = props.totalPages
  const current = props.page
  const window = 2
  const start = Math.max(1, current - window)
  const end = Math.min(total, current + window)
  const pages: number[] = []
  for (let i = start; i <= end; i++) pages.push(i)
  if (start > 1) pages.unshift(1)
  if (end < total) pages.push(total)
  return pages
})
</script>
