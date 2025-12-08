# NimbusSite - Distributed Monolith Architecture

Система управления проектами с поддержкой мультитенантности, построенная как распределенный монолит.

## 📋 Содержание

- [Архитектура](#архитектура)
- [Технологии](#технологии)
- [Структура проекта](#структура-проекта)
- [Реализованные модули](#реализованные-модули)
- [Быстрый старт](#быстрый-старт)
- [API Документация](#api-документация)
- [Конфигурация](#конфигурация)

## 🏗️ Архитектура

Проект построен на принципах **Distributed Monolith** с использованием **Clean Architecture** и **CQRS** паттернов.

### Принципы архитектуры

1. **Каждый модуль - независимое API** со своей БД
2. **Взаимодействие через события** (Kafka) для изменений
3. **Чтение из других модулей** через view БД (read-only)
4. **Шардирование** по TenantId с использованием ShardMapManager
5. **CQRS** - разделение команд и запросов
6. **Domain-Driven Design** - доменная модель в центре

### Слои модуля

Каждый модуль состоит из следующих слоев:

- **Domain** - доменные сущности, события, value objects, агрегаты
- **Application** - команды, запросы, handlers (CQRS), DTOs, маппинг
- **Infrastructure** - репозитории, DbContext, миграции, события, внешние сервисы
- **Api** - REST API, контроллеры, Swagger
- **Events** - контракты событий для Kafka

## 🛠️ Технологии

- **.NET 10.0**
- **Entity Framework Core 8.0.7** (Code First)
- **MySQL** (Pomelo.EntityFrameworkCore.MySql 8.0.2)
- **MediatR 12.2.0** (CQRS)
- **FluentValidation** (валидация команд)
- **AutoMapper** (маппинг объектов)
- **Kafka** (Confluent.Kafka 2.3.0) - события между модулями
- **Swagger/OpenAPI** (Swashbuckle.AspNetCore 10.0.1) - документация API
- **DotNetEnv** - загрузка переменных окружения из .env

## 📁 Структура проекта

```
NimbusSite/
├── src/
│   ├── Common/                          # Общий функционал
│   │   ├── Common.Domain/              # Базовые доменные типы, события, Results
│   │   ├── Common.Application/         # Общие абстракции (ICommand, IQuery, ISender)
│   │   └── Common.Infrastructure/      # Общая инфраструктура
│   │       ├── Messaging/              # MediatRSender (реализация ISender)
│   │       ├── Events/                 # KafkaEventBus, KafkaEventSubscriber
│   │       ├── Behaviors/              # LoggingBehavior, ValidationBehavior
│   │       ├── Middleware/             # ErrorHandling, CorrelationId, TenantContext
│   │       └── Configuration/          # EnvLoader
│   │
│   ├── Modules/                        # Бизнес-модули (домены)
│   │   ├── Tenants/                    # ✅ Управление тенантами (РЕАЛИЗОВАНО)
│   │   ├── Users/                      # ⏳ Управление пользователями
│   │   ├── Identity/                   # ⏳ Аутентификация/Авторизация
│   │   ├── Projects/                   # ⏳ Управление проектами
│   │   ├── Tasks/                      # ⏳ Управление задачами
│   │   └── AccessPermissions/          # ⏳ Разрешения и доступы
│   │
│   └── Gateway/                        # API Gateway
│       └── Gateway.Api/
│
└── .env                                # Переменные окружения (не в git)
```

## ✅ Реализованные модули

### Tenants Module (Полностью реализован)

Модуль для управления тенантами в системе.

#### Функциональность

**Команды (Commands):**
- ✅ `CreateTenant` - создание нового тенанта
- ✅ `UpdateTenant` - обновление информации о тенанте
- ✅ `DeleteTenant` - удаление тенанта (soft delete)
- ✅ `ChangeTenantStatus` - изменение статуса тенанта
- ✅ `UpdateTenantConnectionString` - обновление строки подключения

**Запросы (Queries):**
- ✅ `GetTenants` - получение списка всех тенантов
- ✅ `GetTenantById` - получение тенанта по ID

**API Endpoints:**
- ✅ `GET /api/tenants` - список тенантов
- ✅ `GET /api/tenants/{tenantId}` - тенант по ID
- ✅ `POST /api/tenants` - создать тенанта
- ✅ `PUT /api/tenants/{tenantId}` - обновить тенанта
- ✅ `DELETE /api/tenants/{tenantId}` - удалить тенанта
- ✅ `PATCH /api/tenants/{tenantId}/status` - изменить статус
- ✅ `PUT /api/tenants/{tenantId}/connection-string` - обновить connection string

**Доменные события:**
- ✅ `TenantCreatedEvent`
- ✅ `TenantUpdatedEvent`
- ✅ `TenantDeletedEvent`
- ✅ `TenantStatusChangedEvent`
- ✅ `TenantConnectionStringUpdatedEvent`

**Интеграционные события:**
- ✅ `TenantCreatedIntegrationEvent`
- ✅ `TenantUpdatedIntegrationEvent`
- ✅ `TenantDeletedIntegrationEvent`
- ✅ `TenantStatusChangedIntegrationEvent`
- ✅ `TenantConnectionInfoChangedIntegrationEvent`

**Особенности:**
- ✅ Валидация команд через FluentValidation
- ✅ Обработка ошибок через Result pattern
- ✅ Публикация событий в Kafka
- ✅ Шардирование через ShardMapManager
- ✅ UnitOfWork паттерн
- ✅ Репозиторий паттерн

#### Модель данных

**Tenant:**
- `Id` (Guid) - уникальный идентификатор
- `Name` (string) - название тенанта
- `Subdomain` (string) - поддомен (уникальный)
- `ConnectionString` (string?) - строка подключения к БД тенанта
- `Status` (TenantStatus) - статус (Active, Suspended, Deleted)
- `Description` (string?) - описание
- `AdminEmail` (string?) - email администратора
- `CreatedAt` (DateTime) - дата создания
- `UpdatedAt` (DateTime?) - дата обновления

**ShardMapEntry:**
- `TenantId` (Guid) - ID тенанта
- `ConnectionString` (string) - строка подключения к шарду
- `ShardKey` (string) - ключ шарда

