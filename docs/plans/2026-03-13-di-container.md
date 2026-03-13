# DI Container Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a small Unity-friendly DI module that supports global and scene-level container registration, keyed resolution, and attribute-based injection.

**Architecture:** Keep registrations inside each container as a dictionary keyed by `(Type, string)`, while a static container stack provides override behavior from newest to oldest. Use a `MonoBehaviour` installer base class to manage scene container lifetime in `Awake` and `OnDestroy`.

**Tech Stack:** Unity 6, C#, NUnit, Unity Test Framework

---

### Task 1: Create runtime assembly and failing tests

**Files:**
- Create: `Assets/Scripts/DI/TowerBreak.DI.asmdef`
- Create: `Assets/Tests/EditMode/DI/TowerBreak.DI.Tests.asmdef`
- Create: `Assets/Tests/EditMode/DI/DIContainerTests.cs`

**Step 1: Write the failing test**

Add tests for newest-container-first resolution, fallback after removal, attribute injection, and installer lifecycle.

**Step 2: Run test to verify it fails**

Run Unity EditMode tests for `DIContainerTests`.
Expected: compile failure or test failure because runtime DI types do not exist yet.

### Task 2: Implement minimal runtime DI types

**Files:**
- Create: `Assets/Scripts/DI/DIContainer.cs`
- Create: `Assets/Scripts/DI/DIInjectAttribute.cs`
- Create: `Assets/Scripts/DI/DIInstaller.cs`

**Step 1: Write minimal implementation**

Implement registration by `(Type, string)`, static container add/remove, reverse-order lookup, and reflection-based field/property injection.

**Step 2: Run tests to verify they pass**

Run Unity EditMode tests for `DIContainerTests`.
Expected: all tests pass.

### Task 3: Refine and document usage

**Files:**
- Modify: `docs/plans/2026-03-13-di-container.md`

**Step 1: Refactor if needed**

Keep API surface small: `Register`, `Resolve`, `TryResolve`, static container stack helpers, and `DIInstaller`.

**Step 2: Verify behavior**

Confirm runtime usage is:
- program startup container registered once
- scene installer creates and removes its container automatically
- injection resolves from newest registered container first
