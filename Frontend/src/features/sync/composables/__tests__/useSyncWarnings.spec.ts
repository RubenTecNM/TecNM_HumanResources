import { describe, it, expect, vi, beforeEach } from "vitest"
import { analyzeSyncPreview, confirmImportWithWarnings } from "../useSyncWarnings"
import type { SyncPreview } from "../../api/sync"
import Swal from "sweetalert2"

vi.mock("sweetalert2", () => ({
  default: {
    fire: vi.fn()
  }
}))

describe("useSyncWarnings Composable", () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe("analyzeSyncPreview", () => {
    it("detecta 100% duplicados (idempotencia) cuando no hay nuevos ni errores", () => {
      const preview: SyncPreview = {
        kind: "usuarios",
        totalRows: 15,
        newCount: 0,
        duplicateCount: 15,
        errorCount: 0,
        rows: []
      }
      const result = analyzeSyncPreview("usuarios", preview)
      expect(result.isAllDuplicates).toBe(true)
      expect(result.hasErrors).toBe(false)
      expect(result.hasHighVolume).toBe(false)
      expect(result.summaryMessage).toContain("15")
    })

    it("detecta asignacion de roles privilegiados (Administrador / Autorizador)", () => {
      const preview: SyncPreview = {
        kind: "usuarios",
        totalRows: 2,
        newCount: 2,
        duplicateCount: 0,
        errorCount: 0,
        rows: [
          { row: 2, status: "nuevo", message: "Carlos Lopez", detail: "Rol: Administrador" },
          { row: 3, status: "nuevo", message: "Ana Maria", detail: "Rol: Docente" }
        ]
      }
      const result = analyzeSyncPreview("usuarios", preview)
      expect(result.hasPrivilegedUsers).toBe(true)
      expect(result.privilegedCount).toBe(1)
    })

    it("detecta alto volumen de registros nuevos (> 50)", () => {
      const preview: SyncPreview = {
        kind: "usuarios",
        totalRows: 70,
        newCount: 65,
        duplicateCount: 5,
        errorCount: 0,
        rows: []
      }
      const result = analyzeSyncPreview("usuarios", preview)
      expect(result.hasHighVolume).toBe(true)
    })
  })

  describe("confirmImportWithWarnings", () => {
    it("muestra mensaje informativo y aborta (retorna false) si todo es duplicado", async () => {
      const preview: SyncPreview = {
        kind: "usuarios",
        totalRows: 10,
        newCount: 0,
        duplicateCount: 10,
        errorCount: 0,
        rows: []
      }
      vi.mocked(Swal.fire).mockResolvedValueOnce({ isConfirmed: true } as any)

      const confirmed = await confirmImportWithWarnings("usuarios", preview)
      expect(confirmed).toBe(false)
      expect(Swal.fire).toHaveBeenCalledWith(
        expect.objectContaining({
          title: "Sin Cambios Requeridos",
          icon: "info"
        })
      )
    })

    it("muestra alerta de privilegios y retorna false si el usuario cancela", async () => {
      const preview: SyncPreview = {
        kind: "usuarios",
        totalRows: 1,
        newCount: 1,
        duplicateCount: 0,
        errorCount: 0,
        rows: [{ row: 2, status: "nuevo", message: "Super Admin", detail: "Tipo: Administrador" }]
      }
      vi.mocked(Swal.fire).mockResolvedValueOnce({ isConfirmed: false } as any)

      const confirmed = await confirmImportWithWarnings("usuarios", preview)
      expect(confirmed).toBe(false)
      expect(Swal.fire).toHaveBeenCalledWith(
        expect.objectContaining({
          icon: "warning",
          title: expect.stringContaining("Privilegios")
        })
      )
    })

    it("muestra advertencia de corte vigente/cerrado para permisos y retorna true al confirmar", async () => {
      const preview: SyncPreview = {
        kind: "permisos",
        totalRows: 5,
        newCount: 5,
        duplicateCount: 0,
        errorCount: 0,
        rows: []
      }
      vi.mocked(Swal.fire).mockResolvedValueOnce({ isConfirmed: true } as any)

      const confirmed = await confirmImportWithWarnings("permisos", preview)
      expect(confirmed).toBe(true)
      expect(Swal.fire).toHaveBeenCalledWith(
        expect.objectContaining({
          title: expect.stringContaining("Permisos"),
          html: expect.stringContaining("vigente")
        })
      )
    })
  })
})
