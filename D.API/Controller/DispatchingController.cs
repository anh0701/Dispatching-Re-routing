using System.Data;
using Dapper;
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
    [HttpPost("{id}/reschedule")]
    public async Task<IActionResult> Reschedule(AssignmentRequest req)
    {
        var data = await dispatchingService.Reschedule();

        return Ok(new {data});
    }

    // DELETE /api/dispatching-board/{id}: 
    // Hủy phân công một bước công việc (trả về trạng thái chờ)


    // PATCH /api/dispatching-board/{id}/start: 
    // Cập nhật ActualStart và chuyển Status sang Processing


    // PATCH /api/dispatching-board/{id}/report-progress:
    // Input: ActualQuantity (số lượng hoàn thành thêm).
    // Logic: Cộng dồn vào ActualQuantity hiện tại.


    // PATCH /api/dispatching-board/{id}/complete: 
    // Cập nhật ActualEnd, chuyển Status sang Completed. 
    // Nếu là bước cuối cùng của Route, cập nhật Status của WorkOrders thành Completed.


}