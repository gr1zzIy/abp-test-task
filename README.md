# Conference Booking API

API для керування конференц-залами та їх бронювання.

Проєкт починався як тестове завдання, але в процесі я довів його до більш наближеного до реального backend-сервісу стану: додав автентифікацію, ролі, прив’язку бронювань до користувачів, захист від race condition на рівні PostgreSQL, health checks, інтеграційні тести та CI.

## Що вміє API

- створювати, редагувати та видаляти конференц-зали;
- повертати список залів і детальну інформацію про окремий зал;
- шукати вільні зали за часом і необхідною місткістю;
- реєструвати та автентифікувати користувачів;
- працювати з ролями `Admin` і `Client`;
- створювати бронювання від імені поточного користувача;
- переглядати власні бронювання;
- скасовувати бронювання без фізичного видалення історії;
- вибирати додаткові послуги для бронювання;
- автоматично розраховувати вартість оренди;
- захищати один і той самий зал від подвійного бронювання;
- формувати звіти по доходу, завантаженості залів і популярності послуг;
- віддавати liveness/readiness health checks;
- документувати API через Swagger;
- обмежувати частоту частини запитів через rate limiting;
- запускатися разом із PostgreSQL через Docker Compose;
- автоматично проходити build, unit tests та integration tests у GitHub Actions.

## Технології

- .NET 10
- ASP.NET Core Web API
- ASP.NET Core Identity
- JWT Bearer Authentication
- Entity Framework Core
- PostgreSQL 17
- Npgsql
- FluentValidation
- Swagger / Swashbuckle
- xUnit
- Moq
- WebApplicationFactory
- Testcontainers
- Docker
- Docker Compose
- GitHub Actions

## Структура проєкту

Проєкт розділений на чотири основні шари:

```text
src/
Domain
Application
Infrastructure
WebApi

tests/
Application.Tests
Integration.Tests
```

### Domain

Містить основні сутності та enum-и предметної області:

- `ConferenceRoom`
- `Booking`
- `Service`
- `BookingStatus`

Domain не залежить від інших проєктів solution.

### Application

Тут знаходяться сценарії роботи системи та контракти, через які Application взаємодіє з зовнішнім світом:

- створення та редагування залів;
- пошук доступних залів;
- створення, перегляд і скасування бронювань;
- робота з поточним користувачем;
- реєстрація та login;
- розрахунок вартості;
- формування звітів;
- FluentValidation;
- інтерфейси репозиторіїв;
- абстракції для Identity, часу та інших інфраструктурних залежностей.

Application не працює напряму з `HttpContext`, `UserManager`, PostgreSQL або конкретною реалізацією JWT.

### Infrastructure

Відповідає за реалізацію інфраструктурних залежностей:

- `AppDbContext`;
- EF Core configurations;
- repositories;
- migrations;
- Unit of Work;
- ASP.NET Core Identity;
- створення JWT access token;
- seed ролей;
- початкове створення адміністратора;
- реалізацію business timezone;
- роботу з PostgreSQL.

### WebApi

HTTP-рівень застосунку:

- controllers;
- request contracts;
- Swagger;
- global exception handling;
- rate limiting;
- `ICurrentUser` через `HttpContext`;
- health check endpoints;
- конфігурація DI та middleware pipeline.

## Початкові дані

Після застосування міграцій у БД створюються дані з технічного завдання.

### Зали

| Зал | Місткість | Вартість за годину |
| --- | --------: | -----------------: |
| Зал A | 50 | 2000 грн |
| Зал B | 100 | 3500 грн |
| Зал C | 30 | 1500 грн |

### Послуги

| Послуга | Вартість |
| --- | ---: |
| Проєктор | 500 грн |
| Wi-Fi | 300 грн |
| Звук | 700 грн |

У ТЗ не вказано, які саме послуги доступні для кожного початкового залу, тому зв’язки між seed-залами та послугами автоматично не створюються.

## Розрахунок вартості

Базою для розрахунку є погодинна вартість залу.

Використовуються такі часові правила:

| Час | Правило |
| --- | --- |
| 06:00–09:00 | знижка 10% |
| 09:00–12:00 | базова ціна |
| 12:00–14:00 | націнка 15% |
| 14:00–18:00 | базова ціна |
| 18:00–23:00 | знижка 20% |

Якщо бронювання проходить через кілька тарифних проміжків, кожна частина розраховується окремо.

Наприклад, для залу з базовою вартістю `2000 грн/год` бронювання з `11:00` до `13:00`:

```text
11:00–12:00 = 2000 грн
12:00–13:00 = 2000 * 1.15 = 2300 грн

Разом: 4300 грн
```

Після цього до вартості оренди додається вартість вибраних послуг.

Зараз послуги рахуються як одноразова доплата за бронювання.

## Робота з часом

У PostgreSQL час бронювань зберігається в UTC.

Для перевірки бізнесових обмежень і розрахунку тарифу використовується часова зона:

```text
Europe/Kyiv
```

Це важливо, тому що клієнт не повинен мати можливість впливати на тариф, просто передаючи той самий момент часу з іншим UTC offset.

Наприклад:

```text
2026-09-01 12:00 +03:00
```

і

```text
2026-09-01 09:00 +00:00
```

це один і той самий момент часу, тому результат тарифікації для них має бути однаковим.

## Автентифікація та ролі

Для автентифікації використовується ASP.NET Core Identity та JWT Bearer token.

Доступні дві ролі:

```text
Admin
Client
```

Публічна реєстрація завжди створює користувача з роллю `Client`. Роль не передається в request, тому через `/api/auth/register` не можна самостійно зареєструвати адміністратора.

Адміністратор створюється окремим startup seeder-ом, якщо це явно дозволено конфігурацією. Email і пароль адміністратора не зберігаються в репозиторії.

Основні правила доступу:

- `Admin` може створювати, редагувати та видаляти зали;
- `Admin` має доступ до звітів;
- `Client` може створювати бронювання;
- `Client` бачить тільки власні бронювання;
- `Client` може скасувати тільки власне бронювання;
- `Admin` може переглядати всі бронювання та працювати з будь-яким із них.

Для чужого бронювання клієнту повертається `404`, а не `403`, щоб не розкривати сам факт існування такого запису.

## Захист від подвійного бронювання

Перед створенням бронювання Application перевіряє, чи немає іншого активного бронювання, яке перетинається з вибраним періодом.

Перетин визначається за правилом:

```text
existing.StartTime < requested.EndTime
AND
existing.EndTime > requested.StartTime
```

Суміжні бронювання, наприклад `10:00–12:00` і `12:00–14:00`, не вважаються такими, що перетинаються.

Окремо ця ж гарантія реалізована на рівні PostgreSQL через `EXCLUDE USING gist`.

Application-level перевірка потрібна для нормального сценарію та зрозумілої відповіді користувачу. Constraint у PostgreSQL залишається остаточним захистом від race condition, коли два паралельні запити одночасно бачать один і той самий слот вільним.

Exclusion constraint застосовується тільки до активних бронювань. Після скасування бронювання слот знову можна використовувати.

## API

### Authentication

```http
POST /api/auth/register
POST /api/auth/login
GET  /api/auth/me
```

`/api/auth/me` потребує валідного JWT access token.

### Conference rooms

```http
GET    /api/conference-rooms
GET    /api/conference-rooms/{id}
GET    /api/conference-rooms/available
POST   /api/conference-rooms
PUT    /api/conference-rooms/{id}
DELETE /api/conference-rooms/{id}
```

Операції створення, редагування та видалення залів доступні тільки `Admin`.

Пошук доступних залів:

```http
GET /api/conference-rooms/available?startTime=2026-09-01T10:00:00%2B03:00&endTime=2026-09-01T14:00:00%2B03:00&capacity=50
```

### Bookings

```http
POST /api/bookings
GET  /api/bookings
GET  /api/bookings/{id}
POST /api/bookings/{id}/cancel
```

Приклад створення:

```json
{
  "conferenceRoomId": "11111111-1111-1111-1111-111111111111",
  "startTime": "2026-09-01T10:00:00+03:00",
  "endTime": "2026-09-01T14:00:00+03:00",
  "serviceIds": [1, 2]
}
```

`UserId` у request не передається. Власник бронювання визначається на сервері з JWT поточного користувача.

При створенні API повертає ID бронювання та розраховану загальну вартість.

Скасування не видаляє запис із БД. Статус змінюється на `Cancelled`, після чого цей часовий проміжок знову доступний для бронювання.

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

Звіти доступні тільки користувачам з роллю `Admin`.

Усі звіти приймають період через `from` і `to`.

Наприклад:

```http
GET /api/reports/revenue?from=2026-09-01T00:00:00%2B03:00&to=2026-10-01T00:00:00%2B03:00
```

Бронювання потрапляє у звітний період за часом початку:

```text
[from, to)
```

Скасовані бронювання у звітах не враховуються.

## Health checks

API має два окремі health endpoints:

```http
GET /health/live
GET /health/ready
```

`/health/live` перевіряє, що сам процес API працює.

`/health/ready` додатково перевіряє доступність PostgreSQL. Якщо база недоступна, readiness endpoint поверне `503 Service Unavailable`.

Health check response не містить exception details або connection string.

## Запуск через Docker

Найпростіше запустити проєкт через Docker Compose.

Спочатку потрібно створити `.env`:

```bash
cp .env.example .env
```

Приклад локальної конфігурації:

```env
# Database
POSTGRES_DB=conference_booking
POSTGRES_USER=postgres
POSTGRES_PASSWORD=change_me
ConnectionStrings__DefaultConnection=Host=localhost;Database=conference_booking;Username=postgres;Password=change_me

# JWT Security
Jwt__Key=replace_with_a_long_random_key_min_32_chars!

# Admin Account
Admin__SeedOnStartup=true
Admin__Email=admin@example.com
Admin__Password=replace_with_a_strong_password
```

Для реального середовища ці значення мають приходити із secret storage, а не з файлу в репозиторії.

Після цього:

```bash
docker compose up --build
```

Docker Compose запустить:

- PostgreSQL;
- Conference Booking API.

API буде доступне за адресою:

```text
http://localhost:5034
```

Swagger:

```text
http://localhost:5034/swagger
```

Health checks:

```text
http://localhost:5034/health/live
http://localhost:5034/health/ready
```

При першому запуску в Docker автоматично застосовуються EF Core migrations.

Щоб перевірити запуск повністю з нуля:

```bash
docker compose down -v
docker compose up --build
```

Звичайний:

```bash
docker compose down
```

не видаляє volume, тому дані PostgreSQL зберігаються між перезапусками контейнерів.

## Локальний запуск без Docker

Для локального запуску потрібен PostgreSQL.

Connection string та інші секрети не зберігаються в репозиторії. У Development зручно використовувати .NET User Secrets.

Ініціалізація:

```bash
dotnet user-secrets init --project src/WebApi
```

Connection string:

```bash
dotnet user-secrets set \
  "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=conference_booking;Username=postgres;Password=postgres" \
  --project src/WebApi
```

JWT signing key:

```bash
dotnet user-secrets set \
  "Jwt:Key" \
  "replace-with-a-long-development-key-at-least-32-characters" \
  --project src/WebApi
```

Якщо потрібен локальний Admin:

