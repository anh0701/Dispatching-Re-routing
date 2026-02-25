using System.Data;
using Microsoft.AspNetCore.Mvc;

[ApiController] 
[Route("api/work-centers")]
public class WorkCentersController: ControllerBase
{
    private readonly WorkCentersService workCentersService;

    public WorkCentersController(WorkCentersService workCentersService)
    {
        this.workCentersService = workCentersService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkCenters()
    {
        var data = await workCentersService.GetWorkCenters();
        return Ok(new {data});
    }

    // GET /api/work-centers/status: 
    // Lấy danh sách máy kèm trạng thái (Running, Breakdown, Maintenance).
    

    // PATCH /api/work-centers/{id}/status: 
    // Cập nhật trạng thái máy (ví dụ: khi máy hỏng, Dashboard sẽ cảnh báo không cho điều phối việc vào máy đó).

    
}