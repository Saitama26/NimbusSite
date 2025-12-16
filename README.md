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
- Docker и Docker Compose
- .NET 10.0 SDK (нужно только для локальной разработки/сборки)

### Шаги
1. Клонировать репозиторий:
   ```bash
   git clone <repository-url>
   cd NimbusSite
   ```

2. (Опционально) создать `.env` в корне с вашими значениями:
   ```
   KAFKA_BOOTSTRAP_SERVERS=localhost:9092
   JWT__SECRETKEY=<secret>
   JWT__ISSUER=NimbusSite
   JWT__AUDIENCE=NimbusSite-Users
   ```
   Если не создавать, возьмутся значения из `docker-compose.yml`.

3. Запустить все сервисы:
   ```bash
   docker-compose up --build -d
   ```
   - MySQL создаст базы из `scripts/init-databases.sql`.
   - Миграции применяются при старте API (в Development включено).

4. Доступы по умолчанию:
   - Tenants: http://localhost:5001
   - Users: http://localhost:5002
   - Projects: http://localhost:5003
   - Tasks: http://localhost:5004
   - Identity: http://localhost:5005
   - AccessPermissions: http://localhost:5006
   Swagger UI на корне каждого API (например, http://localhost:5001).

### Полезные команды
- Остановить всё: `docker-compose down`
- Перезапустить только API:  
  `docker-compose restart tenants-api users-api projects-api tasks-api identity-api accesspermissions-api`
- Логи сервиса:  
  `docker-compose logs -f <service-name>`
- Список топиков Kafka:  
  `docker exec -it nimbussite-kafka /opt/kafka/bin/kafka-topics.sh --bootstrap-server localhost:9092 --list`
- Подключиться к MySQL:  
  `docker exec -it nimbussite-mysql mysql -uroot -psqlPassword123`

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