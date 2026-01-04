-- Создание представления vw_Users в базе данных тенанта
-- Представление ссылается на таблицу Users в той же базе данных
CREATE OR REPLACE VIEW `vw_Users` AS
SELECT 
    `Id`,
    `Email`,
    `Name`,
    `Status`,
    `UpdatedAt`
FROM `Users`;

