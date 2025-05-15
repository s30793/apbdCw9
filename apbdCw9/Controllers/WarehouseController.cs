using Microsoft.AspNetCore.Mvc;
using apbdCw9.Model.DTOs;
using apbdCw9.Services;

namespace apbdCw9.Controllers
{
    [ApiController]
    [Route("api/warehouse")]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpPost]
        public async Task<IActionResult> AddProductToWarehouse([FromBody] WarehouseRequestDto request)
        {
            if (!await _warehouseService.ValidationOfProductAndWarehouseAsync(request))
                return BadRequest("Invalid product, warehouse, or amount.");
            var idOrder = await _warehouseService.GetOrderAsync(request);
            if (idOrder is null)
                return BadRequest("No order found.");

            if (await _warehouseService.IsOrderExecutedAsync(idOrder.Value))
                return BadRequest("The order already has all the products in it.");

            if (!await _warehouseService.UpdateOrderAsync(idOrder.Value))
                return NotFound("Update failed.");

            request.IdOrder = idOrder;

            var productPrice = await _warehouseService.GetProductPriceAsync(request.IdProduct);
            var newProductWarehouseId = await _warehouseService.InsertProductAsync(request, productPrice);

            return Ok(new { IdProductWarehouse = newProductWarehouseId, Message = "Product successfully added to warehouse." });
        }
    }
}





