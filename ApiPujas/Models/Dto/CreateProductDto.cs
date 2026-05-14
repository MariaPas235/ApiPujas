/// <summary>
/// DTO con los datos necesarios para publicar un nuevo producto en subasta.
/// El producto se crea automáticamente en estado <see cref="ProductState.Scheduled"/>.
/// </summary>
public class CreateProductDto
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
    /// Fecha y hora en UTC en que la subasta pasará a estado <see cref="ProductState.Active"/>.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Fecha y hora en UTC en que la subasta cerrará y dejará de aceptar pujas.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// URL o ruta de la imagen principal del producto.
    /// </summary>
    public string Photo { get; set; }

    /// <summary>
    /// Categoría a la que pertenece el producto (por ejemplo: Motor, Tecnologia, Hogar).
    /// </summary>
    public string Category { get; set; }


    /// <summary>
    /// Identificador único del usuario vendedor que publica el producto.
    /// </summary>
    public int SellerId { get; set; }
}