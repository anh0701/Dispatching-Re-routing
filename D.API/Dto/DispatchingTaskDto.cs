public class DispatchingTaskDto
{
    public int WorkOrderId { get; set; }
    public int WorkCenterId { get; set; }
    public int OperationId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Status { get; set; } = "";
    public string WorkOrderNo { get; set; } = "";
    public string WorkCenterName { get; set; } = "";
}