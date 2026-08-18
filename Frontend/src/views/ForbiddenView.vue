<template>
  <div class="fb-page">
    <section class="tecnm-card fb-card">
      <div class="tecnm-card-body text-center">
        <i class="fa fa-shield fb-icon" aria-hidden="true"></i>
        <h1>403</h1>
        <p>No tienes permisos para acceder a esta sección.</p>
        <AppButton variant="primary" icon="fa-home" @click="$router.push('/permissions')">Ir al inicio</AppButton>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import AppButton from '@/components/AppButton.vue'
</script>

<style scoped>
.fb-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
}

.fb-card {
  max-width: 480px;
}

.fb-icon {
  font-size: 3rem;
  color: var(--tecnm-danger, #ef4444);
  margin-bottom: 1rem;
}

.fb-card h1 {
  font-size: 3rem;
  margin: 0 0 0.5rem;
}

.text-center {
  text-align: center;
}
</style>
