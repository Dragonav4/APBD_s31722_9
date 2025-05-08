using System.Data;
using APBD_s31722_8_API.Datalayer;
using APBD_s31722_9_APi_2.DataLayer.Models;
using Microsoft.AspNetCore.Mvc;

namespace APBD_s31722_9_APi_2.Controllers;

[ApiController]
[Route("api/warehouses/")]
public class WareHouseController : ControllerBase
{
    private readonly DbClient _dbClient;

    public WareHouseController(DbClient dbClient)
    {
        _dbClient = dbClient;
    }

    
    [HttpPost]
    public async Task<IActionResult> AddProductToWarehouse([FromBody] WareHouseRequestDTO request)
    {
        if(request.Amount <= 0 ) return BadRequest("Amount cannot be negative");
        var price = await _dbClient.ReadScalarAsync<decimal?>(
            "SELECT Price FROM Product WHERE IdProduct =@Id", 
            new () { { "@Id", request.IdProduct }});
        if (price == null) return BadRequest("Product not found if price for product not found");

        var wareHouseExist = await _dbClient.ReadScalarAsync(
            @"SELECT 1 FROM Warehouse WHERE IdWarehouse = @Id",
            new() { { "@Id", request.IdWarehouse } });
        if (!wareHouseExist.HasValue) return BadRequest("Warehouse not found");

        var orderId = await _dbClient.ReadScalarAsync(
            @"SELECT IdOrder FROM [Order] WHERE IdProduct = @Id 
                    AND Amount >= @Amount AND CreatedAt <=@CreatedAt", 
            new() {  
                    {"@Id", request.IdProduct},
                    {"@Amount", request.Amount},
                    {"@CreatedAt", request.CreatedAt} });
        if (!orderId.HasValue) return BadRequest("Order not found");

        var isAlreadyFulfield = await _dbClient.ReadScalarAsync(
            @"select 1 from Product_Warehouse WHERE IdOrder = @OrderId",
            new() { { "@OrderId", orderId.Value } });
        
        if (isAlreadyFulfield.HasValue) return BadRequest("Product has already row in Product_Warehouse");

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

    //      await _dbClient.ExecuteNonQueryAsync(
    //         @"UPDATE [Order]
    //         SET FulfilledAt = @Now
    //         WHERE IdOrder = @OrderId", new()
    //         {
    //             { "@Now", DateTime.Now}, 
    //             {"@OrderId", orderId.Value }
    //         });
    //
    //
    //     var newID = await _dbClient.ExecuteNonQueryAsync(
    //         @"INSERT INTO Product_Warehouse2222
    //            (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
    //           VALUES(@WarehouseId, @ProductId, @OrderId, @Amount, @TotalPrice, @Now)",
    //         new() {
    //         {"@WarehouseId", request.IdWarehouse},
    //         {"@ProductId", request.IdProduct},
    //         {"@OrderId", orderId.Value},
    //         {"@Amount", request.Amount},
    //         {"@TotalPrice", price * request.Amount},
    //         {"@Now", DateTime.Now}});
    return Ok(new { InsertedId = newId });
    }

    [HttpPost("by-procedure")]
    public async Task<IActionResult> AddProductToWarehouseByProcedure([FromBody] WareHouseRequestDTO request)
    {
        if(request.Amount < 0 ) return BadRequest("Amount cannot be negative");
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
        if (result.HasValue) {
        
            return Ok(new { InsertedId = result.Value });    
        }
        
        return BadRequest("Procedure failed or returned no ID.");

    }
}