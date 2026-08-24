import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const routes = [
  {
    path: '/login',
    name: 'login',
    component: () => import('@/views/LoginView.vue'),
    meta: { public: true, title: 'Iniciar Sesión' }
  },
  {
    path: '/forgot-password',
    name: 'forgot-password',
    component: () => import('@/views/auth/ForgotPasswordView.vue'),
    meta: { public: true, title: 'Recuperar Contraseña' }
  },
  {
    path: '/reset-password',
    name: 'reset-password',
    component: () => import('@/views/auth/ResetPasswordView.vue'),
    meta: { public: true, title: 'Restablecer Contraseña' }
  },
  {
    path: '/',
    component: () => import('@/layouts/AppLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      { path: '', redirect: '/permissions' },
      {
        path: 'permissions',
        name: 'permissions.index',
        component: () => import('@/views/permissions/PermissionsListView.vue'),
        meta: { title: 'Gestión de Permisos' }
      },
      {
        path: 'permissions/create',
        name: 'permissions.create',
        component: () => import('@/views/permissions/PermissionCreateView.vue'),
        meta: { title: 'Nuevo Permiso', roles: [1, 3, 4] }
      },
      {
        path: 'permissions/:id',
        name: 'permissions.details',
        component: () => import('@/views/permissions/PermissionDetailsView.vue'),
        meta: { title: 'Detalle de Permiso' }
      },
      {
        path: 'permissions/:id/edit',
        name: 'permissions.edit',
        component: () => import('@/views/permissions/PermissionEditView.vue'),
        meta: { title: 'Editar Permiso', roles: [1, 3] }
      },
      {
        path: 'permissions/:id/authorize',
        name: 'permissions.authorize',
        component: () => import('@/views/permissions/PermissionAuthorizeView.vue'),
        meta: { title: 'Autorizar Permiso', roles: [1, 2] }
      },
      {
        path: 'users',
        name: 'users.index',
        component: () => import('@/views/users/UsersListView.vue'),
        meta: { title: 'Gestión de Usuarios', roles: [1, 3] }
      },
      {
        path: 'users/create',
        name: 'users.create',
        component: () => import('@/views/users/UserFormView.vue'),
        meta: { title: 'Nuevo Usuario', roles: [1, 3] }
      },
      {
        path: 'users/:id/edit',
        name: 'users.edit',
        component: () => import('@/views/users/UserFormView.vue'),
        meta: { title: 'Editar Usuario', roles: [1, 3] }
      },
      {
        path: 'cutoffs',
        name: 'cutoffs.index',
        component: () => import('@/views/cutoffs/CutoffsListView.vue'),
        meta: { title: 'Gestión de Cortes', roles: [1, 3] }
      },
      {
        path: 'cutoffs/create',
        name: 'cutoffs.create',
        component: () => import('@/views/cutoffs/CutoffCreateView.vue'),
        meta: { title: 'Nuevo Periodo de Corte', roles: [1, 3] }
      },
      {
        path: 'cutoffs/:id/edit',
        name: 'cutoffs.edit',
        component: () => import('@/views/cutoffs/CutoffEditView.vue'),
        meta: { title: 'Editar Periodo de Corte', roles: [1, 3] }
      },
      {
        path: 'cutoffs/:id',
        name: 'cutoffs.details',
        component: () => import('@/views/cutoffs/CutoffDetailsView.vue'),
        meta: { title: 'Dashboard Analítico', roles: [1, 3] }
      },
      {
        path: 'departments',
        name: 'departments.index',
        component: () => import('@/views/departments/DepartmentsListView.vue'),
        meta: { title: 'Gestión de Departamentos', roles: [1, 3] }
      },
      {
        path: 'areas',
        name: 'areas.index',
        component: () => import('@/views/areas/AreasListView.vue'),
        meta: { title: 'Gestión de Áreas', roles: [1, 3] }
      },
      {
        path: 'audit',
        name: 'audit.index',
        component: () => import('@/views/audit/AuditLogListView.vue'),
        meta: { title: 'Auditoría', roles: [1] }
      },
      {
        path: 'sync',
        name: 'sync.index',
        component: () => import('@/views/sync/SyncView.vue'),
        meta: { title: 'Sincronización de Datos', roles: [1, 3] }
      },
      {
        path: 'api-keys',
        name: 'api-keys.index',
        component: () => import('@/views/apikeys/ApiKeysListView.vue'),
        meta: { title: 'API Keys', roles: [1] }
      }
    ]
  },
  {
    path: '/403',
    name: 'forbidden',
    component: () => import('@/views/ForbiddenView.vue'),
    meta: { public: true, title: 'Acceso Denegado' }
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'notfound',
    component: () => import('@/views/NotFoundView.vue'),
    meta: { public: true, title: 'No Encontrado' }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior() {
    return { top: 0 }
  }
})

router.beforeEach(async (to) => {
  const auth = useAuthStore()
  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    await auth.fetchMe()
  }
  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }
  const roles = to.meta.roles as number[] | undefined
  if (roles && auth.user && !roles.includes(auth.user.userType)) {
    return { name: 'forbidden' }
  }
  if (to.name === 'login') {
    if (!auth.isAuthenticated) {
      // Rehidratar por si el navegador aún guarda la cookie de sesión previa.
      await auth.fetchMe()
    }
    if (auth.isAuthenticated) {
      return { name: 'permissions.index' }
    }
  }
  return true
})

router.afterEach((to) => {
  const base = typeof to.meta.title === 'string' ? to.meta.title : 'Iniciar Sesión'
  document.title = `${base} | TecNM`
})

export default router