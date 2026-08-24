import { defineStore } from 'pinia'
import { http } from '@/api/client'
import type { AlertResponse } from '@/composables/useAlerts'

export interface AuthUser {
  id: number
  name: string
  email: string
  userType: number
  isDocente?: boolean
  area?: string
  department?: string
  position?: string
}

interface LoginPayload {
  Email: string
  Password: string
  RememberMe: boolean
  ReturnUrl?: string
}

export const USER_TYPE = {
  ADMINISTRADOR: 1,
  AUTORIZADOR: 2,
  RECURSOS_HUMANOS: 3
} as const

// Extracts the __RequestVerificationToken from the served login page HTML
function extractAntiforgeryToken(html: string): string {
  const match = html.match(/name="__RequestVerificationToken"[^>]*value="([^"]+)"/)
  return match ? match[1] : ''
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as AuthUser | null,
    loading: false
  }),

  getters: {
    isAuthenticated: (state) => state.user !== null,
    userType: (state) => state.user?.userType ?? null,
    isAdmin: (state) => state.user?.userType === USER_TYPE.ADMINISTRADOR,
    isRH: (state) => state.user?.userType === USER_TYPE.RECURSOS_HUMANOS,
    isAuthorizer: (state) => state.user?.userType === USER_TYPE.AUTORIZADOR,
    canManageUsers: (state) =>
      state.user?.userType === USER_TYPE.ADMINISTRADOR || state.user?.userType === USER_TYPE.RECURSOS_HUMANOS,
    isRHOrAdmin: (state) =>
      state.user?.userType === USER_TYPE.ADMINISTRADOR || state.user?.userType === USER_TYPE.RECURSOS_HUMANOS,
    isAutorizadorOrAbove: (state) =>
      state.user?.userType === USER_TYPE.ADMINISTRADOR ||
      state.user?.userType === USER_TYPE.RECURSOS_HUMANOS ||
      state.user?.userType === USER_TYPE.AUTORIZADOR,
    initial: (state) => (state.user?.name?.charAt(0) ?? 'U').toUpperCase()
  },

  actions: {
    // .NET cookie auth flow: GET login page (sets antiforgery cookie + token),
    // then POST credentials; both cookies are automatically sent via withCredentials.
    async login(payload: LoginPayload): Promise<AlertResponse> {
      this.loading = true
      try {
        const tokenPage = await http.get<string>('/Account/Login', {
          params: payload.ReturnUrl ? { returnUrl: payload.ReturnUrl } : {}
        })
        const token = extractAntiforgeryToken(tokenPage.data)
        if (!token) {
          // El servidor pudo redirigir al index porque el navegador ya tiene una
          // cookie de sesión válida (login previo). Verificar antes de fallar.
          const existing = await this.fetchCurrentUser()
          if (existing) {
            return {
              success: true,
              title: '¡Éxito!',
              message: 'Sesión ya iniciada.',
              redirectUrl: '/'
            }
          }
          return { success: false, title: 'Error', message: 'No se pudo obtener el token de seguridad.' }
        }

        const form = new URLSearchParams()
        form.set('__RequestVerificationToken', token)
        form.set('Email', payload.Email)
        form.set('Password', payload.Password)
        form.set('RememberMe', payload.RememberMe ? 'true' : 'false')
        form.set('ReturnUrl', payload.ReturnUrl || '')

        const res = await http.post('/Account/Login', form, {
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
        })

        // Failure: server re-renders the login HTML with model errors.
        if (typeof res.data === 'string' && res.data.includes('loginForm')) {
          const summary = res.data.match(/validation-summary-errors">([\s\S]*?)<\/div>/)
          const message = summary
            ? summary[1].replace(/<[^>]+>/g, ' ').replace(/\s+/g, ' ').trim()
            : 'Email o contraseña incorrectos'
          return { success: false, title: 'Error', message }
        }

        // Success: server redirected (browser followed) to Permission/Index.
        const finalUrl = (res.request?.responseURL as string) || window.location.href
        const loggedIn = !finalUrl.toLowerCase().includes('/account/login')
        if (loggedIn) {
          await this.fetchCurrentUser()
        }
        return {
          success: true,
          title: '¡Éxito!',
          message: 'Sesión iniciada correctamente.',
          redirectUrl: '/'
        }
      } catch (error: unknown) {
        const e = error as { response?: { data?: unknown } }
        const data = e.response?.data
        if (data && typeof data === 'object' && 'success' in data) return data as AlertResponse
        return { success: false, title: 'Error', message: 'No se pudo iniciar sesión. Inténtelo nuevamente.' }
      } finally {
        this.loading = false
      }
    },

    // Rehydrate from existing cookie without credentials.
    async fetchCurrentUser(): Promise<AuthUser | null> {
      try {
        const res = await http.get<AuthUser>('/Account/Me')
        this.user = res.data
        return this.user
      } catch {
        this.user = null
        return null
      }
    },

    async fetchMe(): Promise<void> {
      await this.fetchCurrentUser()
    },

    async logout() {
      try {
        const page = await http.get<string>('/Account/Login')
        const token = extractAntiforgeryToken(page.data)
        const form = new URLSearchParams()
        form.set('__RequestVerificationToken', token)
        await http.post('/Account/Logout', form, {
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
        })
      } catch { /* ignore */ }
      this.user = null
    }
  }
})