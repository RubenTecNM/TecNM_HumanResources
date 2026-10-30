import { describe, it, expect, beforeEach, vi } from "vitest"
import { useTheme, THEME_OPTIONS } from "../useTheme"

describe("useTheme Composable", () => {
  beforeEach(() => {
    localStorage.clear()
    document.documentElement.removeAttribute("data-theme")
  })

  it("provee las opciones de tema predefinidas", () => {
    const { options } = useTheme()
    expect(options).toHaveLength(3)
    expect(options.map(o => o.id)).toEqual(["tecnm", "dark", "contrast"])
  })

  it("permite cambiar el tema a dark y actualiza el DOM y localStorage", () => {
    const { setTheme, currentTheme } = useTheme()
    setTheme("dark")

    expect(currentTheme.value.id).toBe("dark")
    expect(document.documentElement.getAttribute("data-theme")).toBe("dark")
    expect(localStorage.getItem("tecnm-theme")).toBe("dark")
    expect(document.documentElement.style.colorScheme).toBe("dark")
  })

  it("ignora temas inexistentes o no autorizados", () => {
    const { setTheme, currentTheme } = useTheme()
    setTheme("dark") // primero dark
    setTheme("invalid-theme-xyz") // intento invalido

    expect(currentTheme.value.id).toBe("dark")
    expect(document.documentElement.getAttribute("data-theme")).toBe("dark")
  })
})
