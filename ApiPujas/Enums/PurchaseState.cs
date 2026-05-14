namespace ApiPujas.Enums
{
    /// <summary>
    /// Define los posibles estados de una compra generada al finalizar una subasta.
    /// </summary>
    public enum PurchaseState
    {
        /// <summary>
        /// La compra está pendiente de pago por parte del comprador.
        /// </summary>
        Pending,

        /// <summary>
        /// El pago ha sido completado y la compra queda registrada como finalizada.
        /// </summary>
        Finalized

    }
}