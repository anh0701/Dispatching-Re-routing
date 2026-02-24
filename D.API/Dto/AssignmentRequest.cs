public class AssignmentRequest
{
    public int WorkOrderId { get; set; }
    public int WorkCenterId { get; set; }
    public int OperationId { get; set; }
    public int StepOrder { get; set; }
    public DateTime ScheduledStart { get; set; }
}

public class ResourceInfo
{
    public float CycleTime { get; set; } // Giây/Sản phẩm
    public int SetupTime { get; set; }   // Phút
    public int Quantity { get; set; }    // Số lượng từ WorkOrder
}