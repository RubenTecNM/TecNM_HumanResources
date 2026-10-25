<template>
  <div class="nf-page">
    <section class="tecnm-card nf-card">
      <div class="tecnm-card-body text-center">
        <i class="fa fa-exclamation-triangle nf-icon" aria-hidden="true"></i>
        <h1>404</h1>
        <p>La página que buscas no existe.</p>
        <AppButton variant="primary" icon="fa-home" @click="$router.push('/permissions')">Ir al inicio</AppButton>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import AppButton from '@/components/AppButton.vue'
</script>

<style scoped>
.nf-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
}

.nf-card {
  max-width: 480px;
}

.nf-icon {
  font-size: 3rem;
  color: var(--tecnm-gold-accent);
  margin-bottom: 1rem;
}

.nf-card h1 {
  font-size: 3rem;
  margin: 0 0 0.5rem;
}

.text-center {
  text-align: center;
}
</style>