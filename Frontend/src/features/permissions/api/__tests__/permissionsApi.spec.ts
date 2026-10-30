import { describe, it, expect, beforeEach, vi } from "vitest"
import {
  fetchPermissionConfig,
  fetchPermission,
  fetchEditConfig,
  fetchAuthorizeConfig,
  cancelPermission
} from "../permissions"
import { http } from "@/shared/api/client"
import { createMockCreateConfig, createMockPermissionDetail } from "../../__tests__/factories"

vi.mock("@/shared/api/client", () => ({
  http: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn()
  }
}))

describe("permissions.ts API Client", () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it("fetchPermissionConfig invoca /api/Permissions/create-config y retorna datos", async () => {
    const mockConfig = createMockCreateConfig()
    vi.mocked(http.get).mockResolvedValueOnce({ data: mockConfig } as any)

    const result = await fetchPermissionConfig()
    expect(http.get).toHaveBeenCalledWith("/api/Permissions/create-config")
    expect(result).toEqual(mockConfig)
    expect(result.canChooseRequester).toBe(false)
  })

  it("fetchPermission invoca /api/Permissions/{id} con el ID correcto", async () => {
    const mockDetail = createMockPermissionDetail({ id: 456 })
    vi.mocked(http.get).mockResolvedValueOnce({ data: mockDetail } as any)

    const result = await fetchPermission(456)
    expect(http.get).toHaveBeenCalledWith("/api/Permissions/456")
    expect(result.id).toBe(456)
  })

  it("fetchEditConfig invoca /api/Permissions/{id}/edit-config", async () => {
    const mockEditConfig = { id: 789, canEdit: true, blockReason: null }
    vi.mocked(http.get).mockResolvedValueOnce({ data: mockEditConfig } as any)

    const result = await fetchEditConfig(789)
    expect(http.get).toHaveBeenCalledWith("/api/Permissions/789/edit-config")
    expect(result.canEdit).toBe(true)
  })

  it("fetchAuthorizeConfig invoca /api/Permissions/{id}/authorize-config", async () => {
    const mockAuthConfig = { id: 123, canAuthorize: true, canChangeWithPay: true }
    vi.mocked(http.get).mockResolvedValueOnce({ data: mockAuthConfig } as any)

    const result = await fetchAuthorizeConfig(123)
    expect(http.get).toHaveBeenCalledWith("/api/Permissions/123/authorize-config")
    expect(result.canAuthorize).toBe(true)
  })

  it("cancelPermission ejecuta DELETE a /api/Permissions/{id}", async () => {
    vi.mocked(http.delete).mockResolvedValueOnce({ data: {} } as any)

    await cancelPermission(999)
    expect(http.put).toHaveBeenCalledWith("/api/Permissions/999/cancel")
  })
})
