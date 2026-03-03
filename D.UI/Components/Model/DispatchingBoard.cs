namespace D.UI.Models 
{
    public class DispatchingBoard
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public int WorkCenterId { get; set; }
        public int OperationId { get; set; }
        public int ActualQuantity { get; set; }
        public string Status { get; set; } = "Scheduled";
       
    }
}