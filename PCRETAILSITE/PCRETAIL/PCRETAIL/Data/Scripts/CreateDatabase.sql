USE [master];
GO

IF DB_ID(N'PCConfiguratorDB') IS NULL
BEGIN
    CREATE DATABASE [PCConfiguratorDB];
END;
GO

USE [PCConfiguratorDB];
GO

IF OBJECT_ID(N'[dbo].[Roles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Roles]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(MAX) NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Users]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Email] NVARCHAR(450) NOT NULL,
        [PasswordHash] NVARCHAR(MAX) NOT NULL,
        [RoleId] INT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Components]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Components]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(MAX) NOT NULL,
        [Type] NVARCHAR(MAX) NOT NULL,
        [Price] DECIMAL(18,2) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [ImagePath] NVARCHAR(260) NULL,
        CONSTRAINT [PK_Components] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END;
GO

IF OBJECT_ID(N'[dbo].[PcConfigurations]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PcConfigurations]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(MAX) NOT NULL,
        [Category] NVARCHAR(MAX) NOT NULL,
        [UserId] INT NOT NULL,
        [CreatedDate] DATETIME2 NOT NULL,
        [Status] NVARCHAR(MAX) NOT NULL,
        [ImagePath] NVARCHAR(260) NULL,
        CONSTRAINT [PK_PcConfigurations] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END;
GO

IF OBJECT_ID(N'[dbo].[Services]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Services]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Title] NVARCHAR(MAX) NOT NULL,
        [Description] NVARCHAR(MAX) NOT NULL,
        [Price] DECIMAL(18,2) NULL,
        [Status] NVARCHAR(MAX) NOT NULL,
        [CreatedDate] DATETIME2 NOT NULL,
        [ClientId] INT NOT NULL,
        [MasterId] INT NULL,
        CONSTRAINT [PK_Services] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END;
GO

IF OBJECT_ID(N'[dbo].[ConfigurationComponents]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ConfigurationComponents]
    (
        [ComponentsId] INT NOT NULL,
        [PcConfigurationsId] INT NOT NULL,
        CONSTRAINT [PK_ConfigurationComponents] PRIMARY KEY CLUSTERED ([ComponentsId] ASC, [PcConfigurationsId] ASC)
    );
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Users_Email'
      AND object_id = OBJECT_ID(N'[dbo].[Users]')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email]
        ON [dbo].[Users] ([Email] ASC);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Users_RoleId'
      AND object_id = OBJECT_ID(N'[dbo].[Users]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Users_RoleId]
        ON [dbo].[Users] ([RoleId] ASC);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_PcConfigurations_UserId'
      AND object_id = OBJECT_ID(N'[dbo].[PcConfigurations]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_PcConfigurations_UserId]
        ON [dbo].[PcConfigurations] ([UserId] ASC);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Services_ClientId'
      AND object_id = OBJECT_ID(N'[dbo].[Services]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Services_ClientId]
        ON [dbo].[Services] ([ClientId] ASC);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Services_MasterId'
      AND object_id = OBJECT_ID(N'[dbo].[Services]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Services_MasterId]
        ON [dbo].[Services] ([MasterId] ASC);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_ConfigurationComponents_PcConfigurationsId'
      AND object_id = OBJECT_ID(N'[dbo].[ConfigurationComponents]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ConfigurationComponents_PcConfigurationsId]
        ON [dbo].[ConfigurationComponents] ([PcConfigurationsId] ASC);
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Users_Roles_RoleId'
)
BEGIN
    ALTER TABLE [dbo].[Users] WITH CHECK
    ADD CONSTRAINT [FK_Users_Roles_RoleId]
        FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id])
        ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_PcConfigurations_Users_UserId'
)
BEGIN
    ALTER TABLE [dbo].[PcConfigurations] WITH CHECK
    ADD CONSTRAINT [FK_PcConfigurations_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
        ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Services_Users_ClientId'
)
BEGIN
    ALTER TABLE [dbo].[Services] WITH CHECK
    ADD CONSTRAINT [FK_Services_Users_ClientId]
        FOREIGN KEY ([ClientId]) REFERENCES [dbo].[Users] ([Id])
        ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_Services_Users_MasterId'
)
BEGIN
    ALTER TABLE [dbo].[Services] WITH CHECK
    ADD CONSTRAINT [FK_Services_Users_MasterId]
        FOREIGN KEY ([MasterId]) REFERENCES [dbo].[Users] ([Id])
        ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_ConfigurationComponents_Components_ComponentsId'
)
BEGIN
    ALTER TABLE [dbo].[ConfigurationComponents] WITH CHECK
    ADD CONSTRAINT [FK_ConfigurationComponents_Components_ComponentsId]
        FOREIGN KEY ([ComponentsId]) REFERENCES [dbo].[Components] ([Id])
        ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_ConfigurationComponents_PcConfigurations_PcConfigurationsId'
)
BEGIN
    ALTER TABLE [dbo].[ConfigurationComponents] WITH CHECK
    ADD CONSTRAINT [FK_ConfigurationComponents_PcConfigurations_PcConfigurationsId]
        FOREIGN KEY ([PcConfigurationsId]) REFERENCES [dbo].[PcConfigurations] ([Id])
        ON DELETE CASCADE;
