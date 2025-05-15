using apbdCw9.Model.DTOs;
using Microsoft.Data.SqlClient;

namespace apbdCw9.Services
{ public class WarehouseService : IWarehouseService
    {
        private readonly IConfiguration _configuration;
        public WarehouseService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<bool> ValidationOfProductAndWarehouseAsync(WarehouseRequestDto request)
        {
            await using var conn = new SqlConnection(_configuration.GetConnectionString("Default"));
            await conn.OpenAsync();
            var pCom = new SqlCommand("SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct", conn);
            pCom.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            var productExists = (int)((await pCom.ExecuteScalarAsync()) ?? throw new InvalidOperationException());
            var wCom = new SqlCommand("SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @IdWarehouse", conn);
            wCom.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
            var warehouseExists = (int)((await wCom.ExecuteScalarAsync()) ?? throw new InvalidOperationException());

            return productExists > 0 && warehouseExists > 0 && request.Amount > 0;
        }

        public async Task<int?> GetOrderAsync(WarehouseRequestDto request)
        {
            await using var conn = new SqlConnection(_configuration.GetConnectionString("Default"));
            await conn.OpenAsync();
            var com = new SqlCommand(@"
                SELECT IdOrder FROM [Order] 
                WHERE IdProduct = @IdProduct 
                  AND Amount = @Amount 
                  AND CreatedAt < @CreatedAt", conn);

            com.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            com.Parameters.AddWithValue("@Amount", request.Amount);
            com.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);
            var result = await com.ExecuteScalarAsync();
            return result != null ? (int?)result : null;
        }

        public async Task<bool> IsOrderExecutedAsync(int idOrder)
        {
            await using var con = new SqlConnection(_configuration.GetConnectionString("Default"));
            await con.OpenAsync();
            var com = new SqlCommand("SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @IdOrder", con);
            com.Parameters.AddWithValue("@IdOrder", idOrder);
            int count = (int)((await com.ExecuteScalarAsync()) ?? throw new InvalidOperationException());
            return count > 0;
        }

        public async Task<bool> UpdateOrderAsync(int idOrder)
        {
            await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await connection.OpenAsync();
            var cmd = new SqlCommand("UPDATE [Order] SET FulfilledAt = @Now WHERE IdOrder = @IdOrder", connection);
            cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@IdOrder", idOrder);
            int affected = await cmd.ExecuteNonQueryAsync();
            return affected > 0;
        }

        public async Task<decimal> GetProductPriceAsync(int idProduct)
        {
            await using var conn = new SqlConnection(_configuration.GetConnectionString("Default"));
            await conn.OpenAsync();
            var com = new SqlCommand("SELECT Price FROM Product WHERE IdProduct = @IdProduct", conn);
            com.Parameters.AddWithValue("@IdProduct", idProduct);
            return (decimal)((await com.ExecuteScalarAsync()) ?? throw new InvalidOperationException());
        }

        public async Task<int> InsertProductAsync(WarehouseRequestDto request, decimal productPrice)
        {
            await using var conn = new SqlConnection(_configuration.GetConnectionString("Default"));
            await conn.OpenAsync();
            decimal totalPrice = productPrice * request.Amount;
            var com = new SqlCommand(@"
                INSERT INTO Product_Warehouse 
                (IdWarehouse, IdProduct, Amount, IdOrder, Price, CreatedAt)
                OUTPUT INSERTED.IdProductWarehouse 
                VALUES (@IdWarehouse, @IdProduct, @Amount, @IdOrder, @Price, @CreatedAt)", conn);

            com.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
            com.Parameters.AddWithValue("@IdProduct", request.IdProduct);
            com.Parameters.AddWithValue("@Amount", request.Amount);
            com.Parameters.AddWithValue("@IdOrder", request.IdOrder);
            com.Parameters.AddWithValue("@Price", totalPrice);
            com.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

            return (int)((await com.ExecuteScalarAsync()) ?? throw new InvalidOperationException());
        }
    }
}

