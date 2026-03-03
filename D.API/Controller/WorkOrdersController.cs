using D.API.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

[ApiController]
[Route("api/work-orders")]
public class WorkOrdersController : ControllerBase
{
    private readonly WorkOrdersService workOrdersService;
    private readonly IHubContext<BroadcastHub> _hubContext;

    public WorkOrdersController(WorkOrdersService workOrdersService, IHubContext<BroadcastHub> hubContext)
    {
        this.workOrdersService = workOrdersService;
        _hubContext = hubContext;
    }

    // Lấy danh sách các Work Orders có trạng thái Planned (chưa sản xuất)

    [HttpGet("pending")]
    public async Task<IActionResult> GetPlanned()
    {
        var data = await workOrdersService.GetPlanned();
        return Ok(new { data });
    }

    // Lấy chi tiết các bước (Route Steps) của một đơn hàng, 
    // bao gồm cả việc gợi ý các máy (WorkCenter) 
    // có khả năng đáp ứng dựa trên bảng ResourceCapabilities.

    [HttpGet("{id}/route-details")]
    public async Task<IActionResult> GetRouteStep(int id)
    {
        var data = await workOrdersService.GetRouteStep(id);
        return Ok(new { data });
    }

}