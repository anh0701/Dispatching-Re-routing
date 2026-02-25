public class WorkOrderService
{
    private readonly HttpClient _http;
    public WorkOrderService(IHttpClientFactory http) => _http = http.CreateClient("MESApi");

    public async Task<List<JobItem>> GetPendingJobsAsync()
    {
        var result = await _http.GetFromJsonAsync<ApiResponse<List<JobItem>>>(
            "api/work-orders/pending");

        var jobs = result?.Data ?? new List<JobItem>();

        return jobs;
    }

    public async Task<WorkOrderRouteDto?> GetRouteDetailsAsync(int workOrderId)
    {
        var res = await _http.GetFromJsonAsync<ApiResponse<WorkOrderRouteDto>>(
            $"api/work-orders/{workOrderId}/route-details");
        var result = res?.Data ?? new WorkOrderRouteDto();
        return result;
    }

    public async Task<List<Machine>> GetWorkCentersAsync()
    {
        var result = await _http.GetFromJsonAsync<ApiResponse<List<Machine>>>(
            "api/work-centers");
        var res = result?.Data ?? new List<Machine>();
        return res;
    }

    public async Task<List<DispatchTaskDto>> GetBoardAsync(DateTime date)
    {
        var result = await _http.GetFromJsonAsync<ApiResponse<List<DispatchTaskDto>>>(
            $"api/dispatching-board?date={date:yyyy-MM-dd}");

        var res = result?.Data ?? new List<DispatchTaskDto>();
        return res;
    }

    public async Task<bool> AssignJobAsync(
        int workOrderId,
        int workCenterId,
        int stepOrder,
        DateTime scheduledStart)
    {
        var response = await _http.PostAsJsonAsync(
            "api/dispatching-board/assign",
            new
            {
                WorkOrderId = workOrderId,
                WorkCenterId = workCenterId,
                StepOrder = stepOrder,
                ScheduledStart = scheduledStart
            });

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UnassignAsync(int dispatchId)
    {
        var response = await _http.DeleteAsync(
            $"api/dispatching-board/{dispatchId}");

        return response.IsSuccessStatusCode;
    }
}