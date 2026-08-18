<template>
  <div class="tecnm-form-group">
    <label v-if="label" class="tecnm-label" :for="id">{{ label }}</label>
    <slot :id="id" />
    <span v-if="error" class="tecnm-form-error">{{ error }}</span>
  </div>
</template>

<script setup lang="ts">
import { useId } from 'vue'

withDefaults(defineProps<{ label?: string; error?: string }>(), {
  error: ''
})

const id = useId()
</script>