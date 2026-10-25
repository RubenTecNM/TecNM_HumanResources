import { http } from "@/shared/api/client"'

export interface CatalogItem {
  id: number
  description?: string
  name?: string
  status?: boolean
}

export interface AreaItem {
  id: number
  description?: string
  name?: string
  status?: boolean
}

export interface PermissionTypeFieldConfig {
  show: boolean
  label: string
  required: boolean
}

export interface PermissionTypeCardConfig {
  icon: string
  subtitle: string
  date1: PermissionTypeFieldConfig
  date2: PermissionTypeFieldConfig
  time1: PermissionTypeFieldConfig
  time2: PermissionTypeFieldConfig
}

export interface PermissionTypeItem extends CatalogItem {
  config?: PermissionTypeCardConfig | null
}

export async function fetchPermissionTypes(): Promise<PermissionTypeItem[]> {
  const res = await http.get<PermissionTypeItem[]>('/api/PermissionTypes')
  return res.data
}

export async function fetchAreas(): Promise<AreaItem[]> {
  const res = await http.get<{ items: AreaItem[] }>('/api/Areas')
  return res.data.items ?? []
}

export async function fetchDepartments(): Promise<CatalogItem[]> {
  const res = await http.get<{ items: CatalogItem[] }>('/api/Departments')
  return res.data.items ?? []
}

export async function fetchPositions(): Promise<CatalogItem[]> {
  const res = await http.get<CatalogItem[]>('/api/Positions')
  return res.data
}

export async function fetchUsers(): Promise<CatalogItem[]> {
  const res = await http.get<CatalogItem[]>('/api/Users')
  return res.data
}

export interface UserSearchItem {
  id: number
  name: string
  email: string
  department?: string
}

// Buscador global de usuarios (endpoint cookie-auth, independiente de la API Key).
// Igual que el Select2 original: filtra por nombre/correo, solo activos, top 20.
export async function searchUsers(term: string): Promise<UserSearchItem[]> {
  if (!term.trim()) return []
  const res = await http.get<UserSearchItem[]>('/api/Users/search', { params: { term } })
  return res.data
}

export type { CatalogItem as CatalogItemDTO, AreaItem as AreaDTO }