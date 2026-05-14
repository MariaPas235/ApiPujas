namespace ApiPujas.Enums
{

    /// <summary>
    /// Define los posibles estados del ciclo de vida de un producto en subasta.
    /// </summary>
    public enum ProductState
    {
        /// <summary>
        /// El producto está programado pero la subasta aún no ha comenzado.
        /// </summary>
        Scheduled,

        /// <summary>
        /// La subasta está en curso y acepta pujas.
        /// </summary>
        Active,

        /// <summary>
        /// La subasta ha finalizado. Ya no se aceptan pujas.
        /// </summary>
        Closed,

        /// <summary>
        /// El producto ha sido enviado al comprador tras completarse la compra.
        /// </summary>
        Sended
    }
}
