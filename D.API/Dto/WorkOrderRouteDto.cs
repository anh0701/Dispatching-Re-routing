public class WorkOrderRouteDto
{
    public int WorkOrderId { get; set; }
    public string OrderNo { get; set; }
    public string ProductName { get; set; }
    public List<RouteStepDto> Steps { get; set; } = new List<RouteStepDto>();
}