using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Text.Json;
using System.Net.Http;

namespace ApiPujas.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class BizumController : ControllerBase
    {

        private readonly HttpClient _httpClient;

        /// <summary>
        /// Constructor del controlador PaymentsController.
        /// </summary>
        /// <param name="httpClientFactory">Factoría para crear instancias de HttpClient.</param>
        public BizumController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        /// <summary>
        /// Endpoint para realizar un pago utilizando Bizum.
        /// </summary>
        /// <param name="request">Datos del pago en formato BizumPaymentRequest.</param>
        /// <returns>ActionResult que representa el resultado de la operación.</returns>
        [HttpPost("bizum")]
        public async Task<IActionResult> PayWithBizum([FromBody] BizumPaymentRequest request)
        {
            // Verifica que el modelo recibido sea válido
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // URL del endpoint de la API de Bizum para inicializar el pago
            var bizumApiUrl = "/api/v1/Bizum/InitBizumPayment";
            var fullUrl = "http://localhost:5000" + bizumApiUrl;

            // Serializa el objeto request a JSON para enviarlo en la solicitud
            var requestBody = JsonSerializer.Serialize(request);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            // Credenciales necesarias para firmar la petición
            var clientId = "178e124f-a127-49ec-aeeb-d8d1c576ddf8";
            var secretKey = "XjfpOdT+D9uYn40adDA7A0QOtsfT81PO+KEEfsLsqKc=";

            // Genera el token HMAC para autenticación
            var token = HMACHelper.GenerateHmacToken("POST", bizumApiUrl, clientId, secretKey, requestBody);
            Console.WriteLine("Token HMAC generado: " + token);

            // Crea el mensaje HTTP con encabezado de autenticación HMAC
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, fullUrl)
            {
                Content = content
            };
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("HMAC", token);

            try
            {
                // Envía la solicitud al servidor de Bizum
                var response = await _httpClient.SendAsync(requestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Verifica si la respuesta fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Comprueba si la respuesta contiene un formulario HTML
                    if (responseContent.Contains("</form>"))
                    {
                        Console.WriteLine("🔥 HTML recibido de Bizum:");
                        Console.WriteLine(responseContent);

                        // Agrega un script para autoenviar el formulario recibido
                        var autoSubmitHtml = responseContent.Replace("</form>", "</form><script>document.forms[0].submit();</script>");
                        return Content(autoSubmitHtml, "text/html");
                    }
                    else
                    {
                        // Si no contiene formulario, se considera inválido
                        return BadRequest(new { message = "Respuesta de Bizum no contenía formulario HTML.", bizumResponse = responseContent });
                    }
                }

                // Devuelve error si el código de respuesta no fue exitoso
                return StatusCode((int)response.StatusCode, new
                {
                    message = "Error al enviar a Bizum.",
                    bizumResponse = responseContent
                });
            }
            catch (HttpRequestException ex)
            {
                // Manejo de excepciones de red o de conexión
                return StatusCode(500, new
                {
                    message = "Excepción al conectar con Bizum.",
                    error = ex.Message
                });
            }
        }

    }
}
