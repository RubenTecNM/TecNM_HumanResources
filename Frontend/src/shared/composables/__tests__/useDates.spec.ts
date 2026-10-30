import { describe, it, expect } from "vitest"
import {
  formatTecNMDate,
  formatTecNMDateTime,
  toDateInputValue,
  toTimeInputValue,
  combineDateTimeLocal,
  calculateDays
} from "../useDates"

describe("useDates Composable", () => {
  describe("formatTecNMDate", () => {
    it("retorna — cuando se le pasa null o undefined", () => {
      expect(formatTecNMDate(null)).toBe("—")
      expect(formatTecNMDate(undefined)).toBe("—")
    })

    it("formatea correctamente una fecha en formato DD/NombreMes/YYYY", () => {
      const date = new Date(2026, 9, 25) // 25 de Octubre 2026 (mes 0-indexed: 9 = Octubre)
      expect(formatTecNMDate(date)).toBe("25/Octubre/2026")
    })

    it("antepone cero al dia si es menor a 10", () => {
      const date = new Date(2026, 4, 5) // 05 de Mayo 2026
      expect(formatTecNMDate(date)).toBe("05/Mayo/2026")
    })
  })

  describe("formatTecNMDateTime", () => {
    it("retorna — en nulo", () => {
      expect(formatTecNMDateTime(null)).toBe("—")
    })

    it("formatea correctamente fecha con hora HH:mm", () => {
      const date = new Date(2026, 9, 25, 8, 30)
      expect(formatTecNMDateTime(date)).toBe("25/Octubre/2026 08:30")
    })
  })

  describe("toDateInputValue", () => {
    it("retorna string vacio si la fecha es nula", () => {
      expect(toDateInputValue(null)).toBe("")
    })

    it("convierte fecha a formato YYYY-MM-DD para input HTML date", () => {
      const date = new Date(2026, 0, 15) // 15 de Enero 2026
      expect(toDateInputValue(date)).toBe("2026-01-15")
    })
  })

  describe("toTimeInputValue", () => {
    it("retorna string vacio si la fecha es nula", () => {
      expect(toTimeInputValue(null)).toBe("")
    })

    it("convierte fecha a formato HH:mm para input HTML time", () => {
      const date = new Date(2026, 0, 15, 14, 45)
      expect(toTimeInputValue(date)).toBe("14:45")
    })
  })

  describe("combineDateTimeLocal", () => {
    it("retorna null si dateOnly no esta presente", () => {
      expect(combineDateTimeLocal(null, "08:00")).toBeNull()
    })

    it("combina fecha y hora sin desfase de zona horaria", () => {
      const result = combineDateTimeLocal("2026-10-25", "09:30")
      expect(result).toBe("2026-10-25T09:30:00")
    })

    it("usa 23:59:59 si endOfDay es true y no hay timeOnly", () => {
      const result = combineDateTimeLocal("2026-10-25", null, true)
      expect(result).toBe("2026-10-25T23:59:59")
    })

    it("usa 00:00:00 si endOfDay es false y no hay timeOnly", () => {
      const result = combineDateTimeLocal("2026-10-25", null, false)
      expect(result).toBe("2026-10-25T00:00:00")
    })
  })

  describe("calculateDays", () => {
    it("retorna null si alguna fecha es nula o vacia", () => {
      expect(calculateDays(null, "2026-10-25")).toBeNull()
      expect(calculateDays("2026-10-25", null)).toBeNull()
    })

    it("retorna 1 si inicio y fin son el mismo dia", () => {
      expect(calculateDays("2026-10-25T08:00:00", "2026-10-25T16:00:00")).toBe(1)
    })

    it("calcula correctamente los dias transcurridos", () => {
      expect(calculateDays("2026-10-19T00:00:00", "2026-10-23T00:00:00")).toBe(5)
    })
  })
})
