public class RouteStepDto
{
    public int StepOrder { get; set; }
    public int OperationId { get; set; }
    public string OpCode { get; set; }
    public string Description { get; set; }
    // Danh sách các máy có khả năng thực hiện công đoạn này
    public List<MachineOptionDto> AvailableMachines { get; set; } = new List<MachineOptionDto>();
}