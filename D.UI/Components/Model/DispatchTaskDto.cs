public class DispatchTaskDto
{
    public int Id { get; set; }

    public int WorkOrderId { get; set; }

    public int WorkCenterId { get; set; }

    public int StepOrder { get; set; }

    public DateTime? ScheduledStart { get; set; }

    public DateTime? ScheduledEnd { get; set; }

    public string Status { get; set; } = "Scheduled";

    public string? WorkOrderNo { get; set; }

    public string? WorkCenterName { get; set; }

    public int? ActualQuantity { get; set; }

    public DateTime? ActualStart { get; set; }

    public DateTime? ActualEnd { get; set; }
}