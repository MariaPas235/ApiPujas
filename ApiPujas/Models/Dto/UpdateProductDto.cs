using ApiPujas.Enums;

namespace ApiPujas.Models.Dto
{

    /// <summary>
    /// DTO con los datos editables de un producto en subasta.
    /// Todos los campos son obligatorios ya que se realiza una actualización completa
    /// de la entidad, incluyendo su estado en el ciclo de vida.
    /// </summary>
    public class UpdateProductDto
    {
        /// <summary>
        /// Título descriptivo del producto mostrado en el listado de subastas.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Descripción detallada del producto, condición y características relevantes.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Precio de salida de la subasta. Ninguna puja podrá ser igual o inferior a este valor.
        /// </summary>
        public decimal InitialPrice { get; set; }

        /// <summary>
        /// Fecha y hora en UTC en que la subasta cerrará y dejará de aceptar pujas.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Fecha y hora en UTC en que la subasta cerrará y dejará de aceptar pujas.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// URL o ruta de la imagen representativa del producto.
        /// </summary>
        public string Photo { get; set; }

        /// <summary>
        /// Categoría a la que pertenece el producto.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Estado actual del producto dentro del ciclo de vida de la subasta.
        /// </summary>
        public ProductState productState { get; set; }
    }
}
