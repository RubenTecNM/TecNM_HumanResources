namespace HumanResourcesProduction.Settings;

/// <summary>
/// Configuración del servidor SMTP para el envío de correos electrónicos.
/// Se bindea desde la sección "SmtpSettings" en appsettings.json usando IOptions&lt;T&gt;.
/// No es una entidad de dominio: no se persiste en la base de datos.
/// </summary>
public class SmtpSettings
{
    /// <summary>Dirección de correo desde la que se envían los mensajes.</summary>
    public string EmailSender { get; set; } = string.Empty;

    /// <summary>Host del servidor SMTP (ej: smtp.gmail.com).</summary>
    public string SmtpServer { get; set; } = string.Empty;

    /// <summary>Puerto del servidor SMTP (ej: 587 para TLS).</summary>
    public int SmtpPort { get; set; }

    /// <summary>Contraseña de aplicación generada para el correo emisor.</summary>
    public string PasswordApp { get; set; } = string.Empty;

    /// <summary>Activa o desactiva el cifrado SSL/TLS en la conexión.</summary>
    public bool EnableSsl { get; set; } = true;
}
