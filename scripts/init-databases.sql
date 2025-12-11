-- Скрипт для создания баз данных
-- Выполнить: docker exec -i nimbussite-mysql mysql -uroot -psqlPassword123 < scripts/init-databases.sql

CREATE DATABASE IF NOT EXISTS NimbusSite_Tenants CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS NimbusSite_Projects CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS NimbusSite_Users CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS NimbusSite_Tasks CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS NimbusSite_Identity CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE DATABASE IF NOT EXISTS NimbusSite_AccessPermissions CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

SHOW DATABASES;

