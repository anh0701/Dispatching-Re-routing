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

    public async Task<AssignTaskResult> AssignTaskToBoard(AssignmentRequest req)
    {
        if (_db.State != ConnectionState.Open) _db.Open();
        using var tran = _db.BeginTransaction();
        try
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
            }, tran);

            if (info == null) 
            {
                tran.Rollback();
                return new AssignTaskResult{
                    Ok = false, 
                    Error = "Máy không hỗ trợ công đoạn này!"
                };
            }

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
            }, tran);

            if (hasConflict != null)
            {
                tran.Rollback();
                return new AssignTaskResult{
                    Ok = false, 
                    Error = $"Máy {hasConflict.MachineName} đang bận từ " +
                    $"{hasConflict.ScheduledStart:HH:mm} đến {hasConflict.ScheduledEnd:HH:mm}. " +
                    $"Không thể xếp tại {req.ScheduledStart:HH:mm}"
                };
            }

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
            }, tran);

            tran.Commit();

            return result > 0
                ? new AssignTaskResult{ Ok = true, Error = null}
                : new AssignTaskResult{ Ok = false, Error = "Không thể ghi vào DispatchingBoard"};
        }
        catch (Exception ex)
        {
            tran.Rollback();
            return new AssignTaskResult
            {
                Ok = false,
                Error = ex.Message
            };
        }
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

    public async Task<dynamic> Reschedule()
    {
        string sql = """
        
        """;

        var data = await _db.ExecuteAsync(sql);
        return data;
    }

    public async Task<bool> CancelJob (int id)
    {
        string sql = """
            // UPDATE WorkOrders SET Status = 0 WHERE WorkOrderId = @id;
            UPDATE DispatchingBoard 
            SET Status = 'Scheduled' 
            WHERE WorkOrderId = @id;
        """;
        var data = await _db.ExecuteAsync(sql, new { id }) > 0;
        return data;
    }

    public async Task<bool> ChangeProcessing(int id)
    {
        string sql = """
            UPDATE DispatchingBoard 
            SET 
                Status = 'Processing',
                ActualStart =  GETDATE()
            WHERE WorkOrderId = @id;
        """;
        var data = await _db.ExecuteAsync(sql, new {
            WorkOrderId = id
        }) > 0;
        return data;
    }

    public async Task<bool> UpdateProgress (int id, GetProgressReq req)
    {
        var sqlSelect = """
            SELECT ActualQuantity
            FROM DispatchingBoard
            WHERE WorkOrderId = @id
        """;

        var selectRes = await _db.QueryFirstOrDefaultAsync<dynamic>(sqlSelect, new
        {
            WorkOrderId = id
        });
        // int actualQuantity = selectRes?.ActualQuantity + req.ActualQuantity;
        var sql = """
            UPDATE DispatchingBoard 
            SET ActualQuantity = ISNULL(ActualQuantity, 0) + @increment
            WHERE WorkOrderId = @id
        """;
        var data = await _db.ExecuteAsync(sql, new
        {
            increment = selectRes?.ActualQuantity,
            WorkOrderId = id   
        }) > 0;
        return data;
    }

    public async Task<bool> ChangeComplete (int id)
    {
        var sql = """
            UPDATE DispatchingBoard
            SET Status = 'Completed',
            ActualEnd = GETDATE()
            WHERE Id = @id;
        """;
        var data = await _db.ExecuteAsync(sql, new { id }) > 0;

        var sqlMaxStep = """
        SELECT MAX(rs.StepOrder) AS MaxStep
        FROM RouteSteps rs
        JOIN ProductionRoutes pr ON rs.RouteId = pr.Id
        JOIN WorkOrders wo ON wo.ProductId = pr.ProductId
        WHERE wo.Id = @id
        AND pr.IsDefault = 1;
        """;
        var maxStep = await _db.QueryFirstOrDefaultAsync<dynamic>(sqlMaxStep, new { id });

        var sqlCurrentStep = """
            SELECT StepOrder
            FROM DispatchingBoard
            WHERE WorkOrderId = @id
        """;

        var currentStep = await _db.QueryFirstOrDefaultAsync<dynamic>(sqlCurrentStep, new { id });
        
        if (maxStep?.MaxStep == currentStep?.StepOrder)
        {
            var sqlMax = """
            UPDATE WorkOrders
            SET Status = 2 -- Completed
            WHERE Id = @id;
            """;
            var res = await _db.ExecuteAsync(sqlMax, new { id }) > 0;
            return res;
        }
        return data;
    }
}