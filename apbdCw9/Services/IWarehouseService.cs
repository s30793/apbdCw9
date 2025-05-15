using apbdCw9.Model.DTOs;

namespace apbdCw9.Services
{ public interface IWarehouseService
    {
        Task<bool> ValidationOfProductAndWarehouseAsync(WarehouseRequestDto request);
        Task<int?> GetOrderAsync(WarehouseRequestDto request);
        Task<bool> IsOrderExecutedAsync(int idOrder);
        Task<bool> UpdateOrderAsync(int idOrder);
        Task<decimal> GetProductPriceAsync(int idProduct);
        Task<int> InsertProductAsync(WarehouseRequestDto request, decimal productPrice);
    }
}
