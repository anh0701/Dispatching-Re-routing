using System.Data;
using Dapper;

public class DispatchingService
{
    private readonly IDbConnection _db;
    
    public DispatchingService (IDbConnection db)
    {
        _db = db;
    }

    public async Task<dynamic> GetDispatching()
    {
        string sql = "select * from DispatchingBoard";
        var data = await _db.QueryAsync(sql);
        return data;
    }

    public async Task<bool> AssignTaskToBoard(AssignmentRequest req)
    {
        string sqlInfo = @"
            SELECT rc.CycleTime, rc.SetupTime, wo.Quantity 
            FROM ResourceCapabilities rc
            JOIN WorkOrders wo ON wo.Id = @WorkOrderId
            WHERE rc.WorkCenterId = @WorkCenterId AND rc.OperationId = @OperationId";

        var info = await _db.QueryFirstOrDefaultAsync<ResourceInfo>(sqlInfo, new { 
            req.WorkOrderId, req.WorkCenterId, req.OperationId 
        });

        if (info == null) throw new Exception("Máy không hỗ trợ công đoạn này!");

        double totalProductionSeconds = info.Quantity * info.CycleTime;
        DateTime scheduledEnd = req.ScheduledStart
                                    .AddMinutes(info.SetupTime)
                                    .AddSeconds(totalProductionSeconds);

        string sqlInsert = @"
            INSERT INTO DispatchingBoard (
                WorkOrderId, WorkCenterId, OperationId, StepOrder, 
                PlannedQuantity, ScheduledStart, ScheduledEnd, Status
            ) 
            VALUES (
                @WorkOrderId, @WorkCenterId, @OperationId, @StepOrder, 
                @Quantity, @ScheduledStart, @ScheduledEnd, 'Scheduled'
            )";

        var result = await _db.ExecuteAsync(sqlInsert, new {
            req.WorkOrderId,
            req.WorkCenterId,
            req.OperationId,
            req.StepOrder,
            info.Quantity,
            req.ScheduledStart,
            ScheduledEnd = scheduledEnd
        });
        
        return result > 0;
    }
}