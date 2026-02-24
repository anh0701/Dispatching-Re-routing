using System.Data;
using Dapper;

public class WorkOrdersService
{
    private readonly IDbConnection _db;
    
    public WorkOrdersService (IDbConnection db)
    {
        _db = db;
    }

    public async Task<dynamic> GetPlanned()
    {
        string sql = "select * from WorkOrders where Status = 0";
        var data = await _db.QueryAsync(sql);
        return data;
    }

    public async Task<dynamic> GetRouteStep(int id)
    {
        var sql = """
            SELECT 
                wo.Id AS WorkOrderId, wo.OrderNo, p.ProductName,
                rs.StepOrder, o.Id AS OperationId, o.OpCode, o.Description,
                wc.Id AS WorkCenterId, wc.Code AS WorkCenterCode, wc.Name AS WorkCenterName,
                rc.CycleTime, rc.SetupTime
            FROM WorkOrders wo
            JOIN Products p ON wo.ProductId = p.Id
            JOIN ProductionRoutes pr ON p.Id = pr.ProductId AND pr.IsDefault = 1
            JOIN RouteSteps rs ON pr.Id = rs.RouteId
            JOIN Operations o ON rs.OperationId = o.Id
            LEFT JOIN ResourceCapabilities rc ON o.Id = rc.OperationId
            LEFT JOIN WorkCenters wc ON rc.WorkCenterId = wc.Id
            WHERE wo.Id = @Id and wc.Status = 1
            ORDER BY rs.StepOrder, rc.CycleTime ASC 
        """;
        var flatData = await _db.QueryAsync(sql, new { Id = id });

        if (!flatData.Any()) return null;

        var first = flatData.First();
        var result = new WorkOrderRouteDto
        {
            WorkOrderId = first.WorkOrderId,
            OrderNo = first.OrderNo,
            ProductName = first.ProductName,
            Steps = flatData
                .GroupBy(x => new { x.StepOrder, x.OperationId, x.OpCode, x.Description })
                .Select(g => new RouteStepDto
                {
                    StepOrder = g.Key.StepOrder,
                    OperationId = g.Key.OperationId,
                    OpCode = g.Key.OpCode,
                    Description = g.Key.Description,
                    AvailableMachines = g
                        .Where(x => x.WorkCenterId != null) // Lọc bỏ nếu bước đó chưa gán máy nào
                        .Select(m => new MachineOptionDto
                        {
                            WorkCenterId = m.WorkCenterId,
                            WorkCenterCode = m.WorkCenterCode,
                            WorkCenterName = m.WorkCenterName,
                            CycleTime = (float)m.CycleTime,
                            SetupTime = m.SetupTime
                        }).ToList()
                }).ToList()
        };

        return result;
    }

}