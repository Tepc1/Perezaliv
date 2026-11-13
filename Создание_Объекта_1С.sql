INSERT INTO [Справочник_lol]
    ([Ссылка_GUID], [Код], [Наименование], [Количество], [Цена], [ПутьККартинке])
VALUES
    (LOWER(CONVERT(NVARCHAR(36), NEWID())), N'000000002', N'tвфывфыest', 15, 25, N'/files/tesвывt.jpg');