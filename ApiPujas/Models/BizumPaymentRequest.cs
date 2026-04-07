using System.Text.Json.Serialization; // No olvides este using

public class BizumPaymentRequest
{
    [JsonPropertyName("data")]
    public string Data { get; set; }

    [JsonPropertyName("order")] // <--- Esto soluciona tu error 400
    public string Order { get; set; }

    [JsonPropertyName("importe")]
    public string Importe { get; set; }

    [JsonPropertyName("urlPageOk")]
    public string UrlPageOk { get; set; }

    [JsonPropertyName("urlPageKO")]
    public string UrlPageKO { get; set; }

    [JsonPropertyName("redsysNotificationApi")]
    public string RedsysNotificationApi { get; set; }

    [JsonPropertyName("languageTpv")]
    public int LanguageTpv { get; set; }
}