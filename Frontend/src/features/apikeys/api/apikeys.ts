import { http } from "@/shared/api/client"

export interface ApiKeyItem {
  id: number
  key: string
  status: boolean
  user: string
  permissionsCount: number
  createdAt?: string
  updatedAt?: string
}

export interface ApiKeyListPage {
  items: ApiKeyItem[]
  totalItems: number
  totalPages: number
  page: number
  pageSize: number
}

export interface ApiKeyRule {
  tableId: number
  tableName: string
  apiPermissionId: number
  permissionName: string
}

export interface ApiKeyDetail extends ApiKeyItem {
  permissions: ApiKeyRule[]
}

export interface ApiTableItem {
  id: number
  description: string
}

export interface ApiPermissionItem {
  id: number
  description: string
}

export interface PermissionAssignment {
  tableId: number
  permissionId: number
}

export async function fetchApiKeys(params: Record<string, unknown> = {}): Promise<ApiKeyListPage> {
  const res = await http.get<ApiKeyListPage>('/api/ApiKeysApi', { params })
  return res.data
}

export async function fetchApiKey(id: number): Promise<ApiKeyDetail> {
  const res = await http.get<ApiKeyDetail>(`/api/ApiKeysApi/${id}`)
  return res.data
}

export async function createApiKey(userId: number, permissions: PermissionAssignment[] = []): Promise<{ key: string }> {
  const res = await http.post<{ key: string }>('/api/ApiKeysApi', { userId, permissions })
  return res.data
}

export async function updateApiKeyOwner(id: number, userId: number): Promise<{ message: string }> {
  const res = await http.put<{ message: string }>(`/api/ApiKeysApi/${id}`, { userId })
  return res.data
}

export async function updateApiKeyPermissions(id: number, userId: number, permissions: PermissionAssignment[]): Promise<{ message: string }> {
  const res = await http.put<{ message: string }>(`/api/ApiKeysApi/${id}/permissions`, { userId, permissions })
  return res.data
}

export async function deactivateApiKey(id: number): Promise<{ message: string }> {
  const res = await http.put<{ message: string }>(`/api/ApiKeysApi/${id}/deactivate`)
  return res.data
}

export async function fetchDatabaseTables(): Promise<ApiTableItem[]> {
  const res = await http.get<ApiTableItem[]>('/api/DatabaseTables')
  return res.data
}

export async function fetchApiPermissions(): Promise<ApiPermissionItem[]> {
  const res = await http.get<ApiPermissionItem[]>('/api/ApiPermissions')
  return res.data
}
