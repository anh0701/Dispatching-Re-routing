public class RouteStepDto
{
    public int StepOrder { get; set; }

    public string StepName { get; set; } = string.Empty;

    public List<AvailableMachineDto> AvailableMachines { get; set; } = new();
}