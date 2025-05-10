using APBD_s31722_9_APi_2.DataLayer.Models;
using APBD_s31722_9_APi_2.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_s31722_9_APi_2.Controllers;

[ApiController]
[Route("api/warehouses/")]
public class WareHouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WareHouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }


    [HttpPost]
    public async Task<IActionResult> AddProductToWarehouse([FromBody] WareHouseRequestDto request)
    {
        var newId = await _warehouseService.AddProductToWarehouse(request);
        return Ok(new { InsertedId = newId });
    }

    [HttpPost("by-procedure")]
    public async Task<IActionResult> AddProductToWarehouseByProcedure([FromBody] WareHouseRequestDto request)
    {
        var newId = await _warehouseService.AddProductToWarehouseByProcedure(request);
        return Ok(new { InsertedId = newId });
    }
}