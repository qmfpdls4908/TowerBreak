# Combat System Fix Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Fix critical combat system bugs including EnemyRow data mutation, animation-damage timing mismatch, and state management issues.

**Architecture:** Separate runtime enemy state from data, synchronize animation with damage application, consolidate player action state management.

**Tech Stack:** Unity 6, C#, TowerBreak Combat System

---

## Task 1: Fix EnemyRow Data Mutation Bug

**Files:**
- Modify: `Assets/Scripts/Combat/EnemyController.cs`

**Step 1: Add runtime health field**

Add private field to store runtime health:
```csharp
private int currentHealth;
```

**Step 2: Initialize runtime health in Initialize()**

**Step 3: Modify TakeDamage to use runtime health**

Replace health modification logic to use `currentHealth` instead of `enemyData.Health`

**Step 4: Add property to access current health**

**Step 5: Commit**

---

## Task 2: Fix Animation-Damage Timing Mismatch

**Files:**
- Modify: `Assets/Scripts/Combat/PlayerController.cs`
- Modify: `Assets/Scripts/Combat/BattleSceneInitializer.cs`

**Step 1: Add animation completion event to PlayerController**

**Step 2: Modify BattleSceneInitializer to delay damage until animation completes**

**Step 3: Remove old HandleAttackAsync method**

**Step 4: Add helper method to find enemy controller by ID**

**Step 5: Commit**

---

## Task 3: Consolidate Player Action State Management

**Files:**
- Modify: `Assets/Scripts/Combat/BattleSceneInitializer.cs`

**Step 1: Remove isAttackInFlight field**

**Step 2: Use PlayerController state exclusively**

**Step 3: Commit**

---

## Task 4: Fix Fire-and-Forget Async Pattern

**Files:**
- Modify: `Assets/Scripts/Combat/BattleSceneInitializer.cs`

**Step 1: Make HandleEnemyDefeated synchronous**

**Step 2: Remove unnecessary async/await**

**Step 3: Commit**

---

## Task 5: Fix CombatManager Performance

**Files:**
- Modify: `Assets/Scripts/Combat/CombatManager.cs`

**Step 1: Cache PlayerController reference**

**Step 2: Use cached reference in CheckPlayerWallCollision**

**Step 3: Commit**

---

## Task 6: Fix EnemyController CombatManager Creation

**Files:**
- Modify: `Assets/Scripts/Combat/EnemyController.cs`

**Step 1: Remove auto-creation logic**

**Step 2: Log error instead**

**Step 3: Commit**

---

## Summary of Fixes

1. **Critical**: EnemyRow data mutation - use runtime health field
2. **High**: Animation-damage timing - sync with animation completion event
3. **Medium**: State management - consolidate to PlayerController
4. **Medium**: Async patterns - remove fire-and-forget
5. **Low**: Performance - cache PlayerController reference
6. **Low**: CombatManager creation - remove auto-creation
