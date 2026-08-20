import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

const BACKEND = 'http://localhost:5270'

const proxyEntry = {
  target: BACKEND,
  changeOrigin: true,
  // Reescribe el dominio de las cookies para que el navegador las envíe
  // correctamente desde :5173 al volver a pasar por el proxy hacia :5270.
  cookieDomainRewrite: 'localhost'
}

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  server: {
    port: 5173,
    proxy: {
      '/api':          proxyEntry,
      '/Account':      proxyEntry,
      '/Permission':   proxyEntry,
      '/ResetPassword': proxyEntry,
      '/health':       proxyEntry
    }
  }
})