END;
GO

SET IDENTITY_INSERT [dbo].[Roles] ON;

MERGE [dbo].[Roles] AS target
USING
(
    VALUES
        (1, N'Admin'),
        (2, N'Master'),
        (3, N'Client')
) AS source ([Id], [Name])
ON target.[Id] = source.[Id]
WHEN MATCHED THEN
    UPDATE SET [Name] = source.[Name]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Name]) VALUES (source.[Id], source.[Name]);

SET IDENTITY_INSERT [dbo].[Roles] OFF;
GO

INSERT INTO [dbo].[Users] ([Email], [PasswordHash], [RoleId], [CreatedAt])
VALUES
    (N'admin@example.com', N'hash_admin_123', 1, GETDATE()),
    (N'master1@example.com', N'hash_master_123', 2, GETDATE()),
    (N'master2@example.com', N'hash_master_456', 2, GETDATE()),
    (N'client1@example.com', N'hash_client_123', 3, GETDATE()),
    (N'client2@example.com', N'hash_client_456', 3, GETDATE()),
    (N'client3@example.com', N'hash_client_789', 3, GETDATE());
GO

INSERT INTO [dbo].[Components] ([Name], [Type], [Price], [CreatedAt], [ImagePath])
VALUES
    (N'Intel Core i5-12400F', N'CPU', 15000.00, GETDATE(), N'cpu_i5_12400f.jpg'),
    (N'Intel Core i7-12700K', N'CPU', 32000.00, GETDATE(), N'cpu_i7_12700k.jpg'),
    (N'AMD Ryzen 5 5600X', N'CPU', 18000.00, GETDATE(), N'cpu_ryzen_5_5600x.jpg'),
    (N'AMD Ryzen 7 5800X', N'CPU', 28000.00, GETDATE(), N'cpu_ryzen_7_5800x.jpg'),
    (N'NVIDIA RTX 3060 12GB', N'GPU', 35000.00, GETDATE(), N'gpu_rtx_3060.jpg'),
    (N'NVIDIA RTX 3070 8GB', N'GPU', 52000.00, GETDATE(), N'gpu_rtx_3070.jpg'),
    (N'AMD Radeon RX 6600', N'GPU', 28000.00, GETDATE(), N'gpu_rx_6600.jpg'),
    (N'AMD Radeon RX 6700 XT', N'GPU', 42000.00, GETDATE(), N'gpu_rx_6700xt.jpg'),
    (N'Kingston 16GB DDR4 3200MHz', N'RAM', 5500.00, GETDATE(), N'ram_kingston_16gb.jpg'),
    (N'Corsair 32GB DDR4 3600MHz', N'RAM', 11000.00, GETDATE(), N'ram_corsair_32gb.jpg'),
    (N'Samsung 16GB DDR5 4800MHz', N'RAM', 7500.00, GETDATE(), N'ram_samsung_16gb_ddr5.jpg'),
    (N'MSI B660 TOMAHAWK', N'Motherboard', 13000.00, GETDATE(), N'mb_msi_b660.jpg'),
    (N'ASUS ROG STRIX B550-F', N'Motherboard', 15000.00, GETDATE(), N'mb_asus_b550.jpg'),
    (N'Gigabyte Z690 AORUS', N'Motherboard', 22000.00, GETDATE(), N'mb_gigabyte_z690.jpg'),
    (N'Samsung 980 500GB NVMe', N'Storage', 5500.00, GETDATE(), N'ssd_samsung_500gb.jpg'),
    (N'WD Blue 1TB SSD', N'Storage', 7500.00, GETDATE(), N'ssd_wd_1tb.jpg'),
    (N'Seagate 2TB HDD', N'Storage', 5500.00, GETDATE(), N'hdd_seagate_2tb.jpg'),
    (N'Corsair 650W 80+ Bronze', N'PSU', 5500.00, GETDATE(), N'psu_corsair_650w.jpg'),
    (N'be quiet! 750W 80+ Gold', N'PSU', 8500.00, GETDATE(), N'psu_bequiet_750w.jpg'),
    (N'Zalman S2 ATX', N'Case', 3500.00, GETDATE(), N'case_zalman_s2.jpg'),
    (N'Corsair 4000D Airflow', N'Case', 8500.00, GETDATE(), N'case_corsair_4000d.jpg');
