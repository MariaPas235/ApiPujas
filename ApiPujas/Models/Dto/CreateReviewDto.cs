namespace ApiPujas.DTOs
{
    /// <summary>
    /// DTO con los datos necesarios para crear una reseña sobre un vendedor
    /// vinculada a una compra completada.
    /// </summary>
    public class CreateReviewDto
    {
        /// <summary>
        /// Identificador único de la compra a la que se asocia la reseña.
        /// Determina automáticamente el vendedor y el comprador implicados.
        /// </summary>
        public int PurchaseId { get; set; }

        /// <summary>
        /// Puntuación otorgada al vendedor. Se utiliza para recalcular
        /// su reputación media tras registrar la reseña.
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Comentario opcional del comprador sobre la experiencia con el vendedor.
        /// </summary>
        public string? Comment { get; set; }
    }
}