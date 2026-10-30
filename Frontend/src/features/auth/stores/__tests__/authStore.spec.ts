import { describe, it, expect, beforeEach, vi } from "vitest"
import { setActivePinia, createPinia } from "pinia"
import { useAuthStore, USER_TYPE } from "../auth"
import { http } from "@/shared/api/client"

vi.mock("@/shared/api/client", () => ({
  http: {
    get: vi.fn(),
    post: vi.fn()
  }
}))

describe("authStore Pinia Store", () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe("Getters y Roles", () => {
    it("isAuthenticated es false cuando user es null", () => {
      const store = useAuthStore()
      expect(store.isAuthenticated).toBe(false)
      expect(store.isAdmin).toBe(false)
      expect(store.isAuthorizer).toBe(false)
      expect(store.isRH).toBe(false)
    })

    it("isAdmin es true unicamente cuando userType es 1 (Administrador)", () => {
      const store = useAuthStore()
      store.user = { id: 10, name: "Admin User", email: "admin@tecnm.mx", userType: USER_TYPE.ADMINISTRADOR }
      expect(store.isAuthenticated).toBe(true)
      expect(store.isAdmin).toBe(true)
      expect(store.isAuthorizer).toBe(false)
      expect(store.isRH).toBe(false)
      expect(store.canManageUsers).toBe(true)
    })

    it("isAuthorizer es true unicamente cuando userType es 2 (Autorizador)", () => {
      const store = useAuthStore()
      store.user = { id: 20, name: "Jefe Depto", email: "jefe@tecnm.mx", userType: USER_TYPE.AUTORIZADOR }
      expect(store.isAuthorizer).toBe(true)
      expect(store.isAdmin).toBe(false)
      expect(store.canManageUsers).toBe(false)
      expect(store.isAutorizadorOrAbove).toBe(true)
    })

    it("isRH es true unicamente cuando userType es 3 (Recursos Humanos)", () => {
      const store = useAuthStore()
      store.user = { id: 30, name: "Lic. RH", email: "rh@tecnm.mx", userType: USER_TYPE.RECURSOS_HUMANOS }
      expect(store.isRH).toBe(true)
      expect(store.canManageUsers).toBe(true)
      expect(store.isAdmin).toBe(false)
    })

    it("initial retorna la primera letra en mayuscula del nombre de usuario", () => {
      const store = useAuthStore()
      store.user = { id: 40, name: "carlos mendoza", email: "carlos@tecnm.mx", userType: 4 }
      expect(store.initial).toBe("C")
    })

    it("initial retorna U por defecto si el usuario no tiene nombre", () => {
      const store = useAuthStore()
      store.user = { id: 41, name: "", email: "anon@tecnm.mx", userType: 4 }
      expect(store.initial).toBe("U")
    })
  })

  describe("Acciones HTTP", () => {
    it("fetchCurrentUser asigna el usuario correctamente en respuesta 200", async () => {
      const store = useAuthStore()
      const mockUserData = { id: 99, name: "Profesor Test", email: "test@tecnm.mx", userType: 4 }
      vi.mocked(http.get).mockResolvedValueOnce({ data: mockUserData } as any)

      const result = await store.fetchCurrentUser()
      expect(http.get).toHaveBeenCalledWith("/Account/Me")
      expect(result).toEqual(mockUserData)
      expect(store.user).toEqual(mockUserData)
    })

    it("fetchCurrentUser limpia user a null si la peticion falla (401)", async () => {
      const store = useAuthStore()
      store.user = { id: 1, name: "Previo", email: "previo@tecnm.mx", userType: 1 }
      vi.mocked(http.get).mockRejectedValueOnce(new Error("Unauthorized"))

      const result = await store.fetchCurrentUser()
      expect(result).toBeNull()
      expect(store.user).toBeNull()
    })

    it("logout limpia el estado local del usuario", async () => {
      const store = useAuthStore()
      store.user = { id: 1, name: "User", email: "user@tecnm.mx", userType: 1 }
      vi.mocked(http.get).mockResolvedValueOnce({ data: "" } as any)
      vi.mocked(http.post).mockResolvedValueOnce({ data: {} } as any)

      await store.logout()
      expect(store.user).toBeNull()
    })
  })
})
