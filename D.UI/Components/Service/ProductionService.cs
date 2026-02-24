public class ProductionService
{
    private readonly HttpClient _http;
    public ProductionService(HttpClient http) => _http = http;

    public async Task<List<JobItem>> GetJobsAsync()
    {
        return await _http.GetFromJsonAsync<List<JobItem>>("api/production/jobs");
    }

    public async Task UpdateJobStatusAsync(string orderNo, string newStatus)
        => await _http.PutAsJsonAsync(
            $"api/production/update-status", 
            new { OrderNo = orderNo, Status = newStatus }
        );

    public async Task UpdateJobDetailAsync(JobItem job)
        => await _http.PutAsJsonAsync($"api/production/update-detail", job);
}