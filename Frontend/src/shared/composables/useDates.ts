// Utility functions ported from legacy wwwroot/js (site.js)
// Standard: Format DD/NombreMes/YYYY

const MONTHS_ES = [
  'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
  'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
]

export function formatTecNMDate(iso?: string | Date | null): string {
  if (!iso) return '—'
  const date = iso instanceof Date ? iso : new Date(iso)
  if (isNaN(date.getTime())) return String(iso)
  const day = String(date.getDate()).padStart(2, '0')
  return `${day}/${MONTHS_ES[date.getMonth()]}/${date.getFullYear()}`
}

export function formatTecNMDateTime(iso?: string | Date | null): string {
  if (!iso) return '—'
  const date = iso instanceof Date ? iso : new Date(iso)
  if (isNaN(date.getTime())) return String(iso)
  const day = String(date.getDate()).padStart(2, '0')
  const hours = String(date.getHours()).padStart(2, '0')
  const minutes = String(date.getMinutes()).padStart(2, '0')
  return `${day}/${MONTHS_ES[date.getMonth()]}/${date.getFullYear()} ${hours}:${minutes}`
}

export function toDateInputValue(date: Date | string | null): string {
  if (!date) return ''
  const d = date instanceof Date ? date : new Date(date)
  if (isNaN(d.getTime())) return ''
  const month = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${d.getFullYear()}-${month}-${day}`
}

export function toTimeInputValue(date: Date | string | null): string {
  if (!date) return ''
  const d = date instanceof Date ? date : new Date(date)
  if (isNaN(d.getTime())) return ''
  return String(d.getHours()).padStart(2, '0') + ':' + String(d.getMinutes()).padStart(2, '0')
}

// Combines YYYY-MM-DD + HH:mm into local "YYYY-MM-DDTHH:mm:ss" (no TZ shift)
export function combineDateTimeLocal(
  dateOnly: string | null | undefined,
  timeOnly: string | null | undefined,
  endOfDay = false
): string | null {
  if (!dateOnly) return null
  let time = timeOnly && timeOnly.trim() ? timeOnly.trim() : endOfDay ? '23:59:59' : '00:00:00'
  if (time.split(':').length === 2) time += ':00'
  return `${dateOnly}T${time}`
}

export function calculateDays(start: string | null, end: string | null): number | null {
  if (!start || !end) return null
  const from = new Date(start)
  const to = new Date(end)
  if (isNaN(from.getTime()) || isNaN(to.getTime())) return null
  return Math.floor((to.getTime() - from.getTime()) / 86400000) + 1
}