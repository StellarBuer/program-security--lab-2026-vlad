# Changelog

Усі висвітлені зміни у документації та архітектурі проєкту фіксуються у цьому файлі.

## [2026-11-16] [FEAT] Впровадження системи ліцензування Freeware з перевіркою цифрового підпису RSA-PSS
- **Доменне ядро (`Game.Domain`):**
  - Створено виняток `LicenseViolationException : AGameEngineException` (`CORE:CORE_LICENSE_VIOLATION`) для фіксації порушень ліцензійних умов або підробки підпису.
- **Шар застосунку (`Game.Application`):**
  - Створено інтерфейс порту `ILicenseService` та контракт моделі `LicenseData` (модель Freeware, перелік дозволених функцій, права некомерційного використання).
- **Шар інфраструктури (`Game.Infrastructure`):**
  - Реалізовано адаптер `RsaPssLicenseService : ILicenseService` на базі асиметричного алгоритму `RSA-PSS SHA-256` із довжиною ключа 3072 біти (`SubjectPublicKeyInfo` PEM).
  - Згенеровано пару ключів (`private.pem`, `public.pem`) та сформовано цифровий підпис `license.sig` для еталонного файлу `license.json`.
- **Презентаційний шар (`Game.Api`):**
  - Створено `LicenseController` (`/api/license`) з ендпоїнтами:
    - `GET /api/license/info` — перевірка статусу та реквізитів поточної ліцензії;
    - `POST /api/license/verify` — верифікація довільного файлу ліцензії та його підпису;
    - `GET /api/license/feature/{featureName}` — перевірка дозволів на окремі модулі рушія.
  - Зареєстровано `ILicenseService` у DI-контейнері (`Program.cs`).
  - Оновлено `GlobalExceptionHandlingMiddleware`: трансляція `LicenseViolationException` $\rightarrow$ HTTP 403 Forbidden.
- **Тестування:**
  - Проведено тестування валідної ліцензії (успішна верифікація) та навмисної модифікації параметрів (виявлення атаки підміни та негайна відмова в доступі).

## [2026-11-16] [DOCS] Декларування моделі розповсюдження Freeware та механізмів контролю ліцензії
- **Політика безпеки (`security_policy.md`):**
  - Додано розділ 4.4 "Software Distribution Model and Licensing Control": зафіксовано модель Freeware, ліцензійні обмеження, криптографічний механізм RSA-PSS SHA-256 (3072 біти), ізоляцію приватного ключа та політику захисту компонентів від підміни.

## [2026-11-16] [FEAT] Впровадження механізмів контролю цілісності та автентичності
- **Доменне ядро (`Game.Domain`):**
  - Створено виняток `DataIntegrityViolationException : AGameEngineException` (`CORE:CORE_INTEGRITY_VIOLATION`) для сигналізації про невідповідність контрольних сум або порушення автентичності даних.
- **Шар застосунку (`Game.Application`):**
  - Створено інтерфейс порту `IIntegrityService` (обчислення та перевірка SHA-256 для статичних конфігурацій, обчислення та перевірка HMAC-SHA256 для динамічних дій/повідомлень).
- **Шар інфраструктури (`Game.Infrastructure`):**
  - Реалізовано `IntegrityService : IIntegrityService` із використанням `CryptographicOperations.FixedTimeEquals` для захисту від Timing Attacks.
  - Оновлено `AesGcmSymmetricEncryptionService`: помилка перевірки автентифікаційного тегу AES-GCM транслюється у `DataIntegrityViolationException`.
- **Презентаційний шар (`Game.Api`):**
  - Створено `IntegrityController` (`/api/integrity`) для валідації конфігурацій гри (SHA-256) та верифікації дій гравців (HMAC-SHA256).
  - Зареєстровано `IIntegrityService` у DI (`Program.cs`).
  - Оновлено `GlobalExceptionHandlingMiddleware`: додано мапінг `DataIntegrityViolationException` $\rightarrow$ HTTP 422 Unprocessable Entity.
- **Тестування:**
  - Проведено тестування штатної роботи та навмисної модифікації (SHA-256 Tampering, HMAC Tampering, JWT Tampering, AEAD SQLite Tampering). Усі спроби модифікації успішно виявлені та нейтралізовані системою.

