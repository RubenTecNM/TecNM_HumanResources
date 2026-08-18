import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

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
      '/api': {
        target: 'http://localhost:5270',
        changeOrigin: true
      },
      '/Account': {
        target: 'http://localhost:5270',
        changeOrigin: true
      },
      '/Permission': {
        target: 'http://localhost:5270',
        changeOrigin: true
      },
      '/ResetPassword': {
        target: 'http://localhost:5270',
        changeOrigin: true
      },
      '/health': {
        target: 'http://localhost:5270',
        changeOrigin: true
      }
    }
  }
})