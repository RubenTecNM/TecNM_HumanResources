namespace HumanResourcesProduction.Core.Permissions;

/// <summary>
/// Configuración de campo individual para un tipo de permiso.
/// Define qué se muestra en el formulario de creación y si es requerido.
/// </summary>
public class PermissionTypeFieldConfig
{
    public bool Show { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool Required { get; set; }
}

/// <summary>
/// Configuración visual y de comportamiento de una tarjeta de tipo de permiso.
/// </summary>
public class PermissionTypeCardConfig
{
    public string Icon { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public PermissionTypeFieldConfig Date1 { get; set; } = new();
    public PermissionTypeFieldConfig Date2 { get; set; } = new();
    public PermissionTypeFieldConfig Time1 { get; set; } = new();
    public PermissionTypeFieldConfig Time2 { get; set; } = new();
}

/// <summary>
/// Catálogo estático de configuraciones por tipo de permiso.
/// Esta es la única fuente de verdad para la configuración visual de cada tipo.
/// Se mantiene aquí (en Core, referenciado por todos los proyectos) para evitar
/// acoplamiento entre controladores.
/// </summary>
public static class PermissionTypeConfig
{
    public static readonly Dictionary<int, PermissionTypeCardConfig> CardConfigs = new()
    {
        [1] = new PermissionTypeCardConfig
        {
            Icon = "fa-history",
            Subtitle = "Llegada tarde",
            Date1 = new PermissionTypeFieldConfig { Show = true, Label = "En el día", Required = true },
            Date2 = new PermissionTypeFieldConfig { Show = false, Label = "Al día", Required = false },
            Time1 = new PermissionTypeFieldConfig { Show = true, Label = "Hora de llegada", Required = true },
            Time2 = new PermissionTypeFieldConfig { Show = false, Label = "Hora salida", Required = false }
        },
        [2] = new PermissionTypeCardConfig
        {
            Icon = "fa-sync-alt",
            Subtitle = "Intercambio de turno",
            Date1 = new PermissionTypeFieldConfig { Show = true, Label = "Del día", Required = true },
            Date2 = new PermissionTypeFieldConfig { Show = true, Label = "Por el día", Required = true },
            Time1 = new PermissionTypeFieldConfig { Show = true, Label = "Hora entrada", Required = true },
            Time2 = new PermissionTypeFieldConfig { Show = true, Label = "Hora salida", Required = true }
        },
        [3] = new PermissionTypeCardConfig
        {
            Icon = "fa-calendar-times",
            Subtitle = "Ausencia por uno o varios días",
            Date1 = new PermissionTypeFieldConfig { Show = true, Label = "Del día", Required = true },
            Date2 = new PermissionTypeFieldConfig { Show = true, Label = "Al día", Required = true },
            Time1 = new PermissionTypeFieldConfig { Show = false, Label = "", Required = false },
            Time2 = new PermissionTypeFieldConfig { Show = false, Label = "", Required = false }
        },
        [4] = new PermissionTypeCardConfig
        {
            Icon = "fa-business-time",
            Subtitle = "Modificar horario",
            Date1 = new PermissionTypeFieldConfig { Show = true, Label = "Del día", Required = true },
            Date2 = new PermissionTypeFieldConfig { Show = true, Label = "Al día", Required = true },
            Time1 = new PermissionTypeFieldConfig { Show = true, Label = "Hora entrada", Required = true },
            Time2 = new PermissionTypeFieldConfig { Show = true, Label = "Hora salida", Required = true }
        },
        [5] = new PermissionTypeCardConfig
        {
            Icon = "fa-chalkboard-teacher",
            Subtitle = "Falta de docente (1 día)",
            Date1 = new PermissionTypeFieldConfig { Show = true, Label = "Día de falta", Required = true },
            Date2 = new PermissionTypeFieldConfig { Show = false, Label = "", Required = false },
            Time1 = new PermissionTypeFieldConfig { Show = false, Label = "", Required = false },
            Time2 = new PermissionTypeFieldConfig { Show = false, Label = "", Required = false }
        }
    };
}
