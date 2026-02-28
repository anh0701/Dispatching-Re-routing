using System.Data;
using Dapper;

public class DispatchingService
{
    private readonly IDbConnection _db;

    public DispatchingService(IDbConnection db)
    {
        _db = db;
    }

    public async Task<dynamic> GetDispatching()
    {
        string sql = "select * from DispatchingBoard";
        var data = await _db.QueryAsync(sql);
        return data;
    }

    public async Task<(bool ok, string? error)> AssignTaskToBoard(AssignmentRequest req)
    {
        string sqlInfo = @"
            SELECT rc.CycleTime, rc.SetupTime, wo.Quantity 
            FROM ResourceCapabilities rc
            JOIN WorkOrders wo ON wo.Id = @WorkOrderId
            WHERE rc.WorkCenterId = @WorkCenterId AND rc.OperationId = @OperationId";

        var info = await _db.QueryFirstOrDefaultAsync<ResourceInfo>(sqlInfo, new
        {
            req.WorkOrderId,
            req.WorkCenterId,
            req.OperationId
        });

        if (info == null) return (false, "Máy không hỗ trợ công đoạn này!");

        double totalProductionSeconds = info.Quantity * info.CycleTime;
        DateTime scheduledEnd = req.ScheduledStart
                                    .AddMinutes(info.SetupTime)
                                    .AddSeconds(totalProductionSeconds);
                                    
        var hasConflict = await _db.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT TOP 1 
                wc.Name AS MachineName,
                db.ScheduledStart,
                db.ScheduledEnd
            FROM DispatchingBoard db
            JOIN WorkCenters wc ON wc.Id = db.WorkCenterId
            WHERE db.WorkCenterId = @WorkCenterId
            AND db.Status IN ('Scheduled','Processing')
            AND @Start < db.ScheduledEnd
            AND @End   > db.ScheduledStart
            ORDER BY db.ScheduledStart
        ", new {
            req.WorkCenterId,
            Start = req.ScheduledStart,
            End = scheduledEnd
        }) > 0;

        if (hasConflict != null)
            return (
                false, 
                $"Máy {hasConflict.MachineName} đang bận từ " +
                $"{hasConflict.ScheduledStart:HH:mm} đến {hasConflict.ScheduledEnd:HH:mm}" +
                $"Không thể xếp tại {req.ScheduledStart:HH:mm}"
            );

        string sqlInsert = @"
            INSERT INTO DispatchingBoard (
                WorkOrderId, WorkCenterId, OperationId, StepOrder, 
                PlannedQuantity, ScheduledStart, ScheduledEnd, Status
            ) 
            VALUES (
                @WorkOrderId, @WorkCenterId, @OperationId, @StepOrder, 
                @Quantity, @ScheduledStart, @ScheduledEnd, 'Scheduled'
            )";

        var result = await _db.ExecuteAsync(sqlInsert, new
        {
            req.WorkOrderId,
            req.WorkCenterId,
            req.OperationId,
            req.StepOrder,
            info.Quantity,
            req.ScheduledStart,
            ScheduledEnd = scheduledEnd
        });

        return result > 0
            ? (true, null)
            : (false, "Không thể ghi vào DispatchingBoard");
    }

    public async Task<IEnumerable<DispatchingTaskDto>> GetBoard(DateTime date)
    {
        string sql = @"
        SELECT 
            db.WorkOrderId,
            db.WorkCenterId,
            db.OperationId,
            db.ScheduledStart,
            db.ScheduledEnd,
            db.Status,
            wo.OrderNo AS WorkOrderNo,
            wc.Name AS WorkCenterName
        FROM DispatchingBoard db
        JOIN WorkOrders wo ON wo.Id = db.WorkOrderId
        JOIN WorkCenters wc ON wc.Id = db.WorkCenterId
        WHERE CAST(db.ScheduledStart AS DATE) = CAST(@Date AS DATE)
        ORDER BY wc.Id, db.ScheduledStart
    ";

        var data = await _db.QueryAsync<DispatchingTaskDto>(sql, new { Date = date });
        return data;
    }
}