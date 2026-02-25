USE DISPATCHING;
GO

-- Thêm máy móc
INSERT INTO WorkCenters (Code, Name, Status) VALUES 
('MC-001', 'Máy Hàn Robot 01', 1),
('MC-002', 'Máy Hàn Tay 02', 1),
('MC-003', 'Máy Cắt Laser', 1);

GO

-- Thêm công đoạn
INSERT INTO Operations (OpCode, Description) VALUES 
('CUT', 'Cắt phôi'),
('WELD', 'Hàn cấu kiện');
GO

-- Cấu hình năng lực (Máy 1 hàn nhanh hơn máy 2)
INSERT INTO ResourceCapabilities (WorkCenterId, OperationId, CycleTime, SetupTime) VALUES 
(3, 1, 10, 5), -- Máy Cắt làm bước Cắt (10s/sp)
(1, 2, 30, 15), -- Máy Hàn 01 làm bước Hàn (30s/sp)
(2, 2, 50, 10); -- Máy Hàn 02 làm bước Hàn (50s/sp - chậm hơn)
GO

-- Thêm sản phẩm và quy trình
INSERT INTO Products (SKU, ProductName) VALUES ('BIKE-01', 'Khung xe đạp');
GO

INSERT INTO Products (SKU, ProductName) VALUES ('BIKE-02', 'Khung xe máy');
GO

INSERT INTO ProductionRoutes (ProductId, RouteName) VALUES (1, 'Quy trình chuẩn khung xe');
GO

INSERT INTO ProductionRoutes (ProductId, RouteName) VALUES (2, 'Quy trình chuẩn khung xe');
GO

INSERT INTO RouteSteps (RouteId, StepOrder, OperationId) VALUES (1, 1, 1), (1, 2, 2);
GO

INSERT INTO RouteSteps (RouteId, StepOrder, OperationId) VALUES (2, 1, 1), (2, 2, 2);
GO


-- Tạo 1 lệnh sản xuất 100 cái khung xe
INSERT INTO WorkOrders (OrderNo, ProductId, Quantity, Priority, DueDate) 
VALUES ('WO-2024-001', 1, 100, 10, '2024-12-31');
GO

INSERT INTO WorkOrders (OrderNo, ProductId, Quantity, Priority, DueDate) 
VALUES ('WO-2024-002', 2, 100, 10, '2024-12-31');
GO