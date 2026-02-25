public class WorkOrderRouteDto
{
    public int WorkOrderId { get; set; }

    public List<RouteStepDto> Steps { get; set; } = new();
}