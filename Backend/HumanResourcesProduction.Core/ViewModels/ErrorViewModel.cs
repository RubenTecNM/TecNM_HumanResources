namespace HumanResourcesProduction.Models.ViewModels;

/// <summary>
/// ViewModel para la vista de error genérico de ASP.NET MVC.
/// No es una entidad de dominio: solo transporta datos de diagnóstico a la vista.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// Identificador único de la solicitud HTTP que generó el error.
    /// Null si no está disponible o no es relevante mostrarlo.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Calculado: true si se debe mostrar el RequestId en la vista de error.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
