using Microsoft.AspNetCore.SignalR;

public class AvailableMachineDto
{
    public int workCenterId { get; set; }

    public string workCenterCode {get; set;}

    public string workCenterName { get; set; } = string.Empty;

    public double CycleTime { get; set; } 

    public int setupTime {get; set;}
}