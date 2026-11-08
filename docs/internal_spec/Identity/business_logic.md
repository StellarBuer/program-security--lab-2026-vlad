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
| `UserAccount` *(Aggregate Root)* | uuid, email, passwordHash, createdAt, status | Головна сутність облікового запису гравця. | - `email` має бути унікальним у системі та мати валідний формат.<br>- `passwordHash` зберігається лише в захешованому вигляді (Argon2 / BCrypt). |

## Value Objects

| Value Object | Description | Invariants |
|---|---|---|
| `EmailVO` | Обгортка над електронною поштою. | - Перевіряє стандартний формат пошти (`@`, наявність домену). |
| `PasswordHashVO` | Зберігає хеш пароля. | - Заборонено передавати або зберігати відкритий текст пароля після хешування. |

## Domain Policies

| Domain Policy | Description |
|---|---|
| `UniqueEmailDPolicy` | Перевіряє базу даних на предмет того, чи зареєстрована вже така пошта. |
| `PasswordStrengthDPolicy` | Вимагає мінімальну довжину пароля (наприклад, від 6 символів). |

## Application Commands & Queries

**Commands (`AC`):**
- `RegisterUserAC` (email, password)
- `LoginUserAC` (email, password) -> повертає JWT-токен.

**Queries (`AQ`):**
- `GetCurrentUserProfileAQ` (userUuid)