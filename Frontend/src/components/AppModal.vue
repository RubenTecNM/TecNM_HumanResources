<template>
  <Teleport to="body">
    <Transition name="modal-fade">
      <div v-if="show" class="tecnm-modal-backdrop" @click.self="handleBackdropClick">
        <div
          ref="containerEl"
          class="tecnm-modal-container"
          :class="[`tecnm-modal--${size}`]"
          role="dialog"
          aria-modal="true"
          :aria-labelledby="titleId"
        >
          <!-- Header -->
          <div class="tecnm-modal-header">
            <h3 :id="titleId" class="tecnm-modal-title">
              <i v-if="icon" :class="`fa ${icon}`" aria-hidden="true"></i>
              {{ title }}
            </h3>
            <button
              ref="closeBtnEl"
              type="button"
              class="tecnm-modal-close"
              aria-label="Cerrar modal"
              @click="close"
            >
              <i class="fa fa-times" aria-hidden="true"></i>
            </button>
          </div>

          <!-- Body -->
          <div class="tecnm-modal-body">
            <slot />
          </div>

          <!-- Footer -->
          <div v-if="$slots.footer || showDefaultFooter" class="tecnm-modal-footer">
            <slot name="footer">
              <AppButton
                v-if="showCancel"
                variant="secondary"
                :disabled="loading"
                @click="close"
              >
                {{ cancelText }}
              </AppButton>

              <AppButton
                v-if="showConfirm"
                :variant="confirmVariant"
                :loading="loading"
                @click="$emit('confirm')"
              >
                {{ confirmText }}
              </AppButton>
            </slot>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, watch, onUnmounted, useId, nextTick } from 'vue'
import AppButton from '@/components/AppButton.vue'

const props = withDefaults(
  defineProps<{
    show: boolean
    title: string
    icon?: string
    size?: 'sm' | 'md' | 'lg' | 'xl'
    loading?: boolean
    showDefaultFooter?: boolean
    showCancel?: boolean
    showConfirm?: boolean
    confirmText?: string
    cancelText?: string
    confirmVariant?: 'primary' | 'accent' | 'secondary' | 'outline' | 'danger'
    closeOnBackdrop?: boolean
  }>(),
  {
    size: 'md',
    loading: false,
    showDefaultFooter: true,
    showCancel: true,
    showConfirm: true,
    confirmText: 'Guardar',
    cancelText: 'Cancelar',
    confirmVariant: 'primary',
    closeOnBackdrop: true
  }
)

const emit = defineEmits<{
  (e: 'update:show', value: boolean): void
  (e: 'close'): void
  (e: 'confirm'): void
}>()

const titleId = useId()
const containerEl = ref<HTMLElement | null>(null)
const closeBtnEl = ref<HTMLButtonElement | null>(null)
let previousFocus: HTMLElement | null = null

function getFocusableElements(): HTMLElement[] {
  if (!containerEl.value) return []
  return Array.from(
    containerEl.value.querySelectorAll<HTMLElement>(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    )
  ).filter((el) => !(el as HTMLInputElement).disabled && el.offsetParent !== null)
}

function handleTabTrap(e: KeyboardEvent) {
  if (e.key !== 'Tab' || !containerEl.value) return
  const focusable = getFocusableElements()
  if (focusable.length === 0) return
  const first = focusable[0]
  const last = focusable[focusable.length - 1]
  if (e.shiftKey) {
    if (document.activeElement === first) {
      e.preventDefault()
      last.focus()
    }
  } else {
    if (document.activeElement === last) {
      e.preventDefault()
      first.focus()
    }
  }
}

function handleKeyDown(e: KeyboardEvent) {
  if (e.key === 'Escape' && props.show) {
    close()
  }
  handleTabTrap(e)
}

function close() {
  if (props.loading) return
  emit('update:show', false)
  emit('close')
}

function handleBackdropClick() {
  if (props.closeOnBackdrop) {
    close()
  }
}

watch(
  () => props.show,
  async (val) => {
    if (val) {
      previousFocus = document.activeElement as HTMLElement
      document.body.style.overflow = 'hidden'
      window.addEventListener('keydown', handleKeyDown)
      await nextTick()
      closeBtnEl.value?.focus()
    } else {
      document.body.style.overflow = ''
      window.removeEventListener('keydown', handleKeyDown)
      previousFocus?.focus()
      previousFocus = null
    }
  },
  { immediate: true }
)

onUnmounted(() => {
  document.body.style.overflow = ''
  window.removeEventListener('keydown', handleKeyDown)
})
</script>

<style scoped>
.tecnm-modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background-color: rgba(15, 23, 42, 0.65);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1050;
  padding: 1rem;
}

.tecnm-modal-container {
  background: var(--tecnm-surface-white);
  border: 1px solid var(--tecnm-border-color);
  border-radius: 12px;
  box-shadow: var(--tecnm-shadow-lg);
  width: 100%;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  color: var(--tecnm-text-primary);
}

.tecnm-modal--sm { max-width: 400px; }
.tecnm-modal--md { max-width: 560px; }
.tecnm-modal--lg { max-width: 800px; }
.tecnm-modal--xl { max-width: 1100px; }

.tecnm-modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.25rem;
  background: linear-gradient(135deg, var(--tecnm-navy, #1b396a) 0%, var(--tecnm-blue-dark, #0f2744) 100%);
  color: #ffffff;
}

.tecnm-modal-title {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.tecnm-modal-close {
  background: transparent;
  border: none;
  color: rgba(255, 255, 255, 0.8);
  font-size: 1.2rem;
  cursor: pointer;
  padding: 0.5rem;
  min-width: 44px;
  min-height: 44px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  transition: all 0.2s ease;
}

.tecnm-modal-close:hover {
  color: #ffffff;
  background: rgba(255, 255, 255, 0.15);
}

.tecnm-modal-body {
  padding: 1.25rem;
  overflow-y: auto;
  flex: 1;
}

.tecnm-modal-footer {
  padding: 1rem 1.25rem;
  background: var(--tecnm-soft-hover);
  border-top: 1px solid var(--tecnm-border-color);
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
}

/* Transiciones CSS */
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.3s ease;
}

.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}

.modal-fade-enter-active .tecnm-modal-container {
  transition: transform 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.modal-fade-leave-active .tecnm-modal-container {
  transition: transform 0.25s cubic-bezier(0.4, 0, 1, 1);
}

.modal-fade-enter-from .tecnm-modal-container {
  transform: scale(0.95) translateY(-10px);
}

.modal-fade-leave-to .tecnm-modal-container {
  transform: scale(0.95) translateY(10px);
}
</style>
