<template>
  <div>
    <AppPageHeader
      :title="isEdit ? 'Editar Usuario' : 'Nuevo Usuario'"
      :subtitle="isEdit ? 'Modificación de datos organizacionales' : 'Registro de nuevo colaborador'"
      icon="fa-user-cog"
    >
      <template #actions>
        <AppButton variant="secondary" icon="fa-arrow-left" @click="$router.push({ name: 'users.index' })">
          Volver a la Lista
        </AppButton>
      </template>
    </AppPageHeader>

    <section class="tecnm-card">
      <div class="tecnm-card-header tecnm-card-header--premium">
        <h4 class="filter-title">
          <i class="fa fa-id-card" aria-hidden="true"></i>
          {{ isEdit ? 'Datos del Colaborador' : 'Registro de Nuevo Colaborador' }}
        </h4>
      </div>
      <div class="tecnm-card-body">
        <form @submit.prevent="handleSubmit" novalidate>

          <!-- Sección: Datos Personales -->
          <p class="section-label"><i class="fa fa-user" aria-hidden="true"></i> Datos Personales</p>
          <div class="form-grid">
            <div class="tecnm-form-group">
              <label class="tecnm-label" for="name">Nombre Completo <span class="required-mark">*</span></label>
              <input
                id="name"
                type="text"
                v-model="form.name"
                class="tecnm-form-control"
                placeholder="Ej. Juan Pérez González"
                required
                :disabled="submitting"
              />
            </div>

            <div class="tecnm-form-group">
              <label class="tecnm-label" for="email">Correo Institucional <span class="required-mark">*</span></label>
              <input
                id="email"
                type="email"
                v-model="form.email"
                class="tecnm-form-control"
                placeholder="ejemplo@monclova.tecnm.mx"
                required
                :disabled="submitting"
              />
            </div>

            <!-- Contraseña con toggle visible -->
            <div v-if="!isEdit || showPasswordReset" class="tecnm-form-group">
              <label class="tecnm-label" for="password">
                {{ isEdit ? 'Nueva Contraseña' : 'Contraseña' }} <span class="required-mark">*</span>
              </label>
              <div class="password-field">
                <input
                  id="password"
                  :type="showPassword ? 'text' : 'password'"
                  v-model="form.password"
                  class="tecnm-form-control"
                  placeholder="••••••••"
                  :required="!isEdit"
                  :disabled="submitting"
                />
                <button
                  type="button"
                  class="password-toggle-btn"
                  :title="showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'"
                  @click="showPassword = !showPassword"
                >
                  <i :class="showPassword ? 'fa fa-eye-slash' : 'fa fa-eye'" aria-hidden="true"></i>
                </button>
              </div>
            </div>

            <!-- Botón restablecer contraseña (solo edit mode) -->
            <div v-if="isEdit && !showPasswordReset" class="tecnm-form-group align-end">
              <AppButton type="button" variant="outline" icon="fa-key" size="sm" @click="showPasswordReset = true">
                Restablecer Contraseña
              </AppButton>
            </div>
          </div>

          <!-- Sección: Datos Organizacionales -->
          <p class="section-label mt-4"><i class="fa fa-building" aria-hidden="true"></i> Datos Organizacionales</p>
          <div class="form-grid">
            <div class="tecnm-form-group">
              <label class="tecnm-label" for="areaId">Área <span class="required-mark">*</span></label>
              <select id="areaId" v-model="form.areaId" class="tecnm-form-control" required :disabled="submitting">
                <option :value="null">Seleccione un área...</option>
                <option v-for="a in areas" :key="a.id" :value="a.id">{{ a.description }}</option>
              </select>
            </div>

            <div class="tecnm-form-group">
              <label class="tecnm-label" for="departmentId">Departamento <span class="required-mark">*</span></label>
              <select id="departmentId" v-model="form.departmentId" class="tecnm-form-control" required :disabled="submitting">
                <option :value="null">Seleccione un departamento...</option>
                <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.description }}</option>
              </select>
            </div>

            <div class="tecnm-form-group">
              <label class="tecnm-label" for="positionId">Puesto <span class="required-mark">*</span></label>
              <select id="positionId" v-model="form.positionId" class="tecnm-form-control" required :disabled="submitting">
                <option :value="null">Seleccione un puesto...</option>
                <option v-for="p in positions" :key="p.id" :value="p.id">{{ p.description }}</option>
              </select>
            </div>

            <div class="tecnm-form-group">
              <label class="tecnm-label" for="userTypeId">Rol de Acceso <span class="required-mark">*</span></label>
              <select id="userTypeId" v-model="form.userTypeId" class="tecnm-form-control" required :disabled="submitting">
                <option :value="null">Seleccione un rol...</option>
                <option v-for="t in userTypes" :key="t.id" :value="t.id">{{ t.description }}</option>
              </select>
            </div>
          </div>

          <!-- Error -->
          <div v-if="errorMessage" class="tecnm-alert tecnm-alert-danger tecnm-mt-3">
            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
            <span>{{ errorMessage }}</span>
          </div>

          <!-- Acciones -->
          <div class="form-actions">
            <AppButton type="submit" icon="fa-save" variant="primary" :loading="submitting">
              {{ isEdit ? 'Guardar Cambios' : 'Registrar Usuario' }}
            </AppButton>
            <AppButton type="button" variant="secondary" icon="fa-arrow-left" @click="$router.push({ name: 'users.index' })">
              Cancelar
            </AppButton>
          </div>
        </form>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { http } from "@/shared/api/client"
import AppPageHeader from '@/shared/components/AppPageHeader.vue'
import AppButton from '@/shared/components/AppButton.vue'
import { showToast } from '@/shared/composables/useAlerts'

const route = useRoute()
const router = useRouter()

const userId = computed(() => route.params.id as string | undefined)
const isEdit = computed(() => !!userId.value)

const showPasswordReset = ref(false)
const showPassword = ref(false)
const submitting = ref(false)
const errorMessage = ref('')

const departments = ref<any[]>([])
const areas = ref<any[]>([])
const positions = ref<any[]>([])
const userTypes = ref<any[]>([])

const form = reactive({
  id: 0,
  name: '',
  email: '',
  password: '',
  departmentId: null as number | null,
  areaId: null as number | null,
  positionId: null as number | null,
  userTypeId: null as number | null,
  status: true
})

async function fetchCatalogs() {
  try {
    const [depRes, areaRes, posRes, typeRes] = await Promise.all([
      http.get('/api/Departments'),
      http.get('/api/Areas'),
      http.get('/api/Positions'),
      http.get('/api/UserTypes')
    ])
    departments.value = depRes.data.items || depRes.data
    areas.value = areaRes.data.items || areaRes.data
    positions.value = posRes.data?.items || posRes.data || []
    userTypes.value = typeRes.data
  } catch (e) {
    console.error(e)
  }
}

async function fetchUserDetail() {
  if (!userId.value) return
  try {
    const res = await http.get(`/api/Users/${userId.value}`)
    const data = res.data
    form.id = data.id
    form.name = data.name
    form.email = data.email
    form.departmentId = data.departmentId
    form.areaId = data.areaId
    form.positionId = data.positionId
    form.userTypeId = data.userTypeId
    form.status = data.status
  } catch (e) {
    console.error(e)
  }
}

async function handleSubmit() {
  submitting.value = true
  errorMessage.value = ''

  try {
    const url = isEdit.value ? `/api/Users/${userId.value}` : '/api/Users'

    const res = isEdit.value
      ? await http.put(url, form)
      : await http.post(url, form)

    if (res.status === 200 || res.status === 201 || res.status === 204) {
      showToast(isEdit.value ? 'Usuario actualizado correctamente.' : 'Usuario creado exitosamente.', 'success')
      router.push({ name: 'users.index' })
    }
  } catch (err: any) {
    errorMessage.value = err.response?.data?.message || 'Error al guardar los datos del usuario.'
  } finally {
    submitting.value = false
  }
}

onMounted(async () => {
  await fetchCatalogs()
  if (isEdit.value) {
    await fetchUserDetail()
  }
})
</script>

<style scoped>
.section-label {
  font-size: 0.8rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--tecnm-text-secondary);
  display: flex;
  align-items: center;
  gap: 0.4rem;
  margin-bottom: 0.75rem;
  padding-bottom: 0.4rem;
  border-bottom: 1px solid var(--tecnm-border-color, #e2e8f0);
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 0 1.25rem;
}

.align-end {
  display: flex;
  align-items: flex-end;
}

/* Password field with toggle button */
.password-field {
  position: relative;
  display: flex;
  align-items: center;
}

.password-field .tecnm-form-control {
  padding-right: 2.6rem;
}

.password-toggle-btn {
  position: absolute;
  right: 0.6rem;
  background: transparent;
  border: none;
  color: var(--tecnm-text-secondary);
  cursor: pointer;
  padding: 0.2rem 0.3rem;
  line-height: 1;
  transition: color 0.15s;
}

.password-toggle-btn:hover {
  color: var(--tecnm-navy);
}

.required-mark {
  color: var(--tecnm-danger);
}

.form-actions {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding-top: 1.25rem;
  margin-top: 1rem;
  border-top: 1px solid var(--tecnm-border-color, #e2e8f0);
}

.filter-title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin: 0;
}

.mt-4 {
  margin-top: 1.5rem;
}

.tecnm-mt-3 {
  margin-top: 0.75rem;
}
</style>
