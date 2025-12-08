# NimbusSite - Multi-tenant Project Management System

NimbusSite - это система управления проектами с поддержкой мультитенантности, построенная на Clean Architecture и Domain-Driven Design (DDD) принципах.

## 🚀 Быстрый старт

### Требования

- **.NET 10.0 SDK** или выше
- **MySQL 8.0+** (или совместимая версия)
- **PowerShell** (для Windows) или **Bash** (для Linux/Mac)

### Шаг 1: Клонирование и настройка окружения

1. Клонируйте репозиторий (если еще не сделано):
```bash
git clone <repository-url>
cd NimbusSite
```

2. Создайте файл `.env` в корне проекта (`NimbusSite/.env`) со следующим содержимым:

```env
# Database Configuration
DATABASE_CONNECTION_STRING=Server=localhost;Port=3306;Database=NimbusSite;User=root;Password=your_password;CharSet=utf8mb4;
MYSQL_SERVER_VERSION=8.0.21

# JWT Configuration
JWT_SECRET_KEY=your-super-secret-key-minimum-32-characters-long-for-security
JWT_ISSUER=NimbusSite
JWT_AUDIENCE=NimbusSite-Users
JWT_ACCESS_TOKEN_EXPIRATION_MINUTES=60
JWT_REFRESH_TOKEN_EXPIRATION_DAYS=30

# CORS Configuration
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:5173
```

**Важно:** 
- Замените `your_password` на ваш пароль MySQL
- Замените `your-super-secret-key-minimum-32-characters-long-for-security` на случайную строку длиной минимум 32 символа
- Убедитесь, что файл `.env` находится в корне `NimbusSite/`

### Шаг 2: Создание базы данных

1. Подключитесь к MySQL:
```bash
mysql -u root -p
```

2. Создайте базу данных:
```sql
CREATE DATABASE NimbusSite CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
EXIT;
```

### Шаг 3: Применение миграций

Из корня проекта (`NimbusSite/`) выполните:

```powershell
# Windows PowerShell
dotnet ef migrations add InitialCreate --project src\Infrastructure\Infrastructure.csproj --startup-project src\Web.Api\Web.Api.csproj

# Применить миграции к базе данных
dotnet ef database update --project src\Infrastructure\Infrastructure.csproj --startup-project src\Web.Api\Web.Api.csproj
```

### Шаг 4: Запуск приложения

```powershell
cd src\Web.Api
dotnet run
```

Приложение будет доступно по адресам:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

## 📚 Использование API

### Swagger UI

После запуска приложения откройте Swagger UI в браузере:
```
https://localhost:5001/swagger
```

Swagger предоставляет интерактивную документацию API, где вы можете:
- Просмотреть все доступные endpoints
- Протестировать API прямо из браузера
- Увидеть примеры запросов и ответов

### Аутентификация

Большинство endpoints требуют JWT аутентификации. Процесс работы:

#### 1. Создание тенанта (публичный endpoint)

```http
POST /api/tenants
Content-Type: application/json

{
  "name": "My Company",
  "connectionString": "Server=localhost;Port=3306;Database=MyCompanyDB;User=root;Password=password;CharSet=utf8mb4;"
}
```

**Ответ:** `{ "tenantId": "guid" }` - сохраните этот ID для регистрации пользователей

#### 2. Регистрация пользователя (публичный endpoint)

```http
POST /api/auth/register
Content-Type: application/json

{
  "tenantId": "guid-from-step-1",
  "userName": "john_doe",
  "email": "john@example.com",
  "password": "SecurePassword123!",
  "role": "Admin"
}
```

**Ответ:** `{ "userId": "guid" }`

#### 3. Вход в систему (публичный endpoint)

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePassword123!"
}
```

**Ответ:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh-token-string",
  "expiresAt": "2024-01-01T12:00:00Z"
}
```

#### 4. Использование токена

Для всех защищенных endpoints добавьте заголовок:
```
Authorization: Bearer {accessToken}
```

#### 5. Обновление токена

Когда access token истечет, используйте refresh token:

```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "refresh-token-string"
}
```

#### 6. Выход из системы

```http
POST /api/auth/logout
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "refreshToken": "refresh-token-string"
}
```

## 🔑 Основные API Endpoints

### Аутентификация (`/api/auth`)
- `POST /api/auth/register` - Регистрация нового пользователя
- `POST /api/auth/login` - Вход в систему
- `POST /api/auth/refresh` - Обновление access token
- `POST /api/auth/logout` - Выход из системы

### Тенанты (`/api/tenants`) - Требует авторизации
- `GET /api/tenants` - Получить все тенанты
- `GET /api/tenants/{id}` - Получить тенант по ID
- `POST /api/tenants` - Создать тенант (публичный)
- `PUT /api/tenants/{id}` - Обновить тенант

### Пользователи (`/api/users`) - Требует авторизации
- `GET /api/users` - Получить всех пользователей (фильтр: `?tenantId=guid`)
- `GET /api/users/{id}` - Получить пользователя по ID

### Проекты (`/api/projects`) - Требует авторизации
- `GET /api/projects` - Получить все проекты (фильтры: `?tenantId=guid&ownerId=guid`)
- `GET /api/projects/{id}` - Получить проект по ID
- `POST /api/projects` - Создать проект
- `PUT /api/projects/{id}` - Обновить проект
- `DELETE /api/projects/{id}` - Удалить проект
- `PATCH /api/projects/{id}/status` - Изменить статус проекта
- `POST /api/projects/{id}/members` - Добавить участника в проект
- `DELETE /api/projects/{id}/members` - Удалить участника из проекта

