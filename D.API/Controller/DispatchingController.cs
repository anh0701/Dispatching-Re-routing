using Microsoft.AspNetCore.Mvc;

[ApiController] 
[Route("api/dispatching-board")]
public class DispatchingController: ControllerBase
{
    private readonly DispatchingService dispatchingService;

    public DispatchingController(DispatchingService dispatchingService)
    {
        this.dispatchingService = dispatchingService;
    }
    
    // Lấy toàn bộ dữ liệu đang hiển thị trên bảng 
    // (lọc theo khoảng thời gian hoặc theo từng Máy)

   [HttpGet]
    public async Task<IActionResult> GetBoard([FromQuery] DateTime date)
    {
        var data = await dispatchingService.GetBoard(date);
        return Ok(new { data });
    }

    // Gán việc vào máy (Tính toán thời gian tự động)
    [HttpPost("assign")]
    public async Task<IActionResult> AssignTaskToBoard(AssignmentRequest req)
    {
        var data = await dispatchingService.AssignTaskToBoard(req);

        return Ok(new {data});
    }

    // một API check xem máy có đang bận trong khoảng ScheduledStart đến ScheduledEnd không.


    // PUT /api/dispatching-board/{id}/reschedule: 
    // Cập nhật lại thời gian hoặc đổi máy khi có sự thay đổi kế hoạch.
    [HttpPut("{id}/reschedule")]
    public async Task<IActionResult> Reschedule(AssignmentRequest req)
    {
        var data = await dispatchingService.Reschedule();

        return Ok(new {data});
    }

    // DELETE /api/dispatching-board/{id}: 
    // Hủy phân công một bước công việc (trả về trạng thái chờ)
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelJob (int id)
    {
        var data = await dispatchingService.CancelJob(id);
        return Ok(new {data});
    }

    // PATCH /api/dispatching-board/{id}/start: 
    // Cập nhật ActualStart và chuyển Status sang Processing
    [HttpPatch("{id}/start")]
    public async Task<IActionResult> ChangeProcessing(int id)
    {
        var data = await dispatchingService.ChangeProcessing(id);
        return Ok(new {data});
    }

    // PATCH /api/dispatching-board/{id}/report-progress:
    // Input: ActualQuantity (số lượng hoàn thành thêm).
    // Logic: Cộng dồn vào ActualQuantity hiện tại.
    [HttpPatch("{id}/report-progress")]
    public async Task<IActionResult> GetProgress (int id, GetProgressReq req)
    {
        var data = dispatchingService.GetProgress(id, req);
        return Ok(new {data});
    }

    // PATCH /api/dispatching-board/{id}/complete: 
    // Cập nhật ActualEnd, chuyển Status sang Completed. 
    // Nếu là bước cuối cùng của Route, cập nhật Status của WorkOrders thành Completed.
    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> ChangeComplete(int id)
    {
        var data = dispatchingService.ChangeComplete(id);
        return Ok(new {data});
    }

}