# NimbusSite

Система управления проектами с мультитенантностью. Distributed Monolith архитектура.

## Технологии

- .NET 10.0
- MySQL 8.0
- Kafka (KRaft mode)
- Entity Framework Core 8.0.7
- Docker & Docker Compose

## Запуск

### Требования
- Docker & Docker Compose
- .NET 10.0 SDK (для разработки)

### Быстрый старт

1. Клонировать репозиторий:
```bash
git clone <repository-url>
cd NimbusSite
```

2. Запустить все сервисы:
```bash
docker-compose up --build
```

3. API доступны:
- **Tenants**: http://localhost:5001
- **Users**: http://localhost:5002
- **Projects**: http://localhost:5003
- **Tasks**: http://localhost:5004
- **Identity**: http://localhost:5005
- **AccessPermissions**: http://localhost:5006

Swagger UI доступен на корневом пути каждого API (например, http://localhost:5001).

### Полезные команды

**Остановить все сервисы:**
```bash
docker-compose down
```

**Перезапустить только API сервисы:**
```bash
docker-compose restart tenants-api users-api projects-api tasks-api identity-api accesspermissions-api
```

**Просмотреть логи:**
```bash
docker-compose logs -f <service-name>
# Например: docker-compose logs -f projects-api
```

**Проверить топики Kafka:**
```bash
docker exec -it nimbussite-kafka /opt/kafka/bin/kafka-topics.sh --bootstrap-server localhost:9092 --list
```

**Подключиться к MySQL:**
```bash
docker exec -it nimbussite-mysql mysql -uroot -psqlPassword123
```

## Что реализовано

### Модули
- ✅ **Tenants** - управление тенантами
- ✅ **Users** - управление пользователями
- ✅ **Identity** - аутентификация/авторизация (JWT)
- ✅ **Projects** - управление проектами
- ✅ **Tasks** - управление задачами
- ✅ **AccessPermissions** - управление правами доступа

### Функциональность
- ✅ CQRS (Commands/Queries)
- ✅ События через Kafka (изменение данных в других доменах)
- ✅ Автоматическое создание топиков Kafka при запуске
- ✅ Шардирование по TenantId
- ✅ REST API с Swagger
- ✅ Миграции БД (EF Core)
- ⏳ Database Views для чтения из других доменов (не реализовано)
- ⏳ Обработка Kafka events (не реализовано)

## Структура проекта

```
src/
├── Common/              # Общий функционал
├── Modules/            # Бизнес-модули
│   ├── Tenants/
│   ├── Users/
│   ├── Identity/
│   ├── Projects/
│   ├── Tasks/
│   └── AccessPermissions/
└── Contracts/          # Контракты событий
```

Каждый модуль: **Domain** → **Application** → **Infrastructure** → **Api**

## Конфигурация

Переменные окружения настраиваются в `docker-compose.yml`. Опционально можно создать `.env` файл в корне проекта.