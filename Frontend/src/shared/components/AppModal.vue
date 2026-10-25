<template>
  <Teleport to="body">
    <Transition name="modal-pop">
      <div v-if="show" class="tecnm-modal-backdrop" @click.self="handleBackdropClick">
        <div
          ref="containerEl"
          class="modal tecnm-modal-container"
          :class="[`modal--${size}`, `tecnm-modal--${size}`]"
          role="dialog"
          aria-modal="true"
          :aria-labelledby="titleId"
        >
          <!-- Header (Guía TecNM: Variantes A Identidad, B Icono, C Simple) -->
          <div v-if="computedHeaderVariant !== 'none'" class="hd" :class="[`hd--${computedHeaderVariant}`]">
            <!-- Variante A: Identidad (perfiles, usuarios) -->
            <template v-if="computedHeaderVariant === 'identity'">
              <div v-if="eyebrow" class="eyebrow">{{ eyebrow }}</div>
              <div class="who">
                <div class="av">{{ avatarInitial || (title ? title.charAt(0).toUpperCase() : 'U') }}</div>
                <div>
                  <h2 :id="titleId">{{ title }}</h2>
                  <span v-if="pill || subtitle" class="pill">{{ pill || subtitle }}</span>
                </div>
              </div>
            </template>

            <!-- Variante B: Icono + título (detalle de registro, formularios con icono) -->
            <template v-else-if="computedHeaderVariant === 'icon'">
              <div v-if="eyebrow" class="eyebrow">{{ eyebrow }}</div>
              <div class="who">
                <div class="hic">
                  <i v-if="icon && !icon.includes('<svg')" :class="['fa', icon]" aria-hidden="true"></i>
                  <span v-else-if="icon" v-html="icon"></span>
                  <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                    <rect x="3" y="3" width="18" height="18" rx="3" />
                    <path d="M9 12h6M12 9v6" />
                  </svg>
                </div>
                <div>
                  <h2 :id="titleId">{{ title }}</h2>
                  <span v-if="pill || subtitle" class="pill">{{ pill || subtitle }}</span>
                </div>
              </div>
            </template>

            <!-- Variante C: Simple (solo título) -->
            <template v-else>
              <h2 :id="titleId">{{ title }}</h2>
            </template>

            <!-- Botón X estandarizado en rojo claro sobre fondo marino -->
            <button
              ref="closeBtnEl"
              type="button"
              class="x"
              aria-label="Cerrar"
              @click="close"
            >
              <svg viewBox="0 0 24 24" aria-hidden="true">
                <path d="M18 6L6 18M6 6l12 12" />
              </svg>
            </button>
          </div>

          <!-- Cuerpo (con scroll propio) -->
          <div class="bd tecnm-modal-body">
            <slot />
          </div>

          <!-- Footer estandarizado: secundario a la izquierda, primario a la derecha -->
          <div v-if="$slots.footer || showDefaultFooter" class="ft tecnm-modal-footer">
            <slot name="footer">
              <button
                v-if="showCancel"
                type="button"
                class="btn btn-ghost"
                :disabled="loading"
                @click="close"
              >
                {{ cancelText }}
              </button>

              <button
                v-if="showConfirm"
                type="button"
                class="btn"
                :class="confirmBtnClass"
                :disabled="loading"
                @click="$emit('confirm')"
              >
                <i v-if="loading" class="fa fa-circle-o-notch fa-spin me-1" aria-hidden="true"></i>
                {{ confirmText }}
              </button>
            </slot>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, watch, onUnmounted, useId, nextTick } from 'vue'

