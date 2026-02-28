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
                wo.Id AS WorkOrderId, 
                wo.OrderNo, 
                p.ProductName,
                rs.StepOrder, 
                o.Id AS OperationId, 
                o.OpCode, 
                o.Description,
                wc.Id AS WorkCenterId, 
                wc.Code AS WorkCenterCode, 
                wc.Name AS WorkCenterName,
                rc.CycleTime, 
                rc.SetupTime
            FROM WorkOrders wo
            JOIN Products p ON wo.ProductId = p.Id
            JOIN ProductionRoutes pr ON p.Id = pr.ProductId AND pr.IsDefault = 1
            JOIN RouteSteps rs ON pr.Id = rs.RouteId
            JOIN Operations o ON rs.OperationId = o.Id
            LEFT JOIN ResourceCapabilities rc ON o.Id = rc.OperationId
            LEFT JOIN WorkCenters wc 
                ON rc.WorkCenterId = wc.Id 
                AND wc.Status = 1
            WHERE wo.Id = 3
            ORDER BY rs.StepOrder, rc.CycleTime ASC;

GO