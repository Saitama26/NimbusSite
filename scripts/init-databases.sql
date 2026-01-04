-- Скрипт для создания баз данных
-- Автоматически выполняется при первом запуске MySQL контейнера
-- Файл монтируется в /docker-entrypoint-initdb.d/ и выполняется автоматически

-- Создаем только центральную БД для каталога тенантов
-- БД для конкретных тенантов (Tenant_1, Tenant_2, ...) создаются динамически
-- при создании тенанта через API или команду CreateTenantCommand
CREATE DATABASE IF NOT EXISTS NimbusSite_Tenants CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

SHOW DATABASES;