# Changelog

Усі висвітлені зміни у документації та архітектурі проєкту фіксуються у цьому файлі.

## [2026-11-10] [FEAT] Реалізація JWT-автентифікації та захисту API (Identity Module)
- **Шар застосунку (`Game.Application`):**
  - Створено інтерфейс `ITokenService` для абстракції випуску JWT-токенів.
  - Створено команду авторизації `LoginUserAC` та DTO результату автентифікації `AuthResultDto`.
  - Розширено `AuthService`: додано сценарії `LoginAsync` (перевірка пароля через BCrypt, генерація токена) та `GetProfileAsync` (отримання профілю гравця за ідентифікатором).
- **Шар інфраструктури (`Game.Infrastructure`):**
  - Реалізовано сервіс `JwtTokenService : ITokenService` з використанням бібліотек `System.IdentityModel.Tokens.Jwt` та `Microsoft.IdentityModel.Tokens`.
  - Налаштовано підписання токенів алгоритмом `HmacSha256` із клеймами (`Sub`, `Email`, `Jti`, `NameIdentifier`).
  - Додано конфігурацію параметрів `Jwt` у `appsettings.json` (секретний ключ, Issuer, Audience, час життя токена).
- **Презентаційний шар (`Game.Api`):**
  - Підключено та налаштовано middleware `JwtBearer` (`AddAuthentication`, `AddJwtBearer`, `UseAuthentication`, `UseAuthorization`).
  - Реалізовано ендпоінт входу `POST /api/auth/login`.
  - Реалізовано захищений ендпоінт `GET /api/auth/me` із вимогою валідного токена (`.RequireAuthorization()`).
  - Інтегровано підтримку передачі Bearer токенів у Swagger UI через `AddSecurityDefinition` та `AddSecurityRequirement`.
  - Додано обробку доменного винятку `UserNotFoundException` у `GlobalExceptionHandlingMiddleware` (HTTP 404 Not Found).

## [2026-11-10] [REFACTOR] Впровадження принципів Clean Architecture та оптимізація чистоти коду
- **Розвантаження Presentation Layer (`Game.Api`):**
  - Ендпоінт `POST /api/auth/register` переведено на патерн Thin Endpoint; пряму роботу з `GameDbContext` та ручне хешування винесено у шар застосунку (`IAuthService`).
- **Шар застосунку (`Game.Application`):**
  - Створено інтерфейси порту інфраструктури: `IUserRepository` (абстракція доступу до даних) та `IPasswordHasher` (абстракція криптографії).
  - Створено DTO `UserResponseDto` та команду `RegisterUserAC`.
  - Реалізовано сервіс `AuthService`, що координує реєстрацію, перевірку надійності пароля, бізнес-інваріанти та виклик репозиторію.
- **Доменне ядро (`Game.Domain` / `SharedKernel`):**
  - Реалізовано абстрактний клас `AValueObject` для автопорівняння об'єктів-значень за компонентами (`GetEqualityComponents`).
  - `GridPositionVO` переведено на успадкування від `AValueObject`.
  - Впроваджено ієрархію винятків на базі абстрактного `AGameEngineException` із кодами помилок та контекстів (`CORE`, `IDNT`, `BTTL`).
  - Додано класи доменних винятків: `DomainValidationException`, `UserAlreadyExistsException`, `InvalidCredentialsException`, `WeakPasswordException`.
  - Оновлено `UserAccount` для викидання типізованих доменних винятків замість системних `ArgumentException`.
- **Безпека та Інфраструктура (`Game.Infrastructure`):**
  - Замінено демонстраційний SHA-256 на промисловий криптографічний алгоритм `BCrypt` із солінням (робочий фактор 11) у класі `BCryptPasswordHasher` (пакет `BCrypt.Net-Next`).
  - Реалізовано репозиторій `UserRepository : IUserRepository` на базі EF Core.
- **Глобальна обробка помилок та відповідність Security Policy:**
  - Реалізовано `GlobalExceptionHandlingMiddleware`, що автоматично перехоплює `AGameEngineException` і транслює їх у клієнтські HTTP-статуси (400, 401, 409), одночасно маскуючи системні помилки під HTTP 500 для запобігання витоку StackTrace та структури БД (захист від STRIDE Information Disclosure).
- **Стандартизація коду та статичний аналіз (Roslyn):**
  - Створено кореневий `.editorconfig` із правилами оформлення C#, сортування директив та правилами неймінгу (`I*`, `_camelCase`, `PascalCase`).
  - Додано `Directory.Build.props` із параметром `EnforceCodeStyleInBuild`, що інтегрує лінтер Roslyn безпосередньо у процес збірки проєкту.

## [2026-11-09] [FEAT] Ініціалізація Clean Architecture, доменного ядра, EF Core та API (Лабораторна №4)
- **Архітектурний скелет:**
  - Розгорнуто структуру Clean Architecture: `Game.Domain`, `Game.Application`, `Game.Infrastructure`, `Game.Api` та клієнт `Game.ConsoleClient` у рішенні `Game.slnx`.
- **Game.Domain:**
  - Створено базовий клас сутностей `ABaseEntity` з автоматичною генерацією унікального ідентифікатора `Guid Id`.
  - Реалізовано сутність користувача `UserAccount` з інваріантами валідації пошти та збереження хешу пароля.
  - Реалізовано сутність `Character` з базовими бойовими характеристиками (`Health`, `Attack`, `Defense`, `Speed`), списком навичок та методом `TakeDamage`.
  - Реалізовано сутність `Skill` та перелік типів навичок `SkillType` (`Normal`, `Enhanced`, `Unique`).
  - Реалізовано об'єкт-значення `GridPositionVO` з валідацією меж загальної сітки 10х5 (дві зони 5х5) та перевіркою коректності розстановки команд у квадратах 3х3 (`IsValidForPlacement`).
- **Game.Infrastructure:**
  - Підключено Entity Framework Core для SQLite.
  - Створено `GameDbContext` із Fluent API налаштуванням сутності `UserAccount` (унікальний індекс за полем `Email`).
  - Згенеровано початкову міграцію БД `InitialCreate` та ініціалізовано локальну базу даних `game.db`.
- **Game.Api:**
  - Створено Minimal API застосунок ASP.NET Core з підтримкою Swagger UI.
  - Реалізовано службовий ендпоінт діагностики `GET /api/health`.
  - Реалізовано робочий ендпоінт реєстрації `POST /api/auth/register` із валідацією полів, перевіркою дублікатів пошти, хешуванням пароля та збереженням запису в SQLite.
- **Game.ConsoleClient:**
  - Ініціалізовано консольний проєкт для майбутнього клієнтського інтерфейсу.

## [2026-11-09] [DOCS] Політики безпеки, конфіденційності та модель загроз STRIDE (Лабораторна №3)
- Створено політику безпеки `security_policy.md` із закріпленням принципу серверного авторитету (Server Authority), правила найменших привілеїв, вимог до криптографії та збереження конфігураційних секретів.
- Створено політику конфіденційності `privacy_policy.md` щодо обробки облікових записів та ігрової телеметрії відповідно до стандартів захисту персональних даних.
- Розроблено комплексну модель загроз `thread_model.md` за методологією **STRIDE**:
  - Побудовано елементи DFD діаграми: `E1 Player`, `P1 Console Client`, `P2 Web API Server`, `D1 Game Database`, потік даних `F1` та межу довіри `TB1 Trust Boundary`.
  - Проведено детальний аналіз ризиків Spoofing, Tampering, Repudiation, Information Disclosure, Denial of Service, Elevation of Privilege.
- Оновлено специфікації бізнес-логіки bounded contexts: `Identity`, `Progress`, `SharedKernel`.

## [2026-11-08] [DOCS] Створення початкової документації (Лабораторна №2)
- Створено структуру директорії `docs/`.
- Додано загальну політику розробки `project_policy.md` з орієнтацією на C# .NET та Clean Architecture / DDD.
- Описано бізнес-логіку модуля `Battle` (покроковий бій, сітка 5х5, навички).
- Створено `contexts_registry.md` та `changelog.md`.