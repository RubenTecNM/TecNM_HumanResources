import { http } from '@/api/client'

export interface PermissionListItem {
  id: number
  requester: string
  requestingUserId?: number
  type: string
  permissionTypeId: number
  startDate: string | null
  endDate: string | null
  reason: string
  permissionStatusTypeId: number
  statusDescription?: string
  statusColor?: string
  withPay: boolean
  createdAt: string | null
  updatedAt: string | null
  shift?: number | null
  shiftDescription?: string | null
  hoursInClassroom?: number | null
  hoursDischarge?: number | null
}

export interface PermissionListPage {
  items: PermissionListItem[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  firstItemOnPage: number
  lastItemOnPage: number
}

export interface PermissionDetail {
  id: number
  requester: string
  requestingUserId?: number
  type: string
  permissionTypeId: number
  startDate: string | null
  endDate: string | null
  reason: string
  evidence: string | null
  status: number
  withPay: boolean
  createdAt: string | null
  cutoff: string | null
  days?: number | null
  authorizedBy?: string | null
  authorizedAt?: string | null
  authorizationComment?: string | null
  shift?: number | null
  shiftDescription?: string | null
  hoursInClassroom?: number | null
  hoursDischarge?: number | null
}

export interface PermissionCreatePayload {
  reason: string
  startDate: string
  endDate?: string
  permissionTypeId: number
  requestingUserId?: number
  withPay: boolean
  evidence?: File | null
  confirmed?: boolean
  clearEvidence?: boolean
  hoursInClassroom?: number | null
  hoursDischarge?: number | null
}

export interface PermissionSubmitResult {
  id?: number
  success?: boolean
  showConfirmation?: boolean
  title?: string
  message?: string
  icon?: string
  confirmButtonText?: string
}

export const PERMISSION_STATUS = {
  PENDING: 1,
  APPROVED: 2,
  REJECTED: 4,
  CANCELLED: 5
} as const

export function statusLabel(id: number | undefined): string {
  switch (id) {
    case PERMISSION_STATUS.PENDING:    return 'Pendiente por Revisar'
    case PERMISSION_STATUS.APPROVED:   return 'Revisado / Aprobado'
    case PERMISSION_STATUS.REJECTED:   return 'Revisado / Rechazado'
    case PERMISSION_STATUS.CANCELLED:  return 'Cancelado por el Solicitante'
    default: return 'Desconocido'
  }
}

export function statusVariant(id: number | undefined): 'pending' | 'approved' | 'rejected' | 'info' {
  switch (id) {
    case PERMISSION_STATUS.PENDING:    return 'pending'
    case PERMISSION_STATUS.APPROVED:   return 'approved'
    case PERMISSION_STATUS.REJECTED:   return 'rejected'
    case PERMISSION_STATUS.CANCELLED:  return 'info'
    default: return 'info'
  }
}

export async function fetchPermissions(params: Record<string, unknown> = {}): Promise<PermissionListPage> {
  const res = await http.get<PermissionListPage>('/api/Permissions', { params })
  return res.data
}

export async function fetchPermission(id: number): Promise<PermissionDetail> {
  const res = await http.get<PermissionDetail>(`/api/Permissions/${id}`)
  return res.data
}

// POST is multipart/form-data so optional evidence (PDF) can be attached.
export async function createPermission(payload: PermissionCreatePayload): Promise<PermissionSubmitResult> {
  const form = buildPermissionForm(payload)
  const res = await http.post<PermissionSubmitResult & { id: number }>('/api/Permissions', form)
  return res.data
}

export async function updatePermission(id: number, payload: PermissionCreatePayload): Promise<PermissionSubmitResult> {
  const form = buildPermissionForm(payload)
  form.append('id', String(id))
  const res = await http.put<PermissionSubmitResult>(`/api/Permissions/${id}`, form)
  return res.data
}

export async function cancelPermission(id: number): Promise<void> {
  // Cancelación vía PUT: el solicitante cancela un permiso Pending dentro de un periodo vigente
  await http.put(`/api/Permissions/${id}/cancel`)
}

export interface PermissionAuthorizePayload {
  approved: boolean
  withPay: boolean
  authorizationComment?: string
}

export async function authorizePermission(id: number, payload: PermissionAuthorizePayload): Promise<PermissionSubmitResult> {
  const res = await http.post<PermissionSubmitResult>(`/api/Permissions/${id}/authorize`, payload)
  return res.data
}

function buildPermissionForm(payload: PermissionCreatePayload): FormData {
  const form = new FormData()
  form.append('reason', payload.reason)
  form.append('startDate', payload.startDate) // "YYYY-MM-DDTHH:mm:ss" local, sin TZ
  if (payload.endDate) form.append('endDate', payload.endDate)
  form.append('permissionTypeId', String(payload.permissionTypeId))
  form.append('withPay', payload.withPay ? 'true' : 'false')
  if (payload.requestingUserId) form.append('requestingUserId', String(payload.requestingUserId))
  if (payload.evidence) form.append('evidenceFile', payload.evidence)
  if (payload.confirmed !== undefined) form.append('confirmed', payload.confirmed ? 'true' : 'false')
  if (payload.clearEvidence) form.append('clearEvidence', 'true')
  if (payload.hoursInClassroom != null) form.append('hoursInClassroom', String(payload.hoursInClassroom))
  if (payload.hoursDischarge != null) form.append('hoursDischarge', String(payload.hoursDischarge))
  return form
}