using System.Data;
using APBD_s31722_8_API.Datalayer;
using APBD_s31722_9_APi_2.DataLayer.Models;
using APBD_s31722_9_APi_2.Exceptions;

namespace APBD_s31722_9_APi_2.Services;

public interface IWarehouseService
{
    Task<int> AddProductToWarehouse(WareHouseRequestDto request);
    Task<decimal> AddProductToWarehouseByProcedure(WareHouseRequestDto request);
}

public class WarehouseService : IWarehouseService
{
    private readonly DbClient _dbClient;

    public WarehouseService(DbClient dbClient)
    {
        _dbClient = dbClient;
    }

    
    public async Task<int> AddProductToWarehouse(WareHouseRequestDto request)
    {
        if(request.Amount <= 0 ) throw new ArgumentException("Amount cannot be negative");
        var price = await _dbClient.ReadScalarAsync<decimal?>(
            "SELECT Price FROM Product WHERE IdProduct =@Id", 
            new () { { "@Id", request.IdProduct }});
        if (price == null) throw new BadRequestException("Product not found if price for product not found");

        var wareHouseExist = await _dbClient.ReadScalarAsync(
            @"SELECT 1 FROM Warehouse WHERE IdWarehouse = @Id",
            new() { { "@Id", request.IdWarehouse } });
        if (!wareHouseExist.HasValue) throw new BadRequestException("Warehouse not found");

        var orderId = await _dbClient.ReadScalarAsync(
            @"SELECT IdOrder FROM [Order] WHERE IdProduct = @Id 
                    AND Amount >= @Amount AND CreatedAt <=@CreatedAt", 
            new() {  
                    {"@Id", request.IdProduct},
                    {"@Amount", request.Amount},
                    {"@CreatedAt", request.CreatedAt} });
        if (!orderId.HasValue) throw new BadRequestException("Order not found");

        var isAlreadyFulfield = await _dbClient.ReadScalarAsync(
            @"select 1 from Product_Warehouse WHERE IdOrder = @OrderId",
            new() { { "@OrderId", orderId.Value } });
        
        if (isAlreadyFulfield.HasValue) throw new BadRequestException("Product has already row in Product_Warehouse");

        var newId = await _dbClient
            .ExecuteNonQueriesAsTransactionAsync([
                new CommandConfig
                {
                    Query = @"UPDATE [Order]
                        SET FulfilledAt = @Now
                        WHERE IdOrder = @OrderId",
                    Parameters = new { Now = DateTime.Now, OrderId = orderId.Value }
                },

                new CommandConfig
                {
                    Query = @"INSERT INTO Product_Warehouse
                        (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                        VALUES(@WarehouseId, @ProductId, @OrderId, @Amount, @TotalPrice, @Now)",
                    Parameters = new
                    {
                        WarehouseId = request.IdWarehouse,
                        ProductId = request.IdProduct,
                        OrderId = orderId.Value,
                        Amount = request.Amount,
                        TotalPrice= price * request.Amount,
                        Now= DateTime.Now}
                    },
            ]);
    return newId;
    }

    public async Task<decimal> AddProductToWarehouseByProcedure(WareHouseRequestDto request)
    {
        if(request.Amount < 0 ) throw new ArgumentException("Amount cannot be negative");
        var result = await _dbClient.ReadScalarAsync<decimal?>(
            "AddProductToWarehouse",
            new()
            {
                { "@IdProduct", request.IdProduct },
                { "@IdWarehouse", request.IdWarehouse },
                { "@Amount", request.Amount },
                { "@CreatedAt", request.CreatedAt }
            },
            CommandType.StoredProcedure);
        return result.HasValue ? result.Value : throw new Exception("Procedure failed or returned no ID.");

    }
}