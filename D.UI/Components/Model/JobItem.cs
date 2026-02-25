public class JobItem
{
    public int Id { get; set; }

    public string OrderNo { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public DateTime? DueDate { get; set; }

    public WorkOrderStatus Status { get; set; }

    // ===== UI-only (không map DB) =====
    public string Color => Status switch
    {
        WorkOrderStatus.Pool => "#95a5a6",
        WorkOrderStatus.Scheduled => "#3498db",
        WorkOrderStatus.Processing => "#f39c12",
        WorkOrderStatus.Completed => "#2ecc71",
        _ => "#bdc3c7"
    };
}
