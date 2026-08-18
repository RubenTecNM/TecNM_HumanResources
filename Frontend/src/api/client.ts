import axios from 'axios'

export const http = axios.create({
  baseURL: '/',
  withCredentials: true,
  headers: { Accept: 'application/json' }
})

// REST API is protected by X-API-KEY (backend ApiKeyAuthorizeAttribute).
// The key can be supplied via VITE_API_KEY; leave empty to use an injected key at runtime.
const apiKey = (import.meta.env.VITE_API_KEY as string | undefined) || ''

// Attach X-API-KEY on every request when configured.
http.interceptors.request.use((config) => {
  if (apiKey) config.headers['X-API-KEY'] = apiKey
  return config
})

// Need antiforgery token: the backend sets cookie + we read meta/input if present.
// For cookie-auth endpoints (.NET cookie login), the JS cookie is sent automatically.

http.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error?.response?.status
    if (status === 401 || status === 403) {
      // Let the router guard react; do not force-reload on API 403 that includes X-API-KEY auth
    }
    return Promise.reject(error)
  }
)

export function isUnauthorized(error: unknown): boolean {
  return (error as { response?: { status?: number } })?.response?.status === 401
}

export function isForbidden(error: unknown): boolean {
  return (error as { response?: { status?: number } })?.response?.status === 403
}