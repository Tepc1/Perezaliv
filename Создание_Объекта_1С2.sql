BEGIN TRANSACTION;

DECLARE @Код NVARCHAR(50) = N'000000003';

-- Код должен быть уникальным, иначе импорт в 1С даст ошибку дубликата
IF EXISTS (SELECT 1 FROM [Справочник_lol] WHERE [Код] = @Код)
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR(N'Элемент с кодом %s уже есть', 16, 1, @Код);
    RETURN;
END

INSERT INTO [Справочник_lol]
    ([Ссылка_GUID], [Код], [Наименование], [Количество], [Цена], [ПутьККартинке])
VALUES
    (LOWER(CONVERT(NVARCHAR(36), NEWID())),
     @Код,
     N'intel core i9',
     CAST(15 AS DECIMAL(19,4)),
     CAST(25 AS DECIMAL(19,4)),
     N'/files/i9.jpg');

-- Проверка: путь должен быть без нулевых байтов (DATALENGTH = 2 * длина строки)
SELECT [Ссылка_GUID], [Код], [Наименование], [ПутьККартинке],
       DATALENGTH([ПутьККартинке]) AS Байт,
       LEN([ПутьККартинке]) * 2 AS Ожидаемо
FROM [Справочник_lol]
WHERE [Код] = @Код;

COMMIT TRANSACTION;