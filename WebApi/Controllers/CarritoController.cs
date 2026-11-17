using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.Services;

namespace WebApi.Controllers
{
    /// <summary>
    /// Servicio SOA: Carrito de Compras
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CarritoController : ControllerBase
    {
        private readonly ICarritoService _carritoService;

        /// <summary>
        /// Constructor del controlador de carrito de compras.
        /// </summary>
        /// <param name="carritoService">Servicio de gestión de carrito.</param>
        public CarritoController(ICarritoService carritoService)
        {
            _carritoService = carritoService;
        }

        /// <summary>
        /// Obtener el carrito de compras del cliente
        /// </summary>
        /// <param name="clienteId">Identificador del cliente.</param>
        /// <param name="tiendaId">Identificador opcional de la tienda física para validar stock.</param>
        /// <param name="direccionId">Identificador opcional de la dirección de entrega.</param>
        /// <param name="tiendaRetiroId">Identificador opcional de la tienda de retiro.</param>
        /// <returns>Contenido del carrito, subtotal general y estado de validación de pre-requisitos de compra.</returns>
        [HttpGet("{clienteId:int}")]
        [ProducesResponseType(typeof(CarritoResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerCarrito(
            [FromRoute] int clienteId,
            [FromQuery] int? tiendaId = null,
            [FromQuery] int? direccionId = null,
            [FromQuery] int? tiendaRetiroId = null)
        {
            var carrito = await _carritoService.ObtenerCarritoAsync(clienteId, tiendaId, direccionId, tiendaRetiroId);
            return Ok(carrito);
        }

        /// <summary>
        /// Servicio 10: Gestión de Carrito de Compras (Agregar, Editar, Eliminar)
        /// </summary>
        /// <param name="request">Datos de la acción (AGREGAR, EDITAR, ELIMINAR), producto, cantidad y tienda/dirección.</param>
        /// <returns>Contenido del carrito y subtotal actualizados.</returns>
        /// <remarks>
        /// Regla de negocio: Producto, cantidad y acción elegida (AGREGAR, EDITAR o ELIMINAR). Contenido del carrito y subtotal actualizados.
        /// Para continuar con la compra es obligatorio ingresar una dirección de entrega o seleccionar un local de retiro.
        /// </remarks>
        [HttpPost("gestionar")]
        [ProducesResponseType(typeof(CarritoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GestionarCarrito([FromBody] GestionarCarritoRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var carritoActualizado = await _carritoService.GestionarCarritoAsync(request);
                return Ok(carritoActualizado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
