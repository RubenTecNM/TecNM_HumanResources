<template>
  <button
    type="button"
    class="tecnm-btn"
    :class="[`tecnm-btn-${variant}`, size === 'sm' ? 'tecnm-btn-sm' : '']"
    :disabled="disabled || loading"
    v-bind="$attrs"
  >
    <i v-if="loading" class="fa fa-spinner fa-spin" aria-hidden="true"></i>
    <i v-else-if="icon" :class="`fa ${icon}`" aria-hidden="true"></i>
    <slot />
  </button>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    variant?: 'primary' | 'accent' | 'secondary' | 'outline' | 'danger'
    size?: 'sm' | 'md'
    icon?: string
    loading?: boolean
    disabled?: boolean
  }>(),
  { variant: 'primary', size: 'md' }
)
</script>