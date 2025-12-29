-- Скрипт для заполнения дефолтными данными (только для локальной разработки)
-- Выполняется автоматически при первом запуске MySQL контейнера, если SEED_DATA=true
-- 
-- ВАЖНО: Этот скрипт выполняется только при первом запуске MySQL (когда том mysql_data пуст)
-- Для повторного сидинга данных используйте отдельный скрипт или API

-- Используем NimbusSite_Tenants БД
USE NimbusSite_Tenants;

-- Создаем таблицу Tenants, если её нет (на случай, если миграции еще не применены)
-- Примечание: таблица Tenants находится в корне БД без схемы
CREATE TABLE IF NOT EXISTS Tenants (
    TenantInt INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(200) NOT NULL UNIQUE,
    ConnectionString VARCHAR(500) NOT NULL,
    Description VARCHAR(1000),
    Status INT NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6),
    INDEX IX_Tenants_Name (Name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Создаем дефолтного тенанта (если еще не существует)
-- Используем INSERT IGNORE чтобы не падать при повторном запуске
-- TenantInt = 1, Name = 'Default Tenant', Status = 1 (Active)
INSERT IGNORE INTO Tenants (TenantInt, Name, Status, CreatedAt, UpdatedAt, ConnectionString)
VALUES 
    (1, 'Default Tenant', 1, NOW(), NOW(), 'Server=mysql;Port=3306;Database=Tenant_1;User=root;Password=sqlPassword123;CharSet=utf8mb4;');

-- Создаем БД для дефолтного тенанта (Tenant_1)
CREATE DATABASE IF NOT EXISTS Tenant_1 CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Примечание: 
-- - Дополнительные данные (пользователи, проекты и т.д.) 
--   будут созданы через API или через отдельные сидеры модулей
-- - В production окружениях сидинг данных выполняется через миграции или отдельные скрипты
-- - Схемы для модулей (Users, Projects, Tasks, Identity, AccessPermissions) 
--   будут созданы автоматически при применении миграций EF Core
