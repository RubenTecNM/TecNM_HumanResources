import type {
  PermissionTypeCreateConfigDto,
  PermissionCreateConfigDto,
  PermissionDetail,
  PermissionListPage
} from "../api/permissions"

export function createMockFieldConfig(show = true, label = "Fecha", required = true) {
  return { show, label, required }
}

export function createMockPermissionType(overrides: Partial<PermissionTypeCreateConfigDto> = {}): PermissionTypeCreateConfigDto {
  return {
    id: 1,
    description: "Retardo",
    icon: "fa-clock-o",
    subtitle: "Entrada posterior a la hora límite",
    date1: createMockFieldConfig(true, "Fecha", true),
    date2: createMockFieldConfig(false, "Fecha fin", false),
    time1: createMockFieldConfig(true, "Hora de entrada", true),
    time2: createMockFieldConfig(false, "Hora de salida", false),
    ...overrides
  }
}

export function createMockCreateConfig(overrides: Partial<PermissionCreateConfigDto> = {}): PermissionCreateConfigDto {
  return {
    availableTypes: [createMockPermissionType()],
    canChooseRequester: false,
    activeCutoffId: 10,
    activeCutoffStartDate: "2026-10-01T00:00:00",
    activeCutoffEndDate: "2026-10-31T23:59:59",
    maxFileSizeMB: 5,
    allowedEvidenceExtensions: [".pdf", ".png", ".jpg"],
    ...overrides
  }
}

export function createMockPermissionDetail(overrides: Partial<PermissionDetail> = {}): PermissionDetail {
  return {
    id: 101,
    requester: "Ing. Roberto Gomez",
    requestingUserId: 5,
    type: "Retardo",
    permissionTypeId: 1,
    startDate: "2026-10-25T08:30:00",
    endDate: null,
    reason: "Retraso por transporte publico",
    permissionStatusTypeId: 1,
    statusDescription: "Pendiente",
    statusColor: "#f39c12",
    withPay: true,
    createdAt: "2026-10-25T08:35:00",
    updatedAt: null,
    evidenceUrl: "",
    shift: 1,
    shiftDescription: "Matutino",
    hoursInClassroom: null,
    hoursDischarge: null,
    ...overrides
  }
}