const props = withDefaults(
  defineProps<{
    show: boolean
    title: string
    subtitle?: string
    eyebrow?: string
    icon?: string
    size?: 'sm' | 'md' | 'lg' | 'xl'
    headerVariant?: 'identity' | 'icon' | 'plain' | 'none'
    avatarInitial?: string
    pill?: string
    loading?: boolean
    showDefaultFooter?: boolean
    showCancel?: boolean
    showConfirm?: boolean
    confirmText?: string
    cancelText?: string
    confirmVariant?: 'primary' | 'secondary' | 'danger' | 'accent' | 'outline'
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

const computedHeaderVariant = computed<'identity' | 'icon' | 'plain' | 'none'>(() => {
  if (props.headerVariant) return props.headerVariant
  if (props.avatarInitial) return 'identity'
  if (props.icon || props.eyebrow || props.pill) return 'icon'
  return 'plain'
})

const confirmBtnClass = computed(() => {
  if (props.confirmVariant === 'danger') return 'btn-danger'
  return 'btn-primary'
})

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
/* Backdrop */
.tecnm-modal-backdrop {
  position: fixed;
  inset: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(14, 28, 54, 0.5);
  backdrop-filter: blur(3px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1050;
  padding: 16px;
  box-sizing: border-box;
}

/* Base Modal (Guía TecNM Sección 2) */
.modal {
  width: min(480px, calc(100% - 32px));
  max-height: calc(100% - 32px);
  padding: 0;
  border: 1px solid var(--line, #e6e9ef);
  border-radius: var(--r-modal, 16px);
  background: #ffffff;
  color: var(--tx, #1e2f55);
  overflow: hidden;
  box-shadow: 0 24px 60px -12px rgba(20, 44, 84, 0.45);
  display: flex;
  flex-direction: column;
  box-sizing: border-box;
}

/* Tamaños estándar */
.modal--sm { width: min(400px, calc(100% - 32px)); }
.modal--md { width: min(480px, calc(100% - 32px)); }
.modal--lg { width: min(640px, calc(100% - 32px)); }
.modal--xl { width: min(800px, calc(100% - 32px)); }

/* Header Institucional TecNM */
.hd {
  position: relative;
  flex: none;
  padding: 18px 24px 22px;
  background: linear-gradient(135deg, var(--navy, #1b3a6b), var(--navy-d, #142c54));
  border-bottom: 3px solid var(--gold, #c9a24d);
  box-sizing: border-box;
}

.hd h2 {
  margin: 0;
  font-size: 20px;
  font-weight: 700;
  color: #ffffff;
  letter-spacing: 0.01em;
  line-height: 1.25;
}

.hd--plain {
  padding: 20px 56px 20px 24px;
}

.hd--plain h2 {
  font-size: 18px;
}

.eyebrow {
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.12em;
  color: var(--gold, #c9a24d);
  text-transform: uppercase;
}

.who {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-top: 12px;
}

/* Avatar de Identidad */
.av {
  flex: none;
  width: 60px;
  height: 60px;
  border-radius: 50%;
  background: var(--cream, #f7f2e5);
  color: var(--navy, #1b3a6b);
  display: grid;
  place-items: center;
  font-size: 24px;
  font-weight: 700;
  box-shadow: 0 0 0 3px rgba(201, 162, 77, 0.55);
}

/* Ficha de Icono de Registro */
.hic {
  flex: none;
  width: 52px;
  height: 52px;
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.12);
  display: grid;
  place-items: center;
  color: #ffffff;
  font-size: 22px;
}

.hic svg {
  width: 26px;
  height: 26px;
  stroke: #ffffff;
  stroke-width: 1.9;
  stroke-linecap: round;
  stroke-linejoin: round;
  fill: none;
}

.pill {
  display: inline-block;
  margin-top: 6px;
  padding: 3px 10px;
  border-radius: 99px;
  background: rgba(201, 162, 77, 0.2);
  color: #f0d9a0;
  font-size: 12px;
  font-weight: 600;
}

/* Botón Cerrar (X) */
.x {
  position: absolute;
  top: 12px;
  right: 12px;
  width: 34px;
  height: 34px;
  display: grid;
  place-items: center;
  border: 0;
  border-radius: 9px;
  background: transparent;
  color: #ff8a8a;
  cursor: pointer;
  transition: background 0.25s, color 0.25s;
}

.x:hover {
  background: rgba(255, 123, 123, 0.18);
  color: #ffb3b3;
}

.x svg {
  width: 18px;
  height: 18px;
  stroke: currentColor;
  stroke-width: 2.2;
  stroke-linecap: round;
  fill: none;
}

/* Cuerpo */
.bd {
  overflow-y: auto;
  padding: 16px 24px 20px;
  flex: 1;
  box-sizing: border-box;
}

/* Footer */
.ft {
  flex: none;
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  padding: 14px 24px;
  background: var(--soft, #f5f7fb);
  border-top: 1px solid var(--line, #e6e9ef);
  box-sizing: border-box;
}

/* Botones del modal */
.btn {
  height: 40px;
  padding: 0 22px;
  border: 0;
  border-radius: var(--r-ctl, 10px);
  font: 600 14px system-ui, -apple-system, sans-serif;
  cursor: pointer;
  transition: background 0.25s, transform 0.25s, box-shadow 0.25s, border-color 0.25s;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.btn:focus-visible {
  outline: none;
  box-shadow: 0 0 0 4px var(--gold-s, rgba(201, 162, 77, 0.28));
}

.btn-primary {
  background: var(--navy, #1b3a6b);
  color: #ffffff;
}

.btn-primary:hover {
  background: var(--navy-d, #142c54);
  transform: translateY(-1px);
}

.btn-ghost {
  background: #ffffff;
  color: var(--navy, #1b3a6b);
  border: 1px solid #d3dae7;
}

.btn-ghost:hover {
  background: var(--chip, #eaf0fa);
  border-color: #c9d6ee;
}

.btn-danger {
  background: var(--red, #d92d2d);
  color: #ffffff;
}

.btn-danger:hover {
  background: #b92323;
  transform: translateY(-1px);
}

.btn-danger:focus-visible {
  box-shadow: 0 0 0 4px rgba(217, 45, 45, 0.2);
}

/* Animaciones */
.modal-pop-enter-active {
  transition: opacity 0.3s ease;
}

.modal-pop-leave-active {
  transition: opacity 0.25s ease;
}

.modal-pop-enter-from,
.modal-pop-leave-to {
  opacity: 0;
}

.modal-pop-enter-active .modal {
  animation: pop 0.3s cubic-bezier(0.2, 0.8, 0.2, 1);
}

.modal-pop-leave-active .modal {
  transition: transform 0.25s cubic-bezier(0.4, 0, 1, 1), opacity 0.25s ease;
  transform: translateY(10px) scale(0.97);
  opacity: 0;
}

@keyframes pop {
  from {
    opacity: 0;
    transform: translateY(10px) scale(0.97);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

@media (prefers-reduced-motion: reduce) {
  .modal-pop-enter-active .modal,
  .modal-pop-leave-active .modal {
    animation: none;
    transition: none;
  }
}
</style>
