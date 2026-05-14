using System.Text.Json.Serialization; // No olvides este using

/// <summary>
/// Modelo de solicitud para iniciar un pago mediante Bizum a través de Redsys.
/// </summary>
public class BizumPaymentRequest
{

    /// <summary>
    /// Datos cifrados de la transacción requeridos por Redsys.
    /// </summary>
    [JsonPropertyName("data")]
    public string Data { get; set; }

    /// <summary>
    /// Número de pedido único de la transacción. Debe ser único por cada solicitud de pago.
    /// </summary>
    [JsonPropertyName("order")] 
    public string Order { get; set; }

    /// <summary>
    /// Importe de la transacción expresado en céntimos (ej: "1050" = 10,50 €).
    /// </summary>
    [JsonPropertyName("importe")]
    public string Importe { get; set; }

    /// <summary>
    /// URL de redirección cuando el pago se completa correctamente.
    /// </summary
    [JsonPropertyName("urlPageOk")]
    public string UrlPageOk { get; set; }

    /// <summary>
    /// URL de redirección cuando el pago falla o es cancelado.
    /// </summary>
    [JsonPropertyName("urlPageKO")]
    public string UrlPageKO { get; set; }

    /// <summary>
    /// URL del endpoint propio para recibir las notificaciones de resultado de Redsys.
    /// </summary>
    [JsonPropertyName("redsysNotificationApi")]
    public string RedsysNotificationApi { get; set; }

    /// <summary>
    /// Código de idioma del TPV virtual (ej: 1 = español).
    /// </summary>
    [JsonPropertyName("languageTpv")]
    public int LanguageTpv { get; set; }
}