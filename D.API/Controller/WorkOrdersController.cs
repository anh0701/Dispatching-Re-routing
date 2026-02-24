using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/work-orders")]
public class WorkOrdersController : ControllerBase
{
    private readonly WorkOrdersService workOrdersService;

    public WorkOrdersController(WorkOrdersService workOrdersService)
    {
        this.workOrdersService = workOrdersService;
    }

    // Lấy danh sách các Work Orders có trạng thái Planned (chưa sản xuất)

    [HttpGet("pending")]
    public async Task<IActionResult> GetPlanned()
    {
        var data = workOrdersService.GetPlanned();
        return Ok(new { data });
    }

    // Lấy chi tiết các bước (Route Steps) của một đơn hàng, 
    // bao gồm cả việc gợi ý các máy (WorkCenter) 
    // có khả năng đáp ứng dựa trên bảng ResourceCapabilities.

    [HttpGet("{id}/route-details")]
    public async Task<IActionResult> GetRouteStep(int id)
    {
        var result = workOrdersService.GetRouteStep(id);
        return Ok(new { result });
    }
}