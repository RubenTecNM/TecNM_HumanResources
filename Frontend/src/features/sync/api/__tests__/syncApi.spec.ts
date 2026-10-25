import { describe, it, expect, vi, beforeEach } from "vitest"
import { validateFile, runImport } from "../sync"
import { http } from "@/shared/api/client"

vi.mock("@/shared/api/client", () => ({
  http: {
    get: vi.fn(),
    post: vi.fn()
  }
}))

describe("sync.ts API Client", () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it("validateFile envia un FormData con la clave file a /api/Sync/validate/{kind}", async () => {
    const mockPreview = {
      kind: "usuarios",
      totalRows: 2,
      newCount: 2,
      duplicateCount: 0,
      errorCount: 0,
      rows: []
    }
    vi.mocked(http.post).mockResolvedValueOnce({ data: mockPreview } as any)

    const dummyFile = new File(["fake-excel"], "test.xlsx", {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    })

    const result = await validateFile("usuarios", dummyFile)

    expect(http.post).toHaveBeenCalledWith(
      "/api/Sync/validate/usuarios",
      expect.any(FormData)
    )
    expect(result).toEqual(mockPreview)
  })

  it("runImport envia un FormData a /api/Sync/import/{kind} y devuelve el resumen", async () => {
    const mockSummary = {
      createdUsers: 5,
      duplicates: 1,
      errors: []
    }
    vi.mocked(http.post).mockResolvedValueOnce({ data: mockSummary } as any)

    const dummyFile = new File(["fake-excel"], "test.xlsx", {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    })

    const result = await runImport("usuarios", dummyFile)

    expect(http.post).toHaveBeenCalledWith(
      "/api/Sync/import/usuarios",
      expect.any(FormData)
    )
    expect(result.createdUsers).toBe(5)
  })
})
