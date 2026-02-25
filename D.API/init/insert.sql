USE DISPATCHING;
GO

-- 2. THÊM MÁY MÓC (WorkCenters)
INSERT INTO WorkCenters (Code, Name, Status) VALUES 
('MC-001', 'Máy Hàn Robot 01', 1), -- ID 1
('MC-002', 'Máy Hàn Tay 02', 1),   -- ID 2
('MC-003', 'Máy Cắt Laser', 1),    -- ID 3
('WC-TEST-02', 'Buồng Kiểm Tra', 2),-- ID 4
('WC-PACK-03', 'Máy Đóng Gói', 3);  -- ID 5
GO

-- 3. THÊM CÔNG ĐOẠN (Operations)
INSERT INTO Operations (OpCode, Description) VALUES 
('CUT', 'Cắt phôi'),             -- ID 1
('WELD', 'Hàn cấu kiện'),        -- ID 2
('OP-TEST', 'Kiểm tra chức năng'),-- ID 3
('OP-PACK', 'Đóng gói');          -- ID 4
GO

INSERT INTO ResourceCapabilities (WorkCenterId, OperationId, CycleTime, SetupTime) VALUES 
(3, 1, 10, 5),  -- Máy Cắt (3) làm CUT (1)
(1, 2, 30, 15), -- Máy Hàn 01 (1) làm WELD (2)
(2, 2, 50, 10), -- Máy Hàn 02 (2) làm WELD (2)
(4, 3, 60, 10), -- Buồng Test (4) làm TEST (3)
(5, 4, 15, 5);  -- Máy Gói (5) làm PACK (4)
GO

-- 5. SẢN PHẨM & QUY TRÌNH
INSERT INTO Products (SKU, ProductName) VALUES 
('BIKE-01', 'Khung xe đạp'),       -- ID 1
('IP17-PRO', 'iPhone 17 Pro');     -- ID 2

INSERT INTO ProductionRoutes (ProductId, RouteName, IsDefault) VALUES 
(1, 'Quy trình khung xe', 1),      -- ID 1
(2, 'Quy trình iPhone', 1);        -- ID 2

INSERT INTO RouteSteps (RouteId, StepOrder, OperationId) VALUES 
(1, 1, 1), (1, 2, 2),              -- Bike: Cắt -> Hàn
(2, 1, 3), (2, 2, 4);              -- iPhone: Test -> Pack
GO

-- 6. LỆNH SẢN XUẤT (WorkOrders)
INSERT INTO WorkOrders (OrderNo, ProductId, Quantity, Priority, DueDate, Status) VALUES 
('WO-2026-001', 1, 100, 10, '2026-12-31', 1), -- ID 1
('WO-2026-002', 2, 500, 5, '2026-03-15', 0);  -- ID 2
GO

-- Trạng thái 'Completed'
INSERT INTO DispatchingBoard 
(WorkOrderId, WorkCenterId, OperationId, StepOrder, PlannedQuantity, ActualQuantity, ScheduledStart, ScheduledEnd, Status)
VALUES 
(1, 3, 1, 1, 100, 100, '2026-02-25 08:00', '2026-02-25 10:00', 'Completed');

-- Trạng thái 'Processing'
INSERT INTO DispatchingBoard 
(WorkOrderId, WorkCenterId, OperationId, StepOrder, PlannedQuantity, ActualQuantity, Status)
VALUES 
(1, 1, 2, 2, 100, 45, 'Processing');

-- Trạng thái 'Breakdown_Hold'
INSERT INTO DispatchingBoard 
(WorkOrderId, WorkCenterId, OperationId, StepOrder, PlannedQuantity, ActualQuantity, Status, Note)
VALUES 
(2, 4, 3, 1, 500, 120, 'Breakdown_Hold', 'Lỗi cảm biến nhiệt độ');

-- Trạng thái 'Scheduled'
INSERT INTO DispatchingBoard 
(WorkOrderId, WorkCenterId, OperationId, StepOrder, PlannedQuantity, Status)
VALUES 
(2, 5, 4, 2, 500, 'Scheduled');
GO

-- (WO-2024-001 (ID 1) là Status 1, WO-2024-002 (ID 2) là Status 0)

INSERT INTO WorkOrders (OrderNo, ProductId, Quantity, Priority, DueDate, Status) VALUES 
('WO-2026-003', 1, 200, 3, '2026-01-15', 2), -- ID 3: Status 2 (Completed) - Đã hoàn thành xong
('WO-2026-004', 2, 50, 1, '2026-05-20', 3);  -- ID 4: Status 3 (Cancelled) - Đã bị hủy
GO

-- Case cho WO-2026-003 (Đã hoàn thành toàn bộ các bước)
INSERT INTO DispatchingBoard 
(WorkOrderId, WorkCenterId, OperationId, StepOrder, PlannedQuantity, ActualQuantity, Status, Note)
VALUES 
(3, 3, 1, 1, 200, 200, 'Completed', 'Bước 1 xong'),
(3, 1, 2, 2, 200, 200, 'Completed', 'Bước 2 xong - Đơn hàng hoàn tất');

-- Case cho WO-2026-004 (Bị hủy)
-- INSERT INTO DispatchingBoard 
-- (WorkOrderId, WorkCenterId, OperationId, StepOrder, PlannedQuantity, Status, Note)
-- VALUES 
-- (4, 4, 3, 1, 50, 'Breakdown_Hold', 'Dừng điều độ do đơn hàng bị hủy');
GO