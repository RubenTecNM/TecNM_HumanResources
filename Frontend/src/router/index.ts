import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/features/auth/stores/auth'

const routes = [
  {
    path: '/login',
    name: 'login',
    component: () => import('@/features/auth/views/LoginView.vue'),
    meta: { public: true, title: 'Iniciar Sesión' }
  },
  {
    path: '/forgot-password',
    name: 'forgot-password',
    component: () => import('@/features/auth/views/ForgotPasswordView.vue'),
    meta: { public: true, title: 'Recuperar Contraseña' }
  },
  {
    path: '/reset-password',
    name: 'reset-password',
    component: () => import('@/features/auth/views/ResetPasswordView.vue'),
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
        component: () => import('@/features/permissions/views/PermissionsListView.vue'),
        meta: { title: 'Gestión de Permisos' }
      },
      {
        path: 'permissions/create',
        name: 'permissions.create',
        component: () => import('@/features/permissions/views/PermissionCreateView.vue'),
        meta: { title: 'Nuevo Permiso', roles: [1, 3, 4] }
      },
      {
        path: 'permissions/:id',
        name: 'permissions.details',
        component: () => import('@/features/permissions/views/PermissionDetailsView.vue'),
        meta: { title: 'Detalle de Permiso' }
      },
      {
        path: 'permissions/:id/edit',
        name: 'permissions.edit',
        component: () => import('@/features/permissions/views/PermissionEditView.vue'),
        meta: { title: 'Editar Permiso', roles: [1, 3] }
      },
      {
        path: 'permissions/:id/authorize',
        name: 'permissions.authorize',
        component: () => import('@/features/permissions/views/PermissionAuthorizeView.vue'),
        meta: { title: 'Autorizar Permiso', roles: [1, 2] }
      },
      {
        path: 'users',
        name: 'users.index',
        component: () => import('@/features/users/views/UsersListView.vue'),
        meta: { title: 'Gestión de Usuarios', roles: [1, 3] }
      },
      {
        path: 'users/create',
        name: 'users.create',
        component: () => import('@/features/users/views/UserFormView.vue'),
        meta: { title: 'Nuevo Usuario', roles: [1, 3] }
      },
      {
        path: 'users/:id/edit',
        name: 'users.edit',
        component: () => import('@/features/users/views/UserFormView.vue'),
        meta: { title: 'Editar Usuario', roles: [1, 3] }
      },
      {
        path: 'cutoffs',
        name: 'cutoffs.index',
        component: () => import('@/features/cutoffs/views/CutoffsListView.vue'),
        meta: { title: 'Gestión de Cortes', roles: [1, 3] }
      },
      {
        path: 'cutoffs/create',
        name: 'cutoffs.create',
        component: () => import('@/features/cutoffs/views/CutoffCreateView.vue'),
        meta: { title: 'Nuevo Periodo de Corte', roles: [1, 3] }
      },
      {
        path: 'cutoffs/:id/edit',
        name: 'cutoffs.edit',
        component: () => import('@/features/cutoffs/views/CutoffEditView.vue'),
        meta: { title: 'Editar Periodo de Corte', roles: [1, 3] }
      },
      {
        path: 'cutoffs/:id',
        name: 'cutoffs.details',
        component: () => import('@/features/cutoffs/views/CutoffDetailsView.vue'),
        meta: { title: 'Dashboard Analítico', roles: [1, 3] }
      },
      {
        path: 'departments',
        name: 'departments.index',
        component: () => import('@/features/departments/views/DepartmentsListView.vue'),
        meta: { title: 'Gestión de Departamentos', roles: [1, 3] }
      },
      {
        path: 'areas',
        name: 'areas.index',
        component: () => import('@/features/areas/views/AreasListView.vue'),
        meta: { title: 'Gestión de Áreas', roles: [1, 3] }
      },
      {
        path: 'organization',
        name: 'organization.index',
        component: () => import('@/features/organization/views/OrganizationStructureView.vue'),
        meta: { title: 'Estructura Organizacional', roles: [1, 3] }
      },
      {
        path: 'positions',
        name: 'positions.index',
        component: () => import('@/features/positions/views/PositionsListView.vue'),
        meta: { title: 'Gestión de Puestos', roles: [1, 3] }
      },
      
      {
        path: 'sync',
        name: 'sync.index',
        component: () => import('@/features/sync/views/SyncView.vue'),
        meta: { title: 'Sincronización de Datos', roles: [1, 3] }
      },
      {
        path: 'api-keys',
        name: 'api-keys.index',
        component: () => import('@/features/apikeys/views/ApiKeysListView.vue'),
        meta: { title: 'API Keys', roles: [1] }
      }
    ]
  },
  {
    path: '/403',
    name: 'forbidden',
    component: () => import('@/features/auth/views/ForbiddenView.vue'),
    meta: { public: true, title: 'Acceso Denegado' }
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'notfound',
    component: () => import('@/shared/components/NotFoundView.vue'),
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