### Задачи (`/api/tasks`) - Требует авторизации
- `GET /api/tasks` - Получить все задачи (фильтры: `?projectId=guid&assignedUserId=guid&tenantId=guid`)
- `GET /api/tasks/{id}` - Получить задачу по ID
- `POST /api/tasks` - Создать задачу
- `PUT /api/tasks/{id}` - Обновить задачу
- `DELETE /api/tasks/{id}` - Удалить задачу
- `POST /api/tasks/{id}/assign` - Назначить задачу пользователю
- `PATCH /api/tasks/{id}/status` - Изменить статус задачи

### Разрешения (`/api/permissions`) - Требует авторизации
- `GET /api/permissions` - Получить все разрешения (фильтры: `?userId=guid&projectId=guid&tenantId=guid&includeRevoked=true`)
- `POST /api/permissions` - Выдать разрешение пользователю
- `DELETE /api/permissions` - Отозвать разрешение

## 🏗️ Архитектура проекта

Проект следует принципам Clean Architecture и разделен на слои:

```
NimbusSite/
├── src/
│   ├── Domain/              # Доменный слой (бизнес-логика, сущности, события)
│   ├── Application/         # Слой приложения (команды, запросы, обработчики)
│   ├── Infrastructure/      # Инфраструктурный слой (БД, внешние сервисы)
│   ├── Web.Api/            # Слой представления (API контроллеры)
│   └── SharedKernel/       # Общие компоненты
├── .env                    # Переменные окружения (не коммитится)
└── README.md              # Этот файл
```

### Основные технологии

- **.NET 10.0** - Платформа разработки
- **Entity Framework Core 9.0** - ORM для работы с БД
- **MySQL (Pomelo)** - База данных
- **MediatR** - Реализация паттерна Mediator для CQRS
- **FluentValidation** - Валидация команд
- **JWT Bearer** - Аутентификация
- **Swagger/OpenAPI** - Документация API

### Паттерны и принципы

- **CQRS** (Command Query Responsibility Segregation) - Разделение команд и запросов
- **DDD** (Domain-Driven Design) - Доменно-ориентированное проектирование
- **Repository Pattern** - Абстракция доступа к данным
- **Unit of Work** - Управление транзакциями
- **Domain Events** - События домена для слабой связанности
- **Result Pattern** - Обработка ошибок без исключений

## 🔧 Разработка

### Добавление новой миграции

```powershell
dotnet ef migrations add MigrationName --project src\Infrastructure\Infrastructure.csproj --startup-project src\Web.Api\Web.Api.csproj
```

### Применение миграций

```powershell
dotnet ef database update --project src\Infrastructure\Infrastructure.csproj --startup-project src\Web.Api\Web.Api.csproj
```

### Откат миграции

```powershell
dotnet ef database update PreviousMigrationName --project src\Infrastructure\Infrastructure.csproj --startup-project src\Web.Api\Web.Api.csproj
```

### Просмотр списка миграций

```powershell
dotnet ef migrations list --project src\Infrastructure\Infrastructure.csproj --startup-project src\Web.Api\Web.Api.csproj
```

### Сборка проекта

```powershell
dotnet build
```

### Запуск тестов (если есть)

```powershell
dotnet test
```

## 📝 Примеры использования

### Пример: Создание проекта

```http
POST /api/projects
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "tenantId": "guid",
  "name": "My First Project",
  "description": "Описание проекта",
  "ownerId": "user-guid",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z"
}
```

### Пример: Создание задачи

```http
POST /api/tasks
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "tenantId": "guid",
  "projectId": "project-guid",
  "title": "Завершить документацию",
  "description": "Написать README для проекта",
  "assignedUserId": "user-guid",
  "dueDate": "2024-01-15T23:59:59Z",
  "priority": "High"
}
```

### Пример: Выдача разрешения

```http
POST /api/permissions
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "tenantId": "guid",
  "userId": "user-guid",
  "projectId": "project-guid",
  "role": "Manager"
}
```

## 🐛 Решение проблем

### Ошибка: "DATABASE_CONNECTION_STRING environment variable is not set"

**Решение:** Убедитесь, что файл `.env` находится в корне `NimbusSite/` и содержит правильную строку подключения.

### Ошибка: "Unable to create a 'DbContext'"

**Решение:** Проверьте, что MySQL запущен и база данных создана. Проверьте правильность строки подключения в `.env`.

### Ошибка: "Swagger failed to load"

**Решение:** 
1. Убедитесь, что проект собран без ошибок: `dotnet build`
2. Проверьте, что все контроллеры правильно настроены
3. Очистите кэш: `dotnet clean` и пересоберите проект

### Ошибка: "401 Unauthorized"

**Решение:** 
1. Убедитесь, что вы отправили токен в заголовке `Authorization: Bearer {token}`
2. Проверьте, что токен не истек
3. Если токен истек, используйте `/api/auth/refresh` для получения нового

## 📖 Дополнительная документация

- [Entity Framework Core Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [MediatR Documentation](https://github.com/jbogard/MediatR/wiki)
- [JWT Authentication](https://jwt.io/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## 📄 Лицензия

MIT License

## 👥 Поддержка

При возникновении проблем создайте issue в репозитории проекта.
