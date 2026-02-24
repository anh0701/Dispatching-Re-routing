CREATE DATABASE DISPATCHING;
GO

USE DISPATCHING;
GO

CREATE TABLE WorkCenters (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Code NVARCHAR(50) UNIQUE NOT NULL,
    Name NVARCHAR(255),
    -- Status: 0: Offline, 1: Running, 2: Breakdown, 3: Maintenance
    Status INT DEFAULT 1, 
    LastMaintenance DATETIME,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE Operations (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OpCode NVARCHAR(20) UNIQUE NOT NULL,
    Description NVARCHAR(255)
);
GO

CREATE TABLE ResourceCapabilities (
    WorkCenterId INT NOT NULL,
    OperationId INT NOT NULL,
    CycleTime FLOAT NOT NULL, -- Thời gian sản xuất 1 sản phẩm (giây)
    SetupTime INT DEFAULT 0,  -- Thời gian chuẩn bị máy (phút)
    PRIMARY KEY (WorkCenterId, OperationId),
    FOREIGN KEY (WorkCenterId) REFERENCES WorkCenters(Id),
    FOREIGN KEY (OperationId) REFERENCES Operations(Id)
);
GO

CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SKU NVARCHAR(50) UNIQUE NOT NULL,
    ProductName NVARCHAR(255)
);
GO

CREATE TABLE ProductionRoutes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT,
    RouteName NVARCHAR(100),
    IsDefault BIT DEFAULT 1,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
GO

CREATE TABLE RouteSteps (
    Id INT PRIMARY KEY IDENTITY(1,1),
    RouteId INT,
    StepOrder INT NOT NULL, -- Thứ tự bước 1, 2, 3...
    OperationId INT,
    FOREIGN KEY (RouteId) REFERENCES ProductionRoutes(Id),
    FOREIGN KEY (OperationId) REFERENCES Operations(Id)
);
GO

CREATE TABLE WorkOrders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderNo NVARCHAR(50) UNIQUE NOT NULL,
    ProductId INT,
    Quantity INT NOT NULL,
    Priority INT DEFAULT 5, -- 1: Thấp, 10: Rất cao
    DueDate DATETIME,
    -- Status: 0: Planned, 1: In-Progress, 2: Completed, 3: Cancelled
    Status INT DEFAULT 0,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
GO

CREATE TABLE DispatchingBoard (
    Id INT PRIMARY KEY IDENTITY(1,1),
    WorkOrderId INT,
    WorkCenterId INT,
    OperationId INT,
    StepOrder INT,
    PlannedQuantity INT,
    ActualQuantity INT DEFAULT 0,
    ScheduledStart DATETIME,
    ScheduledEnd DATETIME,
    ActualStart DATETIME,
    ActualEnd DATETIME,
    -- Status: 'Scheduled', 'Processing', 'Breakdown_Hold', 'Completed'
    Status NVARCHAR(50) DEFAULT 'Scheduled',
    Note NVARCHAR(MAX), 
    
    FOREIGN KEY (WorkOrderId) REFERENCES WorkOrders(Id),
    FOREIGN KEY (WorkCenterId, OperationId) REFERENCES ResourceCapabilities(WorkCenterId, OperationId)
);
GO


-- Thêm máy móc
INSERT INTO WorkCenters (Code, Name, Status) VALUES 
('MC-001', 'Máy Hàn Robot 01', 1),
('MC-002', 'Máy Hàn Tay 02', 1),
('MC-003', 'Máy Cắt Laser', 1);

-- Thêm công đoạn
INSERT INTO Operations (OpCode, Description) VALUES 
('CUT', 'Cắt phôi'),
('WELD', 'Hàn cấu kiện');

-- Cấu hình năng lực (Máy 1 hàn nhanh hơn máy 2)
INSERT INTO ResourceCapabilities (WorkCenterId, OperationId, CycleTime, SetupTime) VALUES 
(3, 1, 10, 5), -- Máy Cắt làm bước Cắt (10s/sp)
(1, 2, 30, 15), -- Máy Hàn 01 làm bước Hàn (30s/sp)
(2, 2, 50, 10); -- Máy Hàn 02 làm bước Hàn (50s/sp - chậm hơn)

-- Thêm sản phẩm và quy trình
INSERT INTO Products (SKU, ProductName) VALUES ('BIKE-01', 'Khung xe đạp');
INSERT INTO ProductionRoutes (ProductId, RouteName) VALUES (1, 'Quy trình chuẩn khung xe');
INSERT INTO RouteSteps (RouteId, StepOrder, OperationId) VALUES (1, 1, 1), (1, 2, 2);

-- Tạo 1 lệnh sản xuất 100 cái khung xe
INSERT INTO WorkOrders (OrderNo, ProductId, Quantity, Priority, DueDate) 
VALUES ('WO-2024-001', 1, 100, 10, '2024-12-31');