GO

INSERT INTO [dbo].[PcConfigurations] ([Name], [Category], [UserId], [CreatedDate], [Status], [ImagePath])
VALUES
    (N'Игровой ПК "Старт"', N'Gaming', 4, GETDATE(), N'Pending', N'pc_gaming_start.jpg'),
    (N'Игровой ПК "Оптима"', N'Gaming', 4, GETDATE(), N'InProgress', N'pc_gaming_optima.jpg'),
    (N'Рабочая станция для видео', N'Workstation', 5, GETDATE(), N'Completed', N'pc_workstation_video.jpg'),
    (N'Офисный ПК', N'Office', 5, GETDATE(), N'Pending', N'pc_office.jpg'),
    (N'Игровой ПК "Ультра"', N'Gaming', 6, GETDATE(), N'InProgress', N'pc_gaming_ultra.jpg'),
    (N'Домашний сервер', N'Server', 6, GETDATE(), N'Completed', N'pc_server_home.jpg');
GO

INSERT INTO [dbo].[Services] ([Title], [Description], [Price], [Status], [CreatedDate], [ClientId], [MasterId])
VALUES
    (N'Сборка игрового ПК', N'Требуется собрать игровой компьютер для современных игр', 5000.00, N'Pending', GETDATE(), 4, NULL),
    (N'Замена термопасты', N'Ноутбук греется, требуется замена термопасты', 1500.00, N'Assigned', DATEADD(day, -2, GETDATE()), 4, 2),
    (N'Диагностика неисправности', N'Компьютер не включается, требуется диагностика', 1000.00, N'InProgress', DATEADD(day, -1, GETDATE()), 5, 3),
    (N'Апгрейд ПК', N'Хочу улучшить производительность в играх', 3000.00, N'Pending', GETDATE(), 5, NULL),
    (N'Чистка от пыли', N'Системный блок сильно запылился', 2000.00, N'Completed', DATEADD(day, -5, GETDATE()), 6, 2),
    (N'Установка Windows', N'Требуется переустановить операционную систему', 1500.00, N'Completed', DATEADD(day, -3, GETDATE()), 6, 3),
    (N'Настройка BIOS', N'Проблемы с разгоном памяти', 1000.00, N'InProgress', DATEADD(day, -1, GETDATE()), 4, 3);
GO

INSERT INTO [dbo].[ConfigurationComponents] ([ComponentsId], [PcConfigurationsId])
VALUES
    (1, 1),
    (5, 1),
    (9, 1),
    (12, 1),
    (15, 1),
    (18, 1),
    (20, 1),
    (4, 2),
    (8, 2),
    (10, 2),
    (13, 2),
    (16, 2),
    (19, 2),
    (21, 2),
    (2, 3),
    (6, 3),
    (11, 3),
    (11, 3),
    (14, 3),
    (16, 3),
    (17, 3),
    (19, 3),
    (21, 3),
    (1, 4),
    (9, 4),
    (12, 4),
    (15, 4),
    (18, 4),
    (20, 4),
    (2, 5),
    (6, 5),
    (10, 5),
    (14, 5),
    (16, 5),
    (19, 5),
    (21, 5),
    (1, 6),
    (9, 6),
    (12, 6),
    (15, 6),
    (17, 6),
    (17, 6),
    (18, 6),
    (20, 6);
GO

SELECT 'Roles' AS TableName, COUNT(*) AS RecordCount FROM [dbo].[Roles]
UNION ALL
SELECT 'Users', COUNT(*) FROM [dbo].[Users]
UNION ALL
SELECT 'Components', COUNT(*) FROM [dbo].[Components]
UNION ALL
SELECT 'PcConfigurations', COUNT(*) FROM [dbo].[PcConfigurations]
UNION ALL
SELECT 'Services', COUNT(*) FROM [dbo].[Services]
UNION ALL
SELECT 'ConfigurationComponents', COUNT(*) FROM [dbo].[ConfigurationComponents];
GO
