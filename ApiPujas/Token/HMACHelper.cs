using System.Security.Cryptography;
using System.Text;

public static class HMACHelper
{
    /// <summary>
    /// Genera un token HMAC para autenticar peticiones HTTP.
    /// Combina método, ruta, nonce, timestamp y opcionalmente el cuerpo de la petición.
    /// </summary>
    public static string GenerateHmacToken(string method, string path, string clientId, string secretKey, string requestBody = "")
    {
        // Genera un identificador único para evitar repetición de tokens
        var nonce = Guid.NewGuid().ToString();
        // Timestamp en formato ISO 8601 UTC
        var timestamp = DateTime.UtcNow.ToString("o");

        // Construye el string base con método, ruta, nonce y timestamp
        var requestContent = new StringBuilder()
            .Append(method.ToUpper())
            .Append(path.ToUpper())
            .Append(nonce)
            .Append(timestamp);

        // Si el método es POST o PUT, incluye el cuerpo de la petición en el cálculo
        if (method == HttpMethod.Post.Method || method == HttpMethod.Put.Method)
        {
            requestContent.Append(requestBody);
        }

        // Convierte clave secreta y contenido a bytes para cálculo HMAC
        var secretBytes = Encoding.UTF8.GetBytes(secretKey);
        var requestBytes = Encoding.UTF8.GetBytes(requestContent.ToString());

        // Calcula el hash HMAC-SHA256
        var hmac = new HMACSHA256(secretBytes);
        var computedHash = hmac.ComputeHash(requestBytes);

        // Codifica el hash resultante a Base64 para transmisión segura
        var computedToken = Convert.ToBase64String(computedHash);

        // Devuelve token con clientId, token, nonce y timestamp concatenados
        return $"{clientId}|{computedToken}|{nonce}|{timestamp}";
    }
}
