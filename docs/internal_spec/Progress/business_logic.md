# ====== PROGRESS MODULE BUSINESS LOGIC ======

## Purpose
Модуль `Progress` володіє:
- Колекцією персонажів гравця та їхнім поточним станом (рівень, досвід).
- Інвентарем, валютою/нагородами за проходження рівнів або перемоги в боях.

**NOT here:**
- Правилами розрахунку покрокового бою — належить модулю `Battle`.
- Даними автентифікації — належить `Identity`.

## Entities

| Entity | Basic Fields | Description | Invariants |
|---|---|---|---|
| `PlayerProgress` *(Aggregate Root)* | uuid, userRef, level, experience, currency | Головний профіль прогресу користувача. | - Досвід та валюта не можуть бути від'ємними. |
| `PlayerCharacter` *(internal Entity)* | uuid, playerProgressRef, characterTemplateRef, level, currentStats | Персонаж, розблокований та прокачаний гравцем. | - Рівень персонажа не може перевищувати поточний рівень аккаунту гравця (`PlayerProgress.level`). |

## Value Objects

| Value Object | Description | Invariants |
|---|---|---|
| `ExperienceVO` | Кількість досвіду з правилами нарахування. | - Строго $\ge 0$. |
| `CharacterStatsVO` | Базові параметри персонажа (Health, Attack, Defense, Speed) з урахуванням поточного рівня. | - З ростом рівня параметри масштабуються за формулою. |

## Domain Policies

| Domain Policy | Description |
|---|---|
| `LevelUpDPolicy` | Перевіряє, чи достатньо досвіду у гравця для підвищення рівня або прокачки персонажа. |

## Application Commands & Queries

**Commands (`AC`):**
- `UpgradeCharacterAC` (playerUuid, characterUuid)
- `ClaimBattleRewardsAC` (playerUuid, battleResultDto)

**Queries (`AQ`):**
- `GetPlayerInventoryAQ` (playerUuid) — повертає список розблокованих персонажів та їх параметри.