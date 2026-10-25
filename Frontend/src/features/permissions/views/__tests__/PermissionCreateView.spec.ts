import { describe, it, expect, beforeEach, vi } from "vitest"
import { mount, flushPromises } from "@vue/test-utils"
import { createPinia, setActivePinia } from "pinia"
import PermissionCreateView from "../PermissionCreateView.vue"
import * as permissionsApi from "@/features/permissions/api/permissions"
import { createMockCreateConfig, createMockPermissionType } from "../../__tests__/factories"

vi.mock("@/features/permissions/api/permissions", async () => {
  const actual = await vi.importActual<any>("@/features/permissions/api/permissions")
  return {
    ...actual,
    fetchPermissionConfig: vi.fn(),
    createPermission: vi.fn()
  }
})

vi.mock("vue-router", () => ({
  useRouter: () => ({
    push: vi.fn(),
    replace: vi.fn()
  }),
  useRoute: () => ({
    params: {},
    query: {}
  })
}))

describe("PermissionCreateView.vue Component", () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it("consume fetchPermissionConfig al montarse y renderiza los tipos devueltos", async () => {
    const mockConfig = createMockCreateConfig({
      availableTypes: [
        createMockPermissionType({ id: 1, description: "Retardo Oficial" }),
        createMockPermissionType({ id: 2, description: "Paternidad / Maternidad" })
      ]
    })
    vi.mocked(permissionsApi.fetchPermissionConfig).mockResolvedValueOnce(mockConfig)

    const wrapper = mount(PermissionCreateView, {
      global: {
        stubs: {
          AppAutocomplete: true,
          AppButton: true,
          AppFormField: true,
          AppSpinner: true
        }
      }
    })

    await flushPromises()

    expect(permissionsApi.fetchPermissionConfig).toHaveBeenCalledTimes(1)
    expect(wrapper.text()).toContain("Retardo Oficial")
    expect(wrapper.text()).toContain("Paternidad / Maternidad")
  })

  it("oculta el selector de solicitante si canChooseRequester es false", async () => {
    const mockConfig = createMockCreateConfig({ canChooseRequester: false })
    vi.mocked(permissionsApi.fetchPermissionConfig).mockResolvedValueOnce(mockConfig)

    const wrapper = mount(PermissionCreateView, {
      global: {
        stubs: { AppAutocomplete: true, AppButton: true, AppFormField: true }
      }
    })

    await flushPromises()

    // No debe existir el autocomplete para elegir solicitante
    expect(wrapper.findComponent({ name: "AppAutocomplete" }).exists()).toBe(false)
  })

  it("muestra el selector de solicitante si canChooseRequester es true", async () => {
    const mockConfig = createMockCreateConfig({ canChooseRequester: true })
    vi.mocked(permissionsApi.fetchPermissionConfig).mockResolvedValueOnce(mockConfig)

    const wrapper = mount(PermissionCreateView, {
      global: {
        stubs: { AppAutocomplete: true, AppButton: true, AppFormField: true }
      }
    })

    await flushPromises()

    expect(wrapper.findComponent({ name: "AppAutocomplete" }).exists()).toBe(true)
  })
})
