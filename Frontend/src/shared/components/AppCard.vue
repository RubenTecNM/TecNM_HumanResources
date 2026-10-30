<template>
  <section class="tecnm-card">
    <div
      v-if="title || $slots.header"
      class="tecnm-card-header"
      :class="{ 'tecnm-card-header--premium': premium }"
    >
      <slot name="header">
        <h3 v-if="title">{{ title }}</h3>
      </slot>
    </div>
    <div class="tecnm-card-body">
      <slot />
    </div>
    <div v-if="$slots.footer" class="tecnm-card-footer">
      <slot name="footer" />
    </div>
  </section>
</template>

<script setup lang="ts">
withDefaults(defineProps<{ title?: string; premium?: boolean }>(), {
  premium: false
})
</script>
