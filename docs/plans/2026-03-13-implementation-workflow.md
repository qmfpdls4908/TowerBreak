# Implementation Workflow Guide

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this workflow consistently across milestones.

**Goal:** Standardize how implementation areas are researched, documented, executed, checked off, and archived so work can continue cleanly across sessions.

**Architecture:** Treat each implementation area as a milestone package with its own references, research, checklist, and status. Keep one parent overview document for the feature, but move completed milestone packages into an archive folder instead of leaving finished work mixed with active work.

**Tech Stack:** Markdown docs under `docs/plans/`, Unity 6, C#, NUnit, Unity Test Framework

---

## 1. Folder structure rules

For a feature area such as `docs/plans/2026-03-13-tower-breaker-vertical-slice/`, use this structure:

```text
docs/plans/2026-03-13-tower-breaker-vertical-slice/
  00-overview.md
  milestones/
    data-schema/
      README.md
      references.md
      research.md
      checklist.md
    addressables-provider/
      README.md
      references.md
      research.md
      checklist.md
  archive/
    data-schema/
      README.md
      references.md
      research.md
      checklist.md
```

Rules:
- active work lives under `milestones/`
- completed and verified work moves to `archive/`
- `00-overview.md` remains the top-level entry point
- milestone folder names should be short, stable, and feature-oriented

## 2. Required sections per milestone

Each milestone must include these sections, whether split across files or combined into one small file:

- `Goal`
- `Reference Documents`
- `Code And Tests Researched`
- `Research Findings`
- `Implementation Guardrails`
- `Checklist`
- `Verification`
- `Manual Follow-Ups`

Minimum expectations:
- `Reference Documents` must link to exact repo files
- `Research Findings` must summarize existing patterns before code changes begin
- `Checklist` must show current progress state
- `Verification` must record the exact commands run and their real status

## 3. Required workflow per milestone

### Step 1: Create the milestone folder

Create the milestone folder under `milestones/` before implementation.

### Step 2: Research first

Before writing code, inspect and record:
- feature plan docs
- design/direction docs
- nearby implementation files
- nearby tests
- asmdefs and reference boundaries
- previous milestone outputs that this work depends on

Write the findings into `research.md` or the equivalent research section.

### Step 3: Set implementation guardrails

Record local rules such as:
- keep Addressables references as string keys
- do not cross runtime/editor asmdef boundaries
- write failing tests first where feasible
- defer Unity-only manual work into `Manual Follow-Ups`

### Step 4: Implement with checklist updates

Update `checklist.md` continuously while working.

Recommended statuses:
- `Todo`
- `Doing`
- `Blocked`
- `Done`

Checklist items should include both coding and verification tasks.

### Step 5: Verify before marking complete

Record:
- exact build/test command
- whether it passed, failed, or was blocked
- why it was blocked if applicable

### Step 6: Archive completed milestone

When implementation and verification are complete:
- move the milestone folder from `milestones/` to `archive/`
- update `00-overview.md` with:
  - archived milestone name
  - archived folder path
  - completion date/status
  - next active milestone

## 4. Parent overview responsibilities

The parent overview document must always show:
- current active milestone
- completed milestone list
- archived milestone paths
- current blockers
- next recommended action

This overview is the first file to read when resuming a later session.

## 5. Handoff rules

At the end of each working session:
- update the active milestone checklist
- update the parent overview
- state any blocked manual Unity/editor tasks clearly
- ensure the next step is written down in docs, not only in chat

## 6. Practical guidance for this repository

- Prefer milestone folders for feature work that spans multiple sessions.
- Keep GameData, DI, and EventBus research tied to the milestone that consumes them.
- If Unity is open and CLI EditMode tests cannot run, write that exact blockage into `Verification` and `Manual Follow-Ups`.
- If a completed milestone still has deferred manual editor work, do not archive it as fully complete. Mark it blocked or partially verified until that step is resolved.
