using System.Data;
using D.API.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

[ApiController] 
[Route("api/work-centers")]
public class WorkCentersController: ControllerBase
{
    private readonly WorkCentersService workCentersService;
    private readonly IHubContext<BroadcastHub> _hubContext;

    public WorkCentersController(WorkCentersService workCentersService, IHubContext<BroadcastHub> hubContext)
    {
        this.workCentersService = workCentersService;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkCenters()
    {
        var data = await workCentersService.GetWorkCenters();
        return Ok(new {data});
    }

    [HttpPost("breakdown/{id}")]
    public async Task<IActionResult> ReportBreakdown(int id)
    {
        var data = await workCentersService.ReportBreakdown(id);

        // 2. Phát tín hiệu cho tất cả các màn hình Blazor
        await _hubContext.Clients.All.SendAsync("MachineStatusChanged", id, "Bị hỏng");

        return Ok(new {data});
    }

    // GET /api/work-centers/status: 
    // Lấy danh sách máy kèm trạng thái (Running, Breakdown, Maintenance).
    

    // PATCH /api/work-centers/{id}/status: 
    // Cập nhật trạng thái máy (ví dụ: khi máy hỏng, Dashboard sẽ cảnh báo không cho điều phối việc vào máy đó).

    
}