using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Services;

namespace WebApi.Controllers
{
    /// <summary>
    /// Servicio SOA: Promociones y Cupones
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PromocionesController : ControllerBase
    {
        private readonly IPromocionService _promocionService;

        /// <summary>
        /// Constructor del controlador de promociones y cupones.
        /// </summary>
        /// <param name="promocionService">Servicio de promociones y campañas de descuento.</param>
        public PromocionesController(IPromocionService promocionService)
        {
            _promocionService = promocionService;
        }

        /// <summary>
        /// Servicio 8: Listar Promociones y Combos ("Tambo Packs")
        /// </summary>
        /// <returns>Colección de combos y packs vigentes con su detalle de productos.</returns>
        /// <remarks>
        /// Regla de negocio: Solo deben mostrarse promociones y combos vigentes (fecha_inicio &lt;= hoy &lt;= fecha_fin).
        /// </remarks>
        [HttpGet("packs")]
        [ProducesResponseType(typeof(IEnumerable<PromocionPackDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarPromociones()
        {
            var packs = await _promocionService.ListarPromocionesCombosAsync();
            return Ok(packs);
        }

        /// <summary>
        /// Servicio 11: Validar Cupón de Descuento
        /// </summary>
        /// <param name="request">Código del cupón y subtotal de la compra.</param>
        /// <returns>Descuento calculado y total recalculado si el cupón es válido, o 400 si no aplica.</returns>
        /// <remarks>
        /// Regla de negocio: Valida código, vigencia, monto mínimo y calcula el descuento y total actualizados.
        /// </remarks>
        [HttpPost("cupones/validar")]
        [ProducesResponseType(typeof(ValidarCuponResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidarCuponResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidarCupon([FromBody] ValidarCuponRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var resultado = await _promocionService.ValidarCuponAsync(request);
            if (!resultado.EsValido)
            {
                return BadRequest(resultado);
            }

            return Ok(resultado);
        }
    }
}
