IF COL_LENGTH('dbo.PcConfigurations', 'Status') IS NULL
BEGIN
    ALTER TABLE dbo.PcConfigurations
    ADD Status NVARCHAR(50) NOT NULL
        CONSTRAINT DF_PcConfigurations_Status DEFAULT N'Pending';
END;

UPDATE dbo.PcConfigurations
SET Status =
    CASE
        WHEN Status IN (N'InProgress', N'Completed') THEN Status
        ELSE N'Pending'
    END;
