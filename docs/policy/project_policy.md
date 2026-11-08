# Політика проєкту — Tactics Game Engine (.NET Core / DDD)

| | |
| --- | --- |
| Назва | Політика проєкту — Tactics Game Engine |
| Версія | 0.1.0 |
| Статус | Draft |
| Класифікація | Internal |
| Останнє оновлення | 2026-11-08 |
| Власник | Vladyslav |

> Ця політика визначає обов'язкові вимоги до коду та архітектури сервісу покрокової тактичної гри. Сервер реалізує бізнес-логіку бою, прогресу та автентифікації, а клієнт (Console/UI) виступає лише засобом відображення та надсилання дій.

## 1. Технологічний стек
- **Мова розробки:** C# 12 / .NET 10
- **Веб-фреймворк:** ASP.NET Core Web API / SignalR (для авторизації, REST API та реалтайм бою)
- **ORM & БД:** Entity Framework Core (PostgreSQL або SQL Server)
- **Аналізатори та лінтери:** Roslyn Analyzers, StyleCop / CSharpier
- **Тестування:** xUnit, FluentAssertions, Moq
- **Контроль версій:** Git

## 2. Архітектурні принципи (Clean Architecture + DDD)
1. **Відокремлення шарів (Strict Layering):**
   - **Domain:** Чистий C#. Жодних залежностей від EF Core, ASP.NET Core чи зовнішніх бібліотек. Містить сутності, VO, доменні сервіси, інваріанти та події.
   - **Application:** Юзкейси (Commands/Queries), оркестрація домену, CQRS.
   - **Infrastructure:** Реалізація `OutsourceContract` (репозиторії EF Core, JWT сервіси, хешування).
   - **Presentation (API):** ASP.NET Controllers / SignalR Hubs.
2. **Серверний авторитет (Server Authority):** 
   - Клієнт ніколи не приймає рішень щодо нарахування досвіду, ушкоджень у бою чи валідності ходу. Всі дії розраховуються і підтверджуються на сервері.

## 3. Політика неймінгу (C# Conventions)
- **Класи, Інтерфейси, Енуми, Властивості, Методи:** `PascalCase`.
- **Локальні змінні, параметри:** `camelCase`.
- **Приватні поля класів:** `_camelCase`.
- **Префікси та суфікси:**
  - Інтерфейси: префікс `I` (`IUserRepository`, `ICharacterRepository`).
  - Абстрактні класи: префікс `A` (`ABaseEntity`, `AValueObject`).
  - Value Objects: суфікс `VO` (`PositionVO`, `StatsVO`).
  - Domain Events: суфікс `DE` (`CharacterLevelUpgradedDE`, `BattleFinishedDE`).
  - Application Commands / Queries: суфікс `AC` / `AQ` (`StartBattleAC`, `GetPlayerInventoryAQ`).
  - Use Cases / Handlers: суфікс `UC` або `Handler`.
  - Доменні політики: суфікс `DPolicy` (`ValidPlacementDPolicy`).

## 4. Обробка помилок та винятки
- Усі доменні та аплікаційні помилки успадковуються від `AGameEngineException`.
- Заборонено протікання системних чи EF Core винятків на шар Presentation. Всі інфраструктурні помилки перехоплюються та обгортаються у відповідні внутрішні винятки.

## 5. Політика тестування
- **Domain Layer:** 100% юніт-покриття бізнес-правил (розрахунок урону, валідація позицій на сітці 5х5, переходи рівнів).
- **Application Layer:** Юніт-тестування сценаріїв з моками репозиторіїв.

## 6. Git Commit Policy
У проєкті використовується спрощений формат **Conventional Commits**:
- `init:` — початкове налаштування.
- `docs:` або `[DOCS]` — робота з документацією в папці `docs/`.
- `feat:` — нова фіча на сервері або клієнті.
- `fix:` — виправлення багів.
- `refactor:` — рефакторинг коду.
- `test:` — покриття тестами.

Приклади коммітів:
- `docs: Додав бізнес-логіку модулів Identity та Progress`
- `[DOCS] Add Battle module specification`
- `feat: Реалізував розрахунок ініціативи в бою`