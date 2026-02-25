USE DISPATCHING;
GO

-- select * from WorkOrders -- where Status = 0

-- select * from Operations

-- select * from ResourceCapabilities

-- select * from Products

-- select * from ProductionRoutes

-- select * from RouteSteps

-- select * from WorkOrders

-- select * from DispatchingBoard

-- SELECT 
--     W.OrderNo, 
--     CASE W.Status 
--         WHEN 0 THEN 'Planned' WHEN 1 THEN 'In-Progress' 
--         WHEN 2 THEN 'Completed' WHEN 3 THEN 'Cancelled' 
--     END AS WO_Status,
--     D.StepOrder,
--     O.OpCode,
--     D.Status AS Dispatch_Status,
--     D.ActualQuantity
-- FROM WorkOrders W
-- LEFT JOIN DispatchingBoard D ON W.Id = D.WorkOrderId
-- LEFT JOIN Operations O ON D.OperationId = O.Id
-- ORDER BY W.Id;

-- SELECT *
-- FROM WorkOrderRoutes
-- WHERE WorkOrderId = 2

-- SELECT *
-- FROM ResourceCapabilities
-- WHERE OperationId IN (3,4);

-- SELECT *
-- FROM ResourceCapabilities rc
-- JOIN WorkCenters wc ON rc.WorkCenterId = wc.Id
-- WHERE rc.OperationId IN (3,4)
--   AND wc.Status = 1;

-- SELECT Id, Code, Status
-- FROM WorkCenters
-- WHERE Id IN (4,5)

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
-- WHERE CAST(db.ScheduledStart AS DATE) = CAST(@Date AS DATE)
ORDER BY wc.Id, db.ScheduledStart

GO