## [2026-11-16] [DOCS] Декларування політики контролю цілісності та автентичності
- **Політика безпеки (`security_policy.md`):**
  - Додано розділ 4.3 "Data Integrity and Authenticity Controls" з описом механізмів (SHA-256 для статичних асетів, HMAC-SHA256 для токенів та ігрових дій, AEAD AES-256-GCM для PII у БД), захисту від Timing Attacks (`FixedTimeEquals`) та політики Fail-Secure.

## [2026-11-15] [FEAT] Реалізація симетричного шифрування (AES-256-GCM) та Blind Indexing (HMAC-SHA256)
- **Доменне ядро (`Game.Domain`):**
  - Сутність `UserAccount` розширено полем `EmailBlindIndex` та бізнес-методом `SetEmailBlindIndex` для підтримки безпечної індексації.
- **Шар застосунку (`Game.Application`):**
  - Створено інтерфейс порту `ISymmetricEncryptionService` (AEAD Encrypt/Decrypt на базі AES-256-GCM із 128-бітним тегом).
  - Створено інтерфейс порту `IBlindIndexService` для детермінованого обчислення сліпих індексів без розкриття вихідного тексту.
  - Оновлено `AuthService`: при реєстрації гравця генерується сліпий індекс `EmailBlindIndex` для забезпечення унікальності та швидкого пошуку.
- **Шар інфраструктури (`Game.Infrastructure`):**
  - Реалізовано адаптер `AesGcmSymmetricEncryptionService` (AES-256-GCM, унікальний 96-бітний Nonce для кожного шифрування, формат `enc:v1:{Base64}`). Забезпечено зворотну сумісність із немігрованими відкритими даними.
  - Реалізовано адаптер `HmacBlindIndexService` (HMAC-SHA256 для $O(1)$ індексного пошуку).
  - Оновлено `GameDbContext`: налаштовано прозорий EF Core `ValueConverter` для шифрування/розшифрування поля `UserAccount.Email` та додано індекс на `EmailBlindIndex`.
  - Оновлено `UserRepository`: метод `GetByEmailAsync` виконує пошук через `EmailBlindIndex`.
  - Згенеровано та застосовано міграцію EF Core `20261115204834_AddEmailBlindIndexAndEncryption`.
- **Конфігурація та API (`Game.Api`):**
  - Зареєстровано `ISymmetricEncryptionService` та `IBlindIndexService` у DI.
  - `appsettings.json`: додано секцію `Security` із 256-бітними ключами для шифрування та сліпого індексування.

## [2026-11-15] [DOCS] Стратегія симетричного криптографічного захисту (AES-256-GCM + Blind Indexing)
- **Політика безпеки (`security_policy.md`):**
  - Додано розділ 4.2 "Symmetric Cryptographic Protection Strategy": визначено об'єкти захисту (PII у стані Data at Rest, конфігураційні секрети), обґрунтовано вибір AES-256-GCM (AEAD), описано модель Envelope Encryption та життєвий цикл ротації ключів (90–180 днів, версіонування ключів, ліниве перешифрування).
- **Модель загроз (`thread_model.md`):**
  - Оновлено рядок D1 Game Database в STRIDE: витік дампу бази (Information Disclosure) та підробку записів (Tampering) нейтралізовано шифруванням AES-256-GCM та валідацією 128-бітного тегу автентичності.
- **Специфікація модуля Identity (`internal_spec/Identity/business_logic.md`):**
  - Зафіксовано шифрування `UserAccount.Email` у стані спокою та використання поля `emailBlindIndex` (HMAC-SHA256) для швидкого пошуку й контролю унікальності.

## [2026-11-13] [REFACTOR] Перехід презентаційного шару з Minimal API на архітектуру MVC Контролерів
- **Презентаційний шар (`Game.Api`):**
  - Виконано рефакторинг Presentation Layer відповідно до `docs/policy/project_policy.md` (розділ 2: *Presentation: ASP.NET Controllers*). Замінено інлайн-ендпоінти Minimal API на структуровані контролери ASP.NET Core MVC (`ControllerBase`, `[ApiController]`).
  - Створено `HealthController` (`/api/health`) для моніторингу працездатності бекенду.
  - Створено `AuthController` (`/api/auth`):
    - `POST /api/auth/register` — реєстрація користувача з роллю `Player`.
    - `POST /api/auth/login` — видача JWT-токена.
    - `GET /api/auth/me` — захищений ендпоінт перегляду профілю із валідацією моделі власності ресурсу (OBAC).
  - Створено `AdminController` (`/api/admin`):
    - `GET /api/admin/users` — захищений системною RBAC-політикою `[Authorize(Policy = "AdminOnly")]` для вибірки списку користувачів.
  - Оновлено `Program.cs`: додано `builder.Services.AddControllers()` та `app.MapControllers()`. Код запуску очищено від інлайн-маршрутизації.
  - Збережено сумісність з `GlobalExceptionHandlingMiddleware`, налаштуваннями Swagger/OpenAPI та JWT Bearer автентифікацією.

