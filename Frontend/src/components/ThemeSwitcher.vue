<template>
  <div class="theme-switcher" :class="{ 'is-collapsed': collapsed }" ref="rootEl">
    <button
      type="button"
      class="theme-switcher__trigger"
      :class="{ 'is-open': open }"
      :aria-expanded="open"
      aria-haspopup="listbox"
      :title="`Tema: ${currentTheme.label}`"
      @click="toggle"
      ref="triggerEl"
    >
      <i class="fa" :class="currentTheme.icon" aria-hidden="true"></i>
      <span v-if="!collapsed" class="theme-switcher__label">{{ currentTheme.shortLabel }}</span>
      <i v-if="!collapsed" class="fa fa-chevron-down theme-switcher__caret" aria-hidden="true"></i>
    </button>

    <Teleport to="body">
      <Transition name="theme-switcher">
        <ul v-if="open" class="theme-switcher__menu" role="listbox" aria-label="Seleccionar tema" :style="menuStyle">
          <li class="theme-switcher__heading">Apariencia</li>
          <li
            v-for="opt in options"
            :key="opt.id"
            role="option"
            :aria-selected="opt.id === currentTheme.id"
            class="theme-switcher__option"
            :class="{ 'is-selected': opt.id === currentTheme.id }"
            @click="select(opt.id)"
          >
            <span class="theme-switcher__swatch" :style="swatchStyle(opt)">
              <span v-for="(c, i) in opt.swatch" :key="i" class="theme-switcher__swatch-dot" :style="{ background: c }"></span>
            </span>
            <span class="theme-switcher__option-label">
              {{ opt.label }}
              <span v-if="opt.animatedBg" class="theme-switcher__annot">· animado</span>
            </span>
            <i v-if="opt.id === currentTheme.id" class="fa fa-check theme-switcher__check" aria-hidden="true"></i>
          </li>
        </ul>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref } from 'vue'
import { useTheme, type ThemeOption } from '@/composables/useTheme'

const props = defineProps<{ collapsed?: boolean }>()

const { currentTheme, setTheme, options } = useTheme()

const open = ref(false)
const rootEl = ref<HTMLElement | null>(null)
const triggerEl = ref<HTMLElement | null>(null)
const menuStyle = ref<Record<string, string>>({})

function toggle() {
  if (!open.value && props.collapsed && triggerEl.value) {
    const rect = triggerEl.value.getBoundingClientRect()
    menuStyle.value = {
      position: 'fixed',
      left: rect.right + 4 + 'px',
      top: rect.top + 'px',
      bottom: 'auto',
    }
  }
  open.value = !open.value
}

function select(id: string) {
  setTheme(id)
  open.value = false
}

function swatchStyle(opt: ThemeOption) {
  return { background: `linear-gradient(135deg, ${opt.swatch.slice().reverse().join(' 0%, ') + ' 100%'})` }
}

function onClickOutside(event: MouseEvent) {
  if (rootEl.value && !rootEl.value.contains(event.target as Node)) {
    open.value = false
  }
}

function onKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape') open.value = false
}

onMounted(() => {
  document.addEventListener('click', onClickOutside)
  document.addEventListener('keydown', onKeydown)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', onClickOutside)
  document.removeEventListener('keydown', onKeydown)
})
</script>

<style scoped>
.theme-switcher {
  position: relative;
}

.theme-switcher__trigger {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  height: 2.35rem;
  padding: 0 0.9rem;
  border: 1px solid rgba(255, 255, 255, 0.22);
  border-radius: var(--tecnm-radius-full);
  background: rgba(255, 255, 255, 0.08);
  color: var(--tecnm-text-on-blue);
  font-family: inherit;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  transition: var(--tecnm-transition);
}

.theme-switcher__trigger:hover,
.theme-switcher__trigger.is-open {
  background: rgba(255, 255, 255, 0.16);
  border-color: var(--tecnm-gold-accent);
}

.theme-switcher__trigger i {
  color: var(--tecnm-gold-accent);
}

.theme-switcher__caret {
  font-size: 0.75rem;
  transition: transform var(--tecnm-transition-fast);
}

.theme-switcher__trigger.is-open .theme-switcher__caret {
  transform: rotate(180deg);
}

.theme-switcher__label {
  line-height: 1;
}

.theme-switcher__menu {
  position: absolute;
  right: 0;
  top: calc(100% + 0.5rem);
  min-width: 250px;
  padding: 0.5rem;
  list-style: none;
  margin: 0;
  border: 1px solid var(--tecnm-border-color);
  border-radius: var(--tecnm-radius-md);
  background: var(--tecnm-surface-white);
  box-shadow: var(--tecnm-shadow-xl);
  z-index: 1100;
}

.theme-switcher__heading {
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: var(--tecnm-text-secondary);
  padding: 0.35rem 0.6rem;
}

.theme-switcher__option {
  display: flex;
  align-items: center;
  gap: 0.7rem;
  padding: 0.55rem 0.6rem;
  border-radius: var(--tecnm-radius-sm);
  color: var(--tecnm-text-primary);
  font-size: 0.9rem;
  cursor: pointer;
  transition: var(--tecnm-transition);
}

.theme-switcher__option:hover {
  background: var(--tecnm-gold-light);
}

.theme-switcher__option.is-selected {
  background: var(--tecnm-gold-light);
  color: var(--tecnm-accent-text);
}

.theme-switcher__swatch {
  display: flex;
  gap: 3px;
  width: 30px;
  height: 20px;
  border-radius: 5px;
  padding: 3px;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--tecnm-border-color);
  flex-shrink: 0;
}

.theme-switcher__swatch-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  border: 1px solid rgba(0, 0, 0, 0.15);
}

.theme-switcher__option-label {
  flex: 1;
  display: flex;
  align-items: baseline;
  gap: 0.35rem;
}

.theme-switcher__annot {
  font-size: 0.72rem;
  color: var(--tecnm-text-secondary);
}

.theme-switcher__check {
  color: var(--tecnm-gold-accent);
}

.theme-switcher-enter-active,
.theme-switcher-leave-active {
  transition: opacity var(--tecnm-transition), transform var(--tecnm-transition);
  transform-origin: top right;
}

.theme-switcher-enter-from,
.theme-switcher-leave-to {
  opacity: 0;
  transform: translateY(-6px) scale(0.97);
}

@media (max-width: 640px) {
  .theme-switcher__label {
    display: none;
  }
}

.theme-switcher.is-collapsed .theme-switcher__trigger {
  padding: 0;
  width: 100%;
  height: 44px;
  justify-content: center;
  border-color: transparent;
  background: transparent;
  border-radius: var(--tecnm-radius-md);
}

.theme-switcher.is-collapsed .theme-switcher__trigger:hover,
.theme-switcher.is-collapsed .theme-switcher__trigger.is-open {
  background: var(--tecnm-gold-light);
  border-color: var(--tecnm-gold-accent);
}
</style>