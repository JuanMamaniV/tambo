using WebApi.DTOs;

namespace WebApi.Services
{
    /// <summary>
    /// Contrato del servicio de promociones, combos especiales y validación de cupones.
    /// </summary>
    public interface IPromocionService
    {
        /// <summary>
        /// Servicio 8: Lista las promociones y combos ("Tambo Packs") vigentes con precios y descuentos.
        /// </summary>
        /// <returns>Colección de combos y packs activos.</returns>
        Task<IEnumerable<PromocionPackDto>> ListarPromocionesCombosAsync();

        /// <summary>
        /// Servicio 11: Valida un cupón de descuento por vigencia, monto mínimo y límite de canjes.
        /// </summary>
        /// <param name="request">Código del cupón y subtotal de la compra.</param>
        /// <returns>Resultado de la validación, monto descontado y nuevo total.</returns>
        Task<ValidarCuponResponseDto> ValidarCuponAsync(ValidarCuponRequestDto request);
    }
}
