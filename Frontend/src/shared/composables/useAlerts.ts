// Alertas y confirmaciones portadas de wwwroot/js/site.js
// (handleAlertResponse / handleActionWithConfirmation / showToast + SweetAlert2)

import Swal, { type SweetAlertIcon } from 'sweetalert2'

export interface AlertResponse {
  success?: boolean
  Success?: boolean
  title?: string
  Title?: string
  message?: string
  Message?: string
  icon?: SweetAlertIcon | string
  Icon?: SweetAlertIcon | string
  redirectUrl?: string
  RedirectUrl?: string
  errors?: Record<string, string[]>
  Errors?: Record<string, string[]>
  showConfirmation?: boolean
  ShowConfirmation?: boolean
  confirmButtonText?: string
  ConfirmButtonText?: string
  cancelButtonText?: string
  CancelButtonText?: string
}

export type ToastType = 'success' | 'info' | 'warning' | 'error'

export function showToast(message: string, type: ToastType = 'info') {
  const Toast = Swal.mixin({
    toast: true,
    position: 'top-end',
    showConfirmButton: false,
    timer: 3000,
    timerProgressBar: true,
    didOpen: (toast) => {
      toast.addEventListener('mouseenter', Swal.stopTimer)
      toast.addEventListener('mouseleave', Swal.resumeTimer)
    }
  })
  void Toast.fire({ icon: type, title: message })
}

// Renders field errors onto an element map { fieldName -> error string[] }
export function fieldErrorsOf(response: AlertResponse): Record<string, string[]> {
  const errors = (response.errors || response.Errors) as Record<string, string[]> | undefined
  return errors ?? {}
}

export async function handleAlertResponse(response: AlertResponse | null | undefined): Promise<void> {
  if (!response || typeof response !== 'object') {
    await Swal.fire({
      title: 'Error Desconocido',
      text: 'El servidor devolvió un formato de respuesta inesperado.',
      icon: 'error'
    })
    return
  }

  const isSuccess = response.success === true || response.Success === true
  const icon = (response.icon || response.Icon || (isSuccess ? 'success' : 'error')) as SweetAlertIcon
  const title = response.title || response.Title || (isSuccess ? '¡Éxito!' : 'Error')
  const text = response.message || response.Message || (isSuccess ? 'Operación completada exitosamente.' : 'Ha ocurrido un error.')
  const redirectUrl = response.redirectUrl || response.RedirectUrl
  const errors = fieldErrorsOf(response)

  const hasFieldErrors = !isSuccess && Object.keys(errors).length > 0

  if (hasFieldErrors) {
    const lines = Object.entries(errors)
      .map(([, msgs]) => msgs.join('<br>'))
      .join('<br>')
    await Swal.fire({
      title,
      html: `<div class="tecnm-alert tecnm-alert-danger">${lines}</div>`,
      icon,
      confirmButtonText: 'Aceptar'
    })
    return
  }

  await Swal.fire({
    title,
    text,
    icon,
    confirmButtonText: 'Aceptar'
  }).then(() => {
    if (isSuccess && redirectUrl) {
      console.log(`Redireccionando a: ${redirectUrl}`)
      window.location.href = redirectUrl
    } else if (isSuccess && !redirectUrl) {
      window.location.reload()
    }
  })
}

// Confirmation flow (showConfirmation DTO) then run a callback with { confirmed: true }
export async function confirmOrExecute(
  response: AlertResponse,
  resubmit: (confirmed: boolean) => Promise<AlertResponse | void> | AlertResponse | void
): Promise<unknown> {
  if (response.showConfirmation || response.ShowConfirmation) {
    const messageHtml = (response.message || response.Message || '')
      .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')

    const swalResult = await Swal.fire({
      title: response.title || response.Title || '¿Estás seguro?',
      html: messageHtml,
      icon: (response.icon || response.Icon || 'warning') as SweetAlertIcon,
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: response.confirmButtonText || response.ConfirmButtonText || 'Sí, continuar',
      cancelButtonText: response.cancelButtonText || response.CancelButtonText || 'Cancelar'
    })

    if (swalResult.isConfirmed) {
      const finalResult = await resubmit(true)
      if (finalResult) await handleAlertResponse(finalResult as AlertResponse)
      return true
    }
    showToast('Acción cancelada', 'info')
    return false
  }

  await handleAlertResponse(response)
  return undefined
}

// Simple confirmation dialog without a prior server round-trip (Guía TecNM 4.4 y 4.5)
export async function askConfirmation(params: {
  title: string
  text?: string
  confirmText?: string
  danger?: boolean
}): Promise<boolean> {
  const result = await Swal.fire({
    title: params.title,
    text: params.text || '',
    icon: params.danger ? 'warning' : 'question',
    showCancelButton: true,
    buttonsStyling: false,
    focusCancel: params.danger === true,
    customClass: {
      popup: 'modal modal--sm tecnm-swal-popup',
      title: 'tecnm-swal-title',
      htmlContainer: 'tecnm-swal-text',
      confirmButton: params.danger ? 'btn btn-danger' : 'btn btn-primary',
      cancelButton: 'btn btn-ghost',
      actions: 'ft ft--split tecnm-swal-actions'
    },
    confirmButtonText: params.confirmText || (params.danger ? 'Eliminar' : 'Aceptar'),
    cancelButtonText: 'Cancelar'
  })
  return result.isConfirmed
}