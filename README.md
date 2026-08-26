# Conference Booking API

API для управління конференц-залами та їх бронювання.

Система дозволяє створювати та редагувати зали, шукати вільні зали на потрібний час, створювати бронювання з додатковими послугами та автоматично розраховувати їх вартість.

Також додані кілька звітів, які можуть бути корисні для аналізу використання залів.

## Основні можливості

* створення, редагування та видалення конференц-залів;
* перегляд списку залів і окремого залу;
* пошук вільних залів за часом і необхідною місткістю;
* створення бронювання;
* вибір додаткових послуг;
* автоматичний розрахунок вартості оренди;
* захист від подвійного бронювання одного залу;
* звіти по доходу, завантаженості залів і популярності послуг;
* Swagger-документація;
* rate limiting для операцій запису, бронювань і звітів;
* запуск API та PostgreSQL через Docker Compose.

## Технології

* .NET 10
* ASP.NET Core Web API
* Entity Framework Core
* PostgreSQL 17
* Npgsql
* FluentValidation
* Swagger / Swashbuckle
* xUnit
* Moq
* Docker
* Docker Compose

## Структура проєкту

Проєкт розділений на декілька шарів:

```text
src/
Domain
Application
Infrastructure
WebApi

tests/
Application.Tests
```

### Domain

Містить основні сутності предметної області:

* `ConferenceRoom`
* `Booking`
* `Service`

Domain не залежить від інших проєктів solution.

### Application

Містить сценарії роботи системи:

* створення та редагування залів;
* пошук доступних залів;
* створення бронювання;
* розрахунок вартості;
* формування звітів;
* валідацію;
* інтерфейси репозиторіїв та інших залежностей.

### Infrastructure

Відповідає за роботу з PostgreSQL через Entity Framework Core:

* `AppDbContext`;
* EF Core configurations;
* repositories;
* migrations;
* реалізацію business timezone;
* Unit of Work.

### WebApi

HTTP-рівень застосунку:

* controllers;
* request contracts;
* Swagger;
* global exception handling;
* rate limiting;
* конфігурація DI.

## Початкові дані

Після застосування міграцій у БД створюються дані з технічного завдання.

### Зали

| Зал   | Місткість | Вартість за годину |
| ----- | --------: | -----------------: |
| Зал A |        50 |           2000 грн |
| Зал B |       100 |           3500 грн |
| Зал C |        30 |           1500 грн |

### Послуги

| Послуга  | Вартість |
| -------- | -------: |
| Проєктор |  500 грн |
| Wi-Fi    |  300 грн |
| Звук     |  700 грн |

У ТЗ не задано, які саме послуги доступні для кожного початкового залу, тому ці зв'язки не додаються автоматично.

## Розрахунок вартості

Базою для розрахунку є погодинна вартість залу.

Використовуються такі часові правила:

| Час         | Правило     |
| ----------- | ----------- |
| 06:00–09:00 | знижка 10%  |
| 09:00–12:00 | базова ціна |
| 12:00–14:00 | націнка 15% |
| 14:00–18:00 | базова ціна |
| 18:00–23:00 | знижка 20%  |

Якщо бронювання проходить через декілька тарифних проміжків, кожна частина розраховується окремо.

Наприклад, для залу з базовою вартістю `2000 грн/год` бронювання з `11:00` до `13:00`:

```text
11:00–12:00 = 2000 грн
12:00–13:00 = 2000 * 1.15 = 2300 грн

Разом: 4300 грн
```

Після цього до вартості оренди додається вартість вибраних послуг.

Послуги рахуються як одноразова доплата за бронювання.

## Робота з часом

У PostgreSQL час бронювань зберігається в UTC.

Для тарифікації використовується бізнесова часова зона:

```text
Europe/Kyiv
```

Це зроблено для того, щоб клієнт не міг впливати на тариф, просто передавши той самий момент часу з іншим UTC offset.

Наприклад:

```text
2026-09-01 12:00 +03:00
```

і

```text
2026-09-01 09:00 +00:00
```

представляють один момент часу і повинні мати однаковий тариф.

## Захист від подвійного бронювання

Перед створенням бронювання Application перевіряє, чи немає іншого бронювання, яке перетинається з вибраним періодом.

Перетин визначається за правилом:

```text
existing.StartTime < requested.EndTime
AND
existing.EndTime > requested.StartTime
```

Додатково така сама гарантія реалізована на рівні PostgreSQL через exclusion constraint.

Це потрібно через можливу ситуацію, коли два паралельні HTTP-запити одночасно перевірять зал і обидва побачать його вільним.

Тому application-level перевірка використовується для нормального відпрацювання, а constraint БД є остаточним захистом від race condition.

## API

### Conference rooms

```http
GET    /api/conference-rooms
GET    /api/conference-rooms/{id}
GET    /api/conference-rooms/available
POST   /api/conference-rooms
PUT    /api/conference-rooms/{id}
DELETE /api/conference-rooms/{id}
```

Пошук доступних залів:

```http
GET /api/conference-rooms/available?startTime=2026-09-01T10:00:00%2B03:00&endTime=2026-09-01T14:00:00%2B03:00&capacity=50
```

### Bookings

```http
POST /api/bookings
```

Приклад:

```json
{
  "conferenceRoomId": "11111111-1111-1111-1111-111111111111",
  "startTime": "2026-09-01T10:00:00+03:00",
  "endTime": "2026-09-01T14:00:00+03:00",
  "serviceIds": [1, 2]
}
```

У відповіді повертається ID бронювання та розрахована загальна вартість.

### Services

```http
GET /api/services
```

### Reports

```http
GET /api/reports/revenue
GET /api/reports/room-utilization
GET /api/reports/popular-services
```

Усі звіти приймають період через `from` і `to`.

Наприклад:

```http
GET /api/reports/revenue?from=2026-09-01T00:00:00%2B03:00&to=2026-10-01T00:00:00%2B03:00
```

Зараз бронювання відноситься до звітного періоду за часом його початку:

```text
[from, to)
```

## Запуск через Docker

Найпростіший спосіб запустити проєкт це звісно через найкращий тул, а саме docker.

Спочатку потрібно створити `.env`:

```bash
cp .env.example .env
```

Приклад:

```env
POSTGRES_DB=conference_booking
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
```

Після цього:

```bash
docker compose up --build
```

Docker Compose запустить:

* PostgreSQL;
* Conference Booking API.

API буде доступне за адресою:

```text
http://localhost:5034
```

Swagger:

```text
http://localhost:5034/swagger
```

При першому запуску в Docker автоматично застосовуються EF Core migrations.

Для перевірки повністю чистого запуску:

```bash
docker compose down -v
docker compose up --build
```

Звичайний:

```bash
docker compose down
```

volume не видаляє, тому дані БД зберігаються між перезапусками контейнерів.

## Локальний запуск без Docker

Для локального запуску потрібен PostgreSQL.

Connection string не зберігається в репозиторії. Для Development використовується .NET User Secrets.

Ініціалізація:

```bash
dotnet user-secrets init --project src/WebApi
```

Додавання connection string:

```bash
dotnet user-secrets set \
  "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=conference_booking;Username=postgres;Password=postgres" \
  --project src/WebApi
```

Застосувати міграції:

```bash
dotnet ef database update -p src/Infrastructure -s src/WebApi
```

Запустити API:

```bash
dotnet run --project src/WebApi
```

## Міграції

Створення нової міграції:

```bash
dotnet ef migrations add MigrationName -p src/Infrastructure -s src/WebApi
```

Застосування:

```bash
dotnet ef database update -p src/Infrastructure -s src/WebApi
```

У Docker автоматичне застосування міграцій контролюється параметром:

```text
Database__ApplyMigrationsOnStartup
```

За замовчуванням у застосунку воно вимкнене.

У `docker-compose.yml` ввімкнене для спрощення локального запуску тестового проєкту.

## Swagger

API задокументоване через Swagger/OpenAPI.

Swagger доступний у Development:

```text
/swagger
```

У документації описані:

* endpoint-и;
* request/response schemas;
* query та route parameters;
* можливі HTTP status codes;
* основні правила створення бронювань.

## Обробка помилок

API повертає помилки у форматі `ProblemDetails`.

Основні статуси:

```text
400 Bad Request
404 Not Found
409 Conflict
429 Too Many Requests
500 Internal Server Error
```

Внутрішні повідомлення exception для `500` клієнту не повертаються.

Повна інформація про неочікувану помилку залишається в server logs, а клієнт отримує `traceId`, за яким помилку можна знайти.

## Rate limiting

Для API додані окремі обмеження:

```text
операції запису = 30 запитів / хвилину
бронювання      = 10 запитів / хвилину
звіти           = 20 запитів / хвилину
```

Ліміти зараз застосовуються за IP-адресою клієнта. (Зрозуміло, що в проді так не піде бо за проксі у всіх однаковий IP)

## Тести

Unit-тестами покрита основна бізнес-логіка:

* розрахунок вартості для різних тарифних проміжків;
* тарифні межі;
* частини години;
* validation бронювання;
* створення бронювання;
* недоступні послуги;
* конфлікт бронювань;
* збереження часу в UTC;
* business timezone;
* пошук доступних залів.

Запуск:

```bash
dotnet test
```

## Рішення, прийняті під час розробки

### Тарифні правила залишені в коді

У поточному завданні часові правила фіксовані, тому вони реалізовані в `RentalPriceCalculator`.

Для реального продукту, де тарифи змінюються через адмін-панель або мають період дії, я б виніс їх у БД.

Наприклад:

```text
start_time
end_time
percentage_modifier
priority
valid_from
valid_to
is_active
```

Сам алгоритм розрахунку при цьому все одно залишався б у коді.

### Зал із бронюваннями не видаляється

При спробі видалити зал, який уже має бронювання, API повертає `409 Conflict`.

Історичні дані про бронювання не видаляються каскадно.

### Міграції при запуску

Автоматичні міграції не ввімкнені глобально.

Вони використовуються в Docker Compose для зручного запуску тестового проєкту.

Для production я б запускав міграції окремим кроком deployment pipeline.

## Що варто було б додати для production

Поточний проєкт реалізує вимоги тестового, але для реального використання я б додав:

* authentication та authorization;
* ролі для адміністратора та клієнта;
* окреме керування тарифними правилами;
* історію зміни тарифів;
* pagination для списків;
* окрему CI/CD стратегію для міграцій;
* налаштування forwarded headers для роботи за reverse proxy.