## [2026-11-11] [FEAT] Реалізація гібридної моделі розмежування доступу (RBAC + OBAC)
- **Доменне ядро (`Game.Domain`):**
  - Додано перелічення `UserRole` (`Player = 1`, `Admin = 2`) для типізації системних ролей користувачів.
  - Доменну сутність `UserAccount` розширено полем `Role` та методом зміни ролі `ChangeRole(UserRole newRole)`.
  - Створено доменний виняток `ForbiddenActionException : AGameEngineException` (контекст `CORE`, код `CORE_FORBIDDEN_ACTION`), що представляє спробу несанкціонованої дії.
- **Шар застосунку (`Game.Application`):**
  - Створено інтерфейс порту `ICurrentUserService` для отримання контексту автентифікованого користувача (`UserId`, `Email`, `Role`, `IsInRole`) без зв'язування з HTTP-інфраструктурою.
  - Розширено інтерфейс `IUserRepository` методом `GetAllAsync`.
  - Оновлено DTO `UserResponseDto` полем `UserRole Role`.
  - Оновлено сервіс `AuthService`:
    - Реєстрація `RegisterAsync` гарантує призначення ролі `UserRole.Player` (заборона самовільного підвищення привілеїв).
    - `GetProfileAsync`: впроваджено перевірку OBAC (Ownership-Based Access Control) — гравець отримує доступ лише до власного профілю (`UserId == targetUserId`), адміністратори мають глобальний доступ.
    - `GetAllUsersAdminAsync`: впроваджено системну перевірку RBAC (Role-Based Access Control) на наявність ролі `Admin`.
- **Шар інфраструктури (`Game.Infrastructure`):**
  - Реалізовано сервіс `CurrentUserService : ICurrentUserService`, що безпечно вилучає ідентифікатор та клейми ролі з `IHttpContextAccessor`.
  - `JwtTokenService`: додано клейм ролі `ClaimTypes.Role` до структури JWT-токена.
  - `UserRepository`: реалізовано вибірку `GetAllAsync` з бази даних.
  - `GameDbContext`: налаштовано мапінг властивості `Role` до цілочисельного типу через Fluent API.
  - Створено та успішно застосовано міграцію EF Core `20261111202054_AddUserRole` для додавання стовпця `Role` з дефолтним значенням `1` (`Player`) у таблицю `UserAccounts`.
- **Презентаційний шар (`Game.Api`):**
  - Зареєстровано `IHttpContextAccessor` та `ICurrentUserService` у DI.
  - Налаштовано політику авторизації `AdminOnly` в `AddAuthorization`.
  - Створено захищений маршрут `GET /api/admin/users`, захищений політикою `AdminOnly` (RBAC).
  - Оновлено `GET /api/auth/me` з підтримкою перевірки контексту користувача.
  - `GlobalExceptionHandlingMiddleware`: додано перехоплення `ForbiddenActionException` з поверненням HTTP 403 Forbidden без витоку внутрішніх деталей.

## [2026-11-11] [DOCS] Проектування гібридної моделі розмежування доступу (RBAC + OBAC)
- **Політика безпеки (`security_policy.md`):**
  - Додано розділ 4.1 "Access Control Model" з описом дворівневої моделі: системний RBAC (`Player`, `Admin`) та ресурсний OBAC (Ownership-Based Access Control) для ізоляції ігрових об'єктів.
- **Модель загроз (`thread_model.md`):**
  - Оновлено аналіз STRIDE: загрозу Elevation of Privilege (EoP) зв'язано з RBAC-політиками ASP.NET Core, а загрозу Tampering/BOLA (IDOR) — з перевірками власності OBAC.
- **Специфікації модулів (`internal_spec`):**
  - `Identity`: розширено модель `UserAccount` ролями `UserRole` (`Player`, `Admin`), додано клейм ролі до JWT та політику `RoleAssignmentDPolicy`.
  - `Battle`: додано доменну політику `BattleParticipantDPolicy` (OBAC) для валідації права на хід у конкретній бойовій сесії.

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