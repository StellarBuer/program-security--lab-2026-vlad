# ====== IDENTITY MODULE BUSINESS LOGIC ======

## Purpose
Модуль `Identity` володіє:
- Даними облікових записів гравців (користувачів).
- Логікою реєстрації, безпечного зберігання паролів (хешування) та автентифікації (видача JWT-токенів).

**NOT here:**
- Ігровою прогресією, персонажами чи боями — належить модулям `Progress` та `Battle`.

## Entities

| Entity | Basic Fields | Description | Invariants |
|---|---|---|---|
| `UserAccount` *(Aggregate Root)* | uuid, email, passwordHash, role, createdAt | Головна сутність облікового запису гравця. | - `email` має бути унікальним у системі та мати валідний формат.<br>- `passwordHash` зберігається лише в захешованому вигляді (BCrypt).<br>- `role` визначає системні привілеї користувача (`Player`, `Admin`). За замовчуванням при реєстрації призначається `Player`. |

## Value Objects

| Value Object | Description | Invariants |
|---|---|---|
| `EmailVO` | Обгортка над електронною поштою. | - Перевіряє стандартний формат пошти (`@`, наявність домену). |
| `PasswordHashVO` | Зберігає хеш пароля. | - Заборонено передавати або зберігати відкритий текст пароля після хешування. |
| `UserRole` | Перелік системних ролей розмежування доступу (RBAC). | - Дозволені значення: `Player` (звичайний гравець), `Admin` (адміністратор). |

## Domain Policies

| Domain Policy | Description |
|---|---|
| `UniqueEmailDPolicy` | Перевіряє базу даних на предмет того, чи зареєстрована вже така пошта. |
| `PasswordStrengthDPolicy` | Вимагає мінімальну довжину пароля (наприклад, від 6 символів). |
| `RoleAssignmentDPolicy` | Забороняє пряму самостійну реєстрацію з роллю `Admin` через публічні клієнтські API. |

## Application Commands & Queries

**Commands (`AC`):**
- `RegisterUserAC` (email, password) -> створює користувача з роллю `Player`.
- `LoginUserAC` (email, password) -> повертає JWT-токен із клеймами `NameIdentifier`, `Email` та `Role`.

**Queries (`AQ`):**
- `GetCurrentUserProfileAQ` (userUuid)
- `GetAllUsersAdminAQ` () -> доступний виключно користувачам із роллю `Admin`.