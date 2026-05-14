namespace ApiPujas.Models.Dto
{
    /// <summary>
    /// DTO genérico utilizado como envoltorio de respuesta en los endpoints de la API.
    /// Estandariza la estructura de todas las respuestas indicando si la operación
    /// fue exitosa, un mensaje descriptivo y los datos devueltos.
    /// </summary>
    public class ResponseDto
    {    
        /// <summary>
         /// Datos devueltos por la operación. Puede ser una entidad, una lista
         /// o <c>null</c> si la operación no produce datos (por ejemplo, un error).
         /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Indica si la operación se completó correctamente. Por defecto <c>true</c>.
        /// </summary>
        public bool IsSuccess { get; set; } = true;

        /// <summary>
        /// Mensaje descriptivo del resultado de la operación. En caso de error,
        /// contiene el detalle del fallo. Por defecto cadena vacía.
        /// </summary>
        public string Message { get; set; } = "";
    }
}
