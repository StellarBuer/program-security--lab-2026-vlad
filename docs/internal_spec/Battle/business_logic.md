# ====== BATTLE MODULE BUSINESS LOGIC ======

## Purpose

Модуль `Battle` відповідає за:
- Створення та розрахунок тактичного покрокового бою між двома командами.
- Валідацію розстановки персонажів на ігровій сітці.
- Розрахунок черговості ходів, використання навичок, розрахунок шкоди/захисту та зміну стану бійців.

**NOT here:**
- Збереження постійного прогресу гравця (досвід, інвентар) — належить модулю `Progress`.
- Автентифікація гравців — належить модулю `Identity`.

## Entities

| Entity | Basic Fields | Description | Invariants |
|---|---|---|---|
| `BattleSession` *(Aggregate Root)* | uuid, playerTeamRef, enemyTeamRef, currentTurn, status | Агрегат, що керує повним циклом одного бою від розстановки до фіналу. | - Створюється в статусі `SETUP`.<br>- Перехід у `IN_PROGRESS` можливий лише після підтвердження валідної розстановки обох команд.<br>- Не можна робити хід у завершеному бою (`FINISHED`). |
| `BattleUnit` *(internal Entity)* | uuid, characterRef, currentStats, position, skills | Конкретний боєць на полі бою з поточними динамічними параметрами. | - `currentStats.Health` не може бути більшим за `maxHealth`.<br>- Смерть настає при `Health <= 0`. |

## Value Objects

| Value Object | Description | Invariants |
|---|---|---|
| `GridPositionVO` | Координати клітинки на ігровій сітці. | - $X \in [0, 9]$, $Y \in [0, 4]$ (Загальне поле 10х5 = два блоки 5х5 side-by-side).<br>- Для блоку гравця (X: 0..4): дозволений зони розстановки (3х3 посередині) відповідає $X \in [1, 3], Y \in [1, 3]$.<br>- Для блоку суперника (X: 5..9): зона розстановки відповідає $X \in [6, 8], Y \in [1, 3]$. |
| `StatsVO` | Набір базових/поточних характеристик (Health, Attack, Defense, Speed). | - Усі показники $\ge 0$.<br>- `Speed` визначає черговість ходу в ініціативі. |
| `SkillVO` | Опис навички (Type: Normal, Enhanced, Unique). | - Кожна навичка має тип, перезарядку (cooldown) та коефіцієнт дії. |

## Domain Policies

| Domain Policy | Description |
|---|---|
| `ValidPlacementDPolicy` | Перевіряє, чи знаходиться обрана координата `GridPositionVO` у дозволеному квадраті 3х3 всередині відповідного блоку 5х5, та чи не зайнята вона іншим бійцем. |
| `SkillExecutionDPolicy` | Перевіряє, чи боєць живий, чи не перебуває навичка на перезарядці (cooldown) та чи досяжна ціль. |

## Domain Events

| Event | Carries | Notes |
|---|---|---|
| `BattleStartedDE` | battleUuid, playerTeamUuid, enemyTeamUuid | Публікується при переході бою в `IN_PROGRESS`. |
| `UnitAttackedDE` | battleUuid, attackerUnitUuid, targetUnitUuid, damageDealt | Фіксує результати атаки/навички. |
| `BattleFinishedDE` | battleUuid, winnerPlayerUuid, rewardsSummary | Сигналізує модулю `Progress` про необхідність нарахувати нагороди. |

## Application Commands & Queries

**Commands (`AC`):**
- `PlaceCharacterAC` (battleUuid, unitUuid, targetPosition)
- `ExecuteSkillAC` (battleUuid, actorUnitUuid, skillSlot, targetPosition)

**Queries (`AQ`):**
- `GetBattleStateAQ` (battleUuid) — повертає повне поле (50 клітинок) та стан усіх бійців для консольного клієнта.