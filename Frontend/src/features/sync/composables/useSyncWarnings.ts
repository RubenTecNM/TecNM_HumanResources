import Swal from "sweetalert2"
import type { SyncKind, SyncPreview } from "../api/sync"

export interface SyncRiskAnalysis {
  isAllDuplicates: boolean
  hasPrivilegedUsers: boolean
  privilegedCount: number
  hasHighVolume: boolean
  hasErrors: boolean
  summaryMessage: string
}

export function analyzeSyncPreview(kind: SyncKind, preview: SyncPreview): SyncRiskAnalysis {
  const isAllDuplicates = preview.newCount === 0 && preview.duplicateCount > 0 && preview.errorCount === 0
  const hasErrors = preview.errorCount > 0
  const hasHighVolume = preview.newCount > 50

  let privilegedCount = 0
  if (kind === "usuarios" || kind === "autorizadores") {
    for (const r of preview.rows) {
      const text = (r.message + " " + (r.detail || "")).toLowerCase()
      if (text.includes("administrador") || text.includes("autorizador")) {
        privilegedCount++
      }
    }
  }

  const hasPrivilegedUsers = privilegedCount > 0

  let summaryMessage = ""
  if (isAllDuplicates) {
    summaryMessage = `Todos los registros (${preview.duplicateCount}) ya existen con la misma definición. No se registrarán modificaciones.`
  } else if (hasPrivilegedUsers) {
    summaryMessage = `Se detectó la asignación de roles con privilegios altos (Administrador/Autorizador) en ${privilegedCount} registro(s).`
  } else if (hasHighVolume) {
    summaryMessage = `Estás a punto de insertar un volumen alto de registros nuevos (${preview.newCount}).`
  }

  return {
    isAllDuplicates,
    hasPrivilegedUsers,
    privilegedCount,
    hasHighVolume,
    hasErrors,
    summaryMessage
  }
}

export async function confirmImportWithWarnings(
  kind: SyncKind,
  preview: SyncPreview
): Promise<boolean> {
  const analysis = analyzeSyncPreview(kind, preview)

  // Caso 1: 100% Duplicados idénticos (Idempotencia)
  if (analysis.isAllDuplicates) {
    await Swal.fire({
      title: "Sin Cambios Requeridos",
      html: `
        <div style="text-align: left; font-size: 0.95rem;">
          <p><strong>100% de los registros ya existen</strong> en la base de datos con idéntica definición.</p>
          <div style="background:#eef2ff; border-left:4px solid #6366f1; padding:10px; margin-top:8px;">
            Duplicados detectados: <strong>${preview.duplicateCount}</strong><br>
            Nuevos registros: <strong>0</strong>
          </div>
          <p style="margin-top:10px; color:#475569;">No se requiere ejecutar la importación.</p>
        </div>
      `,
      icon: "info",
      confirmButtonText: "Entendido"
    })
    return false
  }

  // Caso 2: Alerta Crítica - Asignación de Roles Administrativos / Autorizadores
  if (analysis.hasPrivilegedUsers) {
    const result = await Swal.fire({
      title: "⚠️ Privilegios Elevados Detectados",
      html: `
        <div style="text-align: left;">
          <p>El archivo incluye <strong>${analysis.privilegedCount} usuario(s)</strong> con permisos de <strong>Administrador o Autorizador</strong>.</p>
          <p style="color:#b91c1c; font-size:0.9rem;">
            <em>Otorgar estos roles permite administrar usuarios, consultar auditorías y autorizar solicitudes de permisos institucionales.</em>
          </p>
          <p>¿Deseas confirmar la carga con esta elevación de privilegios?</p>
        </div>
      `,
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#b91c1c",
      cancelButtonColor: "#64748b",
      confirmButtonText: "Sí, autorizar carga",
      cancelButtonText: "Cancelar"
    })
    return result.isConfirmed
  }

  // Caso 3: Permisos en Corte Vigente (Regla de negocio)
  if (kind === "permisos") {
    const result = await Swal.fire({
      title: "📅 Importación de Permisos",
      html: `
        <div style="text-align: left;">
          <p>Se registrarán <strong>${preview.newCount}</strong> permisos nuevos (${preview.duplicateCount} duplicados omitidos).</p>
          <div style="background:#fffbeb; border-left:4px solid #f59e0b; padding:10px; margin:10px 0; font-size:0.9rem;">
            <strong>Nota de Auditoría:</strong><br>
            • Si corresponden a un corte <strong>cerrado</strong>: se registran aprobados históricamente.<br>
            • Si corresponden al corte <strong>vigente</strong>: permanecerán <strong>Pendientes</strong> para revisión de su jefatura.
          </div>
          <p>¿Confirmar la inserción en la base de datos?</p>
        </div>
      `,
      icon: "question",
      showCancelButton: true,
      confirmButtonColor: "#1B396A",
      cancelButtonColor: "#64748b",
      confirmButtonText: "Sí, importar permisos",
      cancelButtonText: "Cancelar"
    })
    return result.isConfirmed
  }

  // Caso 4: Errores parciales detectados
  if (analysis.hasErrors) {
    const result = await Swal.fire({
      title: "Carga Parcial con Errores",
      html: `
        <div style="text-align: left;">
          <p>Se detectaron <strong>${preview.errorCount} filas con error</strong> que serán omitidas.</p>
          <p>Se importarán únicamente los <strong>${preview.newCount} registros válidos</strong>.</p>
          <p>¿Deseas proceder con la importación parcial?</p>
        </div>
      `,
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#1B396A",
      cancelButtonColor: "#64748b",
      confirmButtonText: "Continuar con registros válidos",
      cancelButtonText: "Cancelar"
    })
    return result.isConfirmed
  }

  // Caso 5: Volumen alto o inserción estándar
  const title = analysis.hasHighVolume ? "⚠️ Carga Masiva de Alto Volumen" : "¿Confirmar Importación?"
  const text = analysis.hasHighVolume
    ? `Estás a punto de insertar ${preview.newCount} registros nuevos. ¿Estás seguro de continuar?`
    : `Se insertarán ${preview.newCount} registros nuevos (${preview.duplicateCount} duplicados omitidos).`

  const result = await Swal.fire({
    title,
    text,
    icon: analysis.hasHighVolume ? "warning" : "question",
    showCancelButton: true,
    confirmButtonColor: "#1B396A",
    cancelButtonColor: "#64748b",
    confirmButtonText: "Sí, importar",
    cancelButtonText: "Cancelar"
  })

  return result.isConfirmed
}
