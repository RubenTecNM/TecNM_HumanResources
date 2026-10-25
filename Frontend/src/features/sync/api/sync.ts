import { http as client } from './client'

export interface SyncRowResult {
  row: number
  status: 'nuevo' | 'duplicado' | 'error'
  message: string
  detail?: string
}

export interface SyncPreview {
  kind: string
  totalRows: number
  newCount: number
  duplicateCount: number
  errorCount: number
  rows: SyncRowResult[]
}

export interface SyncImportSummary {
  createdUsers?: number
  promotedAuthorizers?: number
  createdCutoffs?: number
  createdPermissions?: number
  duplicates: number
  errors: SyncRowResult[]
}

export type SyncKind = 'usuarios' | 'permisos' | 'autorizadores'

export async function downloadTemplate(kind: SyncKind) {
  const res = await client.get(`/api/Sync/template/${kind}`, { responseType: 'blob' })
  const url = URL.createObjectURL(res.data as Blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `plantilla_${kind}.xlsx`
  a.click()
  URL.revokeObjectURL(url)
}

export async function validateFile(kind: SyncKind, file: File) {
  const form = new FormData()
  form.append('file', file)
  const res = await client.post<SyncPreview>(`/api/Sync/validate/${kind}`, form)
  return res.data
}

export async function runImport(kind: SyncKind, file: File) {
  const form = new FormData()
  form.append('file', file)
  const res = await client.post<SyncImportSummary>(`/api/Sync/import/${kind}`, form)
  return res.data
}
