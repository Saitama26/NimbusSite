# NimbusSite

Система управления проектами с мультитенантностью. Distributed Monolith архитектура на основе Clean Architecture и CQRS.

## 🚀 Быстрый старт

### Требования

- **Docker** и **Docker Compose** (версия 2.0+)
- **.NET 10.0 SDK** (только для локальной разработки/сборки)

### Запуск проекта одной командой

```bash
docker compose up -d
```

Эта команда автоматически:
- ✅ Создаст и запустит MySQL контейнер
- ✅ Создаст центральную БД `NimbusSite_Tenants`
- ✅ Создаст Kafka контейнер и все необходимые топики
- ✅ Соберет и запустит все API модули
- ✅ Применит миграции БД для всех модулей (в Development режиме)
- ✅ Настроит все зависимости между сервисами

### Первый запуск

1. **Клонируйте репозиторий:**
   ```bash
   git clone <repository-url>
   cd NimbusSite
   ```

2. **(Опционально) Настройте переменные окружения:**
   
   Скопируйте пример файла `.env.example` в `.env`:
   ```bash
   cp .env.example .env
   ```
   
   Отредактируйте `.env` файл и настройте значения под ваше окружение (см. раздел [Конфигурация](#-конфигурация)).

3. **Запустите проект:**
   ```bash
   docker compose up -d --build
   ```

4. **Проверьте статус сервисов:**
   ```bash
   docker compose ps
   ```

5. **Проверьте логи (если нужно):**
   ```bash
   docker compose logs -f
   ```

### Доступ к API

После успешного запуска все API доступны по следующим адресам:

| Модуль | URL | Swagger UI |
|--------|-----|------------|
| **Tenants** | http://localhost:5001 | http://localhost:5001 |
| **Users** | http://localhost:5002 | http://localhost:5002 |
| **Projects** | http://localhost:5003 | http://localhost:5003 |
| **Tasks** | http://localhost:5004 | http://localhost:5004 |
| **Identity** | http://localhost:5005 | http://localhost:5005 |
| **AccessPermissions** | http://localhost:5006 | http://localhost:5006 |

> **Примечание:** Swagger UI доступен на корне каждого API (например, http://localhost:5001/swagger)

## 📋 Полезные команды

### Управление контейнерами

```bash
# Остановить все сервисы
docker compose down

# Остановить и удалить volumes (очистить данные БД)
docker compose down -v

# Перезапустить все сервисы
docker compose restart

# Перезапустить только API сервисы
docker compose restart tenants-api users-api projects-api tasks-api identity-api accesspermissions-api

# Просмотр логов конкретного сервиса
docker compose logs -f <service-name>

# Просмотр всех логов
docker compose logs -f
```

### Пример `.env` файла

Скопируйте `.env.example` в `.env` и настройте значения:

```bash
cp .env.example .env
```

Основные переменные:

#### MySQL
- `MYSQL_ROOT_PASSWORD` - пароль root пользователя MySQL (по умолчанию: `sqlPassword123`)
- `MYSQL_PORT_EXTERNAL` - внешний порт MySQL (по умолчанию: `3307`)

#### Kafka
- `KAFKA_BOOTSTRAP_SERVERS` - адрес Kafka брокера (по умолчанию: `kafka:9092`)
- `KAFKA_TOPIC_PREFIX` - префикс для топиков (по умолчанию: `domain-events`)

#### ASP.NET Core
- `ASPNETCORE_ENVIRONMENT` - окружение приложения (по умолчанию: `Development`)
- `ASPNETCORE_URLS` - URL для привязки (по умолчанию: `http://+:8080`)

#### Порты API
- `TENANTS_API_PORT` - порт Tenants API (по умолчанию: `5001`)
- `USERS_API_PORT` - порт Users API (по умолчанию: `5002`)
- `PROJECTS_API_PORT` - порт Projects API (по умолчанию: `5003`)
- `TASKS_API_PORT` - порт Tasks API (по умолчанию: `5004`)
- `IDENTITY_API_PORT` - порт Identity API (по умолчанию: `5005`)
- `ACCESSPERMISSIONS_API_PORT` - порт AccessPermissions API (по умолчанию: `5006`)

#### Строки подключения к БД

**Центральная БД для каталога тенантов:**
- `TENANTS_DB_CONNECTION_STRING` - строка подключения к `NimbusSite_Tenants`

#### JWT (для Identity API)
- `JWT_SECRET_KEY` - секретный ключ для подписи JWT токенов (минимум 32 символа)
- `JWT_ISSUER` - издатель токена (по умолчанию: `NimbusSite`)
- `JWT_AUDIENCE` - аудитория токена (по умолчанию: `NimbusSite-Users`)
- `JWT_ACCESS_TOKEN_EXPIRATION_MINUTES` - время жизни access token (по умолчанию: `60`)
- `JWT_REFRESH_TOKEN_EXPIRATION_DAYS` - время жизни refresh token (по умолчанию: `30`)


## 🏗️ Архитектура

### Структура проекта

```
src/
├── Common/              # Общий функционал
│   ├── Common.Domain/   # Базовые сущности, события, результаты
│   ├── Common.Application/  # Абстракции (CQRS, Events, Tenancy)
│   └── Common.Infrastructure/  # Реализации (Kafka, EF Core, Middleware)
├── Modules/            # Бизнес-модули
│   ├── Tenants/       # Управление тенантами
│   ├── Users/         # Управление пользователями
│   ├── Identity/      # Аутентификация/авторизация (JWT)
│   ├── Projects/      # Управление проектами
│   ├── Tasks/         # Управление задачами
│   └── AccessPermissions/  # Управление правами доступа
```

### Структура модуля

Каждый модуль следует Clean Architecture:

```
ModuleName/
├── ModuleName.Domain/        # Доменные сущности, enum, ошибки
├── ModuleName.Application/    # Команды, запросы, обработчики, события
├── ModuleName.Infrastructure/ # DbContext, конфигурации, репозитории
├── ModuleName.Contracts/     # API модели, события, view models
└── ModuleName.Api/          # REST API контроллеры, Program.cs
```

### Мультитенантность

Проект использует **multi-tenant архитектуру**:

- **Центральная БД** (`NimbusSite_Tenants`): хранит каталог всех тенантов
- **Tenant-специфичные БД** (`Tenant_1`, `Tenant_2`, ...): отдельная БД для каждого тенанта
- **Схемы внутри БД тенанта**: `Users`, `Projects`, `Tasks`, `Identity`, `AccessPermissions`

### Топики Kafka

Автоматически создаются следующие топики:
- `domain-events-tenants` - события тенантов
- `domain-events-users` - события пользователей
- `domain-events-projects` - события проектов
- `domain-events-tasks` - события задач
- `domain-events-identity` - события аутентификации
- `domain-events-accesspermissions` - события прав доступа



### Миграции

Миграции EF Core применяются автоматически при старте API в режиме `Development`.


### Очистка и перезапуск

Если что-то пошло не так, можно полностью очистить и перезапустить:

```bash
# Остановить и удалить все контейнеры и volumes
docker compose down -v

# Пересобрать и запустить заново
docker compose up --build -d
```