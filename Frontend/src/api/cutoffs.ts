import { http } from '@/api/client'

export interface CutoffListItem {
  id: number
  startDate: string
  endDate: string
  createdAt?: string
  updatedAt?: string
  cutoffStatusTypeId: number
  statusDescription: string
  statusColor: string
  createdBy: string
  permissionsCount: number
}

export interface CutoffListPage {
  items: CutoffListItem[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
}

export interface CutoffDetail {
  id: number
  startDate: string
  endDate: string
  cutoffStatusTypeId: number
  statusDescription: string
  createdBy?: string
  updatedBy?: string
  createdAt?: string
  updatedAt?: string
}

export interface CutoffKpis {
  totalPermissions: number
  withPay: number
  withoutPay: number
  averageDays: number
  pendingCount: number
  approvedCount: number
}

export interface CutoffAnalytics {
  cutoff: CutoffDetail
  kpis: CutoffKpis
  permissionsByArea: { name: string; value: number }[]
  permissionsByType: { name: string; value: number; color?: string }[]
  trend: { date: string; total: number; approved: number; rejected: number }[]
  topRequesters?: { name: string; area?: string; value: number }[]
}

export type CutoffExportFormat = 'excel' | 'csv' | 'json' | 'xml'

export async function downloadCutoffExport(id: number, format: CutoffExportFormat) {
  const res = await http.get<Blob>(`/api/Cutoffs/${id}/export`, {
    params: { format },
    responseType: 'blob'
  })
  const url = URL.createObjectURL(res.data as Blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `corte-${id}.${format}`
  document.body.appendChild(a)
  a.click()
  a.remove()
  URL.revokeObjectURL(url)
}

export const CUTOFF_STATUS = {
  OPEN: 1,
  CLOSED: 2
} as const

export function cutoffStatusLabel(id: number): string {
  switch (id) {
    case CUTOFF_STATUS.OPEN:        return 'Abierto'
    case CUTOFF_STATUS.CLOSED:      return 'Cerrado'
    default: return 'Desconocido'
  }
}

export function cutoffStatusVariant(id: number): 'approved' | 'rejected' | 'info' {
  switch (id) {
    case CUTOFF_STATUS.OPEN:        return 'approved'
    case CUTOFF_STATUS.CLOSED:      return 'rejected'
    default: return 'info'
  }
}

export async function fetchCutoffs(params: Record<string, unknown> = {}): Promise<CutoffListPage> {
  const res = await http.get<CutoffListPage>('/api/Cutoffs', { params })
  return res.data
}

export async function fetchCutoff(id: number): Promise<CutoffDetail> {
  const res = await http.get<CutoffDetail>(`/api/Cutoffs/${id}`)
  return res.data
}

export async function updateCutoff(id: number, payload: { startDate: string; endDate: string }): Promise<{ message: string }> {
  const res = await http.put<{ message: string }>(`/api/Cutoffs/${id}`, { id, startDate: payload.startDate, endDate: payload.endDate })
  return res.data
}

export async function fetchCutoffAnalytics(id: number): Promise<CutoffAnalytics> {
  const res = await http.get<CutoffAnalytics>(`/api/Cutoffs/${id}/analytics`)
  return res.data
}

export async function closeCutoff(id: number): Promise<{ message: string }> {
  const res = await http.put<{ message: string }>(`/api/Cutoffs/${id}/close`)
  return res.data
}

export async function inactivateCutoff(id: number): Promise<{ message: string }> {
  const res = await http.put<{ message: string }>(`/api/Cutoffs/${id}/inactivate`)
  return res.data
}
