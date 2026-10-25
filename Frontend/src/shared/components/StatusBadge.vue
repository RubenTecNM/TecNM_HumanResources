<template>
  <span class="tecnm-badge tecnm-badge--dot" :class="`tecnm-badge-${variant ?? 'info'}`">
    <span class="tecnm-badge-dot"></span>
    <slot />
  </span>
</template>

<script setup lang="ts">
withDefaults(defineProps<{ variant?: 'approved' | 'pending' | 'rejected' | 'info' }>(), {
  variant: 'info'
})
</script>