```bash
dotnet user-secrets set "Admin:SeedOnStartup" "true" --project src/WebApi
dotnet user-secrets set "Admin:Email" "admin@conference-booking.local" --project src/WebApi
dotnet user-secrets set "Admin:Password" "AdminPassword123" --project src/WebApi
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

У `docker-compose.yml` параметр увімкнений для зручного локального запуску. Для production я б не запускав міграції разом зі стартом кожного екземпляра API, а виніс їх в окремий крок deployment pipeline.

## Swagger

API задокументоване через Swagger/OpenAPI.

Swagger доступний у Development:

```text
/swagger
```

У документації є:

- endpoint-и;
- request/response schemas;
- query та route parameters;
- HTTP status codes;
- основні правила створення бронювань.

Для JWT у Swagger налаштована Bearer-схема, тому після login token можна передати через кнопку `Authorize`.

## Обробка помилок

API повертає помилки у форматі `ProblemDetails`.

Основні статуси:

```text
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
429 Too Many Requests
500 Internal Server Error
```

Для validation errors використовується `ValidationProblemDetails`.

Внутрішній текст неочікуваного exception при `500` клієнту не повертається. Повна інформація залишається в server logs, а у відповіді є `traceId`, за яким можна знайти відповідний запис.

## Rate limiting

Для різних груп endpoint-ів налаштовані окремі ліміти:

```text
операції запису = 30 запитів / хвилину
бронювання      = 10 запитів / хвилину
звіти           = 20 запитів / хвилину
```

Зараз partition виконується за IP-адресою клієнта.

Для локального запуску цього достатньо. При deployment за reverse proxy потрібно правильно налаштувати forwarded headers і довірені proxy, інакше API може бачити адресу самого proxy замість реального клієнта.

## Тести

У solution є два окремі test projects.

### Application.Tests

Unit-тестами покрита основна бізнес-логіка:

- розрахунок вартості для різних тарифних проміжків;
- тарифні межі;
- частини години;
- validation;
- створення бронювання;
- недоступні послуги;
- конфлікт бронювань;
- збереження часу в UTC;
- business timezone;
- пошук доступних залів;
- authentication handlers;
- booking ownership;
- скасування бронювання.

### Integration.Tests

Integration tests запускають справжній Web API через `WebApplicationFactory` і тимчасовий PostgreSQL через Testcontainers.

Вони перевіряють:

- запуск застосунку та застосування migrations;
- seed даних;
- registration → login → JWT → `/api/auth/me`;
- `401` для неавтентифікованих запитів;
- `403` для користувача без потрібної ролі;
- доступ Admin до захищених endpoint-ів;
- створення бронювання від імені поточного користувача;
- ізоляцію бронювань між різними Client;
- concurrent booking одного залу й часу;
- скасування бронювання та повторне використання слота;
- роботу PostgreSQL GiST exclusion constraint напряму;
- liveness та readiness health checks.

Запуск усіх тестів:

```bash
dotnet test
```

Для integration tests потрібен доступний Docker daemon, оскільки PostgreSQL запускається через Testcontainers.

## CI

Для репозиторію налаштований GitHub Actions workflow.

На push у `main` і на pull request у `main` виконуються:

```text
restore
build (Release)
unit tests
integration tests
```

Integration tests у CI так само використовують PostgreSQL через Testcontainers.

TRX-файли з результатами тестів зберігаються як artifact workflow run.

## Рішення, прийняті під час розробки

### Тарифні правила поки залишені в коді

У поточному завданні часові правила фіксовані, тому вони реалізовані в `RentalPriceCalculator`.

Якби тарифи потрібно було змінювати через адмін-панель або задавати їм період дії, я б виніс правила в БД, наприклад:

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

Якщо зал уже має бронювання, API повертає `409 Conflict` при спробі його видалити.

Це зроблено навмисно: історичні бронювання не повинні зникати каскадно разом із залом.

### Скасування замість видалення бронювання

Бронювання не видаляється фізично.

При скасуванні його статус змінюється на `Cancelled`. Історія при цьому залишається в БД, але слот перестає блокувати нові бронювання та не враховується у звітах.

### Міграції при запуску

Автоматичні міграції не ввімкнені глобально.

У Docker Compose вони використовуються для зручності локального запуску тестового проєкту.

Для production міграції краще запускати окремим контрольованим кроком deployment pipeline.

### Подвійний захист від overlap

Перевірку зайнятості залу я залишив і в Application, і в PostgreSQL.

Перша дає нормальну поведінку у звичайному сценарії, друга закриває race condition. Це навмисне дублювання бізнесового обмеження на двох рівнях, а не випадкова повторна перевірка.

## Що я б додав далі

Проєкт уже закриває вимоги тестового завдання і має кілька речей, які зазвичай з’являються вже при подальшому розвитку сервісу.

Наступними кроками я б розглядав:

- refresh tokens;
- email confirmation та password reset;
- керування тарифними правилами через БД;
- історію цін послуг і тарифів;
- pagination для великих списків;
- структуроване логування;
- OpenTelemetry, metrics і tracing;
- Redis для даних, які справді мають сенс кешувати;
- окремий production-процес запуску migrations;
- forwarded headers і trusted proxies;
- deployment workflow окремо від CI.
