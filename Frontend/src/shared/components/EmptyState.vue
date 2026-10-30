<template>
  <div class="empty-state">
    <i class="fa fa-inbox" aria-hidden="true"></i>
    <p>{{ message }}</p>
    <slot />
  </div>
</template>

<script setup lang="ts">
withDefaults(defineProps<{ message?: string }>(), {
  message: 'No hay datos disponibles'
})
</script>

<style scoped>
.empty-state {
  text-align: center;
  padding: 3rem 1rem;
  color: var(--tecnm-text-secondary);
}

.empty-state i {
  font-size: 2.5rem;
  margin-bottom: 0.75rem;
  opacity: 0.5;
}
</style>