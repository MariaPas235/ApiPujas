namespace ApiPujas.DTOs
{
    /// <summary>
    /// DTO con los datos necesarios para registrar una nueva puja sobre un producto en subasta.
    /// </summary>
    public class CreateBidDto
    {
        /// <summary>
        /// Identificador único del producto sobre el que se realiza la puja.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Identificador único del usuario que realiza la puja.
        /// </summary>
        public int BuyerId { get; set; }

        /// <summary>
        /// Importe ofertado. Debe ser superior a la puja más alta registrada
        /// o al precio inicial del producto si no hay pujas previas.
        /// </summary>
        public decimal Amount { get; set; }
    }
}