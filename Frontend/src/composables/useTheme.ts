import { ref, computed } from 'vue'

export interface ThemeOption {
  id: string
  label: string
  shortLabel: string
  icon: string
  animatedBg: boolean
  swatch: string[]
}

export const THEME_OPTIONS: ThemeOption[] = [
  {
    id: 'tecnm',
    label: 'Claro institucional',
    shortLabel: 'TecNM',
    icon: 'fa-sun-o',
    animatedBg: false,
    swatch: ['#1B396A', '#C5A059', '#F4F6F9']
  },
  {
    id: 'dark',
    label: 'Oscuro',
    shortLabel: 'Oscuro',
    icon: 'fa-moon-o',
    animatedBg: true,
    swatch: ['#0F2548', '#C5A059', '#16233A']
  },
  {
    id: 'contrast',
    label: 'Alto contraste',
    shortLabel: 'Contraste',
    icon: 'fa-eye',
    animatedBg: false,
    swatch: ['#0B0B0F', '#FFDD00', '#FFFFFF']
  }
]

const STORAGE_KEY = 'tecnm-theme'
const FALLBACK_THEME = 'tecnm'

function readStoredTheme(): string {
  if (typeof localStorage !== 'undefined') {
    const stored = localStorage.getItem(STORAGE_KEY)
    if (stored && THEME_OPTIONS.some((t) => t.id === stored)) return stored
  }
  return FALLBACK_THEME
}

const currentThemeId = ref<string>(readStoredTheme())

function applyThemeToDocument(id: string) {
  const root = document.documentElement
  root.setAttribute('data-theme', id)
  const option = THEME_OPTIONS.find((t) => t.id === id) ?? THEME_OPTIONS[0]
  root.style.colorScheme = option.id === 'dark' ? 'dark' : 'light'
}

export function useTheme() {
  const currentTheme = computed(() => THEME_OPTIONS.find((t) => t.id === currentThemeId.value) ?? THEME_OPTIONS[0])

  function setTheme(id: string) {
    if (!THEME_OPTIONS.some((t) => t.id === id)) return
    currentThemeId.value = id
    try {
      localStorage.setItem(STORAGE_KEY, id)
    } catch {
      /* storage unavailable */
    }
    applyThemeToDocument(id)
  }

  function initTheme() {
    applyThemeToDocument(currentThemeId.value)
  }

  return { currentTheme, setTheme, initTheme, options: THEME_OPTIONS }
}

export const themeKey = STORAGE_KEY