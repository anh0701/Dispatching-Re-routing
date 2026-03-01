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
    WorkOrderId INT NOT NULL,
    WorkCenterId INT NOT NULL,
    OperationId INT NOT NULL,
    StepOrder INT,
    Sequence INT DEFAULT 0,
    PlannedQuantity INT,
    ActualQuantity INT DEFAULT 0,
    OriginalStart DATETIME,
    ScheduledStart DATETIME NOT NULL,
    ScheduledEnd DATETIME NOT NULL,
    ActualStart DATETIME,
    ActualEnd DATETIME,
    -- Status: 'Scheduled', 'Processing', 'Breakdown_Hold', 'Completed'
    Status NVARCHAR(50) DEFAULT 'Scheduled',
    Note NVARCHAR(MAX), 
    Version INT DEFAULT 1,
    
    FOREIGN KEY (WorkOrderId) REFERENCES WorkOrders(Id),
    FOREIGN KEY (WorkCenterId) REFERENCES WorkCenters(Id),
    FOREIGN KEY (OperationId) REFERENCES Operations(Id)
);
GO

CREATE INDEX IX_Dispatching_Schedule ON DispatchingBoard (WorkCenterId, ScheduledStart, ScheduledEnd);
GO

ALTER TABLE DispatchingBoard ADD
    AssignedBy NVARCHAR(100),
    AssignedAt DATETIME DEFAULT GETDATE(),
    LastUpdatedAt DATETIME,
    LastUpdatedBy NVARCHAR(100);
GO

CREATE TABLE WorkCenterStatusLog (
    Id INT IDENTITY PRIMARY KEY,
    WorkCenterId INT,
    Status INT,
    Reason NVARCHAR(255),
    StartTime DATETIME,
    EndTime DATETIME NULL,
    FOREIGN KEY (WorkCenterId) REFERENCES WorkCenters(Id)
);

CREATE TABLE HoldReasons (
    Code NVARCHAR(50) PRIMARY KEY,
    Description NVARCHAR(255)
);

ALTER TABLE DispatchingBoard ADD HoldReasonCode NVARCHAR(50);


