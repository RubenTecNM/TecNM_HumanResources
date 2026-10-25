<template>
  <div class="tecnm-page-hero">
    <div class="tecnm-page-hero__content">
      <h1 class="tecnm-page-hero__title"><i :class="`fa ${icon}`" aria-hidden="true"></i> {{ title }}</h1>
      <p v-if="subtitle" class="tecnm-page-hero__subtitle">{{ subtitle }}</p>
    </div>
    <div v-if="$slots.actions" class="tecnm-page-hero__actions">
      <slot name="actions" />
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{ title: string; subtitle?: string; icon?: string }>()
</script>
