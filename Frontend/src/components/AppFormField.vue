<template>
  <div class="tecnm-form-group">
    <label v-if="label" class="tecnm-label" :for="id">{{ label }}</label>
    <slot :id="id" :aria-describedby="errorId" />
    <span v-if="error" :id="errorId" class="tecnm-form-error" role="alert">{{ error }}</span>
  </div>
</template>

<script setup lang="ts">
import { computed, useId } from 'vue'

const props = withDefaults(defineProps<{ label?: string; error?: string }>(), {
  error: ''
})

const id = useId()
const errorId = computed(() => props.error ? `${id}-error` : undefined)
</script>
