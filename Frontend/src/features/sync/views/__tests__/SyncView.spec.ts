import { describe, it, expect, vi, beforeEach } from "vitest"
import { mount, flushPromises } from "@vue/test-utils"
import SyncView from "../SyncView.vue"
import AppButton from "@/shared/components/AppButton.vue"
import AppPageHeader from "@/shared/components/AppPageHeader.vue"
import * as syncApi from "../../api/sync"
import * as warnings from "../../composables/useSyncWarnings"

vi.mock("../../api/sync", () => ({
  downloadTemplate: vi.fn(),
  validateFile: vi.fn(),
  runImport: vi.fn()
}))

vi.mock("../../composables/useSyncWarnings", () => ({
  confirmImportWithWarnings: vi.fn()
}))


vi.mock("@/shared/composables/useAlerts", () => ({
  showToast: vi.fn()
}))

vi.mock("vue-router", () => ({
  useRouter: () => ({ back: vi.fn() })
}))

describe("SyncView.vue Component", () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it("renderiza el titulo y las 3 secciones principales de sincronizacion", () => {
    const wrapper = mount(SyncView, {
      global: {
        components: { AppButton, AppPageHeader }
      }
    })
    expect(wrapper.text()).toContain("Sincronización de Datos")
    expect(wrapper.text()).toContain("Usuarios")
    expect(wrapper.text()).toContain("Autorizadores")
    expect(wrapper.text()).toContain("Permisos")
  })

  it("rechaza archivos con extension distinta a .xlsx sin invocar a validateFile", async () => {
    const wrapper = mount(SyncView, {
      global: {
        components: { AppButton, AppPageHeader }
      }
    })
    const invalidFile = new File(["data"], "data.csv", { type: "text/csv" })
    const input = wrapper.find("input[type=\"file\"]")

    Object.defineProperty(input.element, "files", { value: [invalidFile] })
    await input.trigger("change")

    expect(syncApi.validateFile).not.toHaveBeenCalled()
    expect(wrapper.text()).toContain("El archivo debe ser .xlsx")
  })

  it("al seleccionar un .xlsx valido invoca validateFile y renderiza el preview", async () => {
    const mockPreview: syncApi.SyncPreview = {
      kind: "usuarios",
      totalRows: 10,
      newCount: 8,
      duplicateCount: 2,
      errorCount: 0,
      rows: []
    }
    vi.mocked(syncApi.validateFile).mockResolvedValueOnce(mockPreview)

    const wrapper = mount(SyncView, {
      global: {
        components: { AppButton, AppPageHeader }
      }
    })
    const validFile = new File(["data"], "usuarios.xlsx", {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    })
    const input = wrapper.find("input[type=\"file\"]")

    Object.defineProperty(input.element, "files", { value: [validFile] })
    await input.trigger("change")
    await flushPromises()

    expect(syncApi.validateFile).toHaveBeenCalledWith("usuarios", validFile)
    expect(wrapper.text()).toContain("8 nuevos")
    expect(wrapper.text()).toContain("2 duplicados")
  })

  it("interrumpe la importacion si confirmImportWithWarnings devuelve false (usuario cancela)", async () => {
    const mockPreview: syncApi.SyncPreview = {
      kind: "usuarios",
      totalRows: 1,
      newCount: 1,
      duplicateCount: 0,
      errorCount: 0,
      rows: []
    }
    vi.mocked(syncApi.validateFile).mockResolvedValueOnce(mockPreview)
    vi.mocked(warnings.confirmImportWithWarnings).mockResolvedValueOnce(false)

    const wrapper = mount(SyncView, {
      global: {
        components: { AppButton, AppPageHeader }
      }
    })
    const validFile = new File(["data"], "usuarios.xlsx", {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    })
    const input = wrapper.find("input[type=\"file\"]")

    Object.defineProperty(input.element, "files", { value: [validFile] })
    await input.trigger("change")

    await flushPromises()

    const buttons = wrapper.findAllComponents(AppButton)
    const importBtn = buttons.find(b => b.text().includes("Importar") || b.text().includes("Importando"))
    expect(importBtn).toBeDefined()

    await importBtn!.trigger("click")
    await flushPromises()

    expect(warnings.confirmImportWithWarnings).toHaveBeenCalledWith("usuarios", mockPreview)
    expect(syncApi.runImport).not.toHaveBeenCalled()
  })
})
