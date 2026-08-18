import { createApp } from 'vue'
import { createPinia } from 'pinia'
import router from '@/router'
import App from '@/App.vue'
import { useTheme } from '@/composables/useTheme'

import '@/assets/css/tecnm-theme.css'
import '@/assets/css/main.css'
import '../node_modules/font-awesome/css/font-awesome.min.css'

useTheme().initTheme()

const app = createApp(App)
app.use(createPinia())
app.use(router)
app.mount('#app')