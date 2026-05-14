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
        var nonce = Guid.NewGuid().ToString();
        var timestamp = DateTime.UtcNow.ToString("o");

        var requestContent = new StringBuilder()
            .Append(method.ToUpper())
            .Append(path.ToUpper())
            .Append(nonce)
            .Append(timestamp);

        if (method == HttpMethod.Post.Method || method == HttpMethod.Put.Method)
        {
            requestContent.Append(requestBody);
        }

        var secretBytes = Encoding.UTF8.GetBytes(secretKey);
        var requestBytes = Encoding.UTF8.GetBytes(requestContent.ToString());      
        var hmac = new HMACSHA256(secretBytes);
        var computedHash = hmac.ComputeHash(requestBytes);
        var computedToken = Convert.ToBase64String(computedHash);

        return $"{clientId}|{computedToken}|{nonce}|{timestamp}";
    }
}
