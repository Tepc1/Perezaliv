IF COL_LENGTH('dbo.Components', 'ImagePath') IS NULL
BEGIN
    ALTER TABLE dbo.Components
    ADD ImagePath NVARCHAR(260) NULL;
END;

UPDATE dbo.Components
SET ImagePath =
    CASE
        WHEN ImagePath IS NULL OR LTRIM(RTRIM(ImagePath)) = '' THEN NULL
        WHEN CHARINDEX('\', REPLACE(ImagePath, '/', '\')) > 0
            THEN RIGHT(
                REPLACE(ImagePath, '/', '\'),
                CHARINDEX('\', REVERSE(REPLACE(ImagePath, '/', '\'))) - 1
            )
        ELSE LTRIM(RTRIM(ImagePath))
    END
WHERE ImagePath IS NOT NULL;

IF COL_LENGTH('dbo.PcConfigurations', 'ImagePath') IS NOT NULL
BEGIN
    UPDATE dbo.PcConfigurations
    SET ImagePath =
        CASE
            WHEN ImagePath IS NULL OR LTRIM(RTRIM(ImagePath)) = '' THEN NULL
            WHEN CHARINDEX('\', REPLACE(ImagePath, '/', '\')) > 0
                THEN RIGHT(
                    REPLACE(ImagePath, '/', '\'),
                    CHARINDEX('\', REVERSE(REPLACE(ImagePath, '/', '\'))) - 1
                )
            ELSE LTRIM(RTRIM(ImagePath))
        END
    WHERE ImagePath IS NOT NULL;
END;
