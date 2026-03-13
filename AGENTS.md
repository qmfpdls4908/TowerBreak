# AGENTS.md

## Purpose
- This repository is a Unity 6 project with small C# modules under `Assets/Scripts` and EditMode tests under `Assets/Tests/EditMode`.
- Use this file as the operating guide for agentic coding assistants working in this repo.
- Prefer repo-specific conventions over generic Unity or C# advice.

## Rule Sources
- Existing `AGENTS.md`: present in the repo root and should be kept current.
- Cursor rules: no `.cursorrules` file and no files under `.cursor/rules/` were found.
- Copilot rules: no `.github/copilot-instructions.md` file was found.
- Generated project files such as `*.csproj` are Unity-generated; do not edit them manually.

## Project Snapshot
- Unity version: `6000.3.11f1` from `ProjectSettings/ProjectVersion.txt`.
- Active solution: `TowerBreak.sln`.
- Stale/empty solution: `TowerBreaker.sln` exists but has no projects; ignore it unless the user asks about it.
- Runtime assemblies:
  - `Assets/Scripts/DI/`
  - `Assets/Scripts/GameData/`
  - `Assets/Scripts/EventBus/`
- Editor-only code:
  - `Assets/Scripts/GameData/Editor/`
- EditMode tests:
  - `Assets/Tests/EditMode/DI/`
  - `Assets/Tests/EditMode/GameData/`
  - `Assets/Tests/EditMode/EventBus/`
- Unity Test Framework is installed through `Packages/manifest.json`.

## High-Value Commands
- Fast compile/analyzer pass for the generated solution:
  - `dotnet build "TowerBreak.sln"`
- Build a specific generated project quickly:
  - `dotnet build "TowerBreak.DI.csproj"`
  - `dotnet build "TowerBreak.GameData.csproj"`
  - `dotnet build "TowerBreak.GameData.Editor.csproj"`
  - `dotnet build "TowerBreak.DI.Tests.csproj"`
  - `dotnet build "TowerBreak.GameData.Tests.csproj"`
- Run all Unity EditMode tests from the CLI:
  - `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -logFile "Logs\editmode-tests.log" -testResults "Logs\editmode-tests.xml"`
- Run one Unity EditMode fixture/class:
  - use the same Unity command with `-testFilter "TowerBreak.DI.Tests.DIContainerTests"`
- Run one Unity EditMode test:
  - `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.DI.Tests.DIContainerTests.ResolveFromRegistered_ReturnsNewestRegisteredContainerValue" -logFile "Logs\single-test.log" -testResults "Logs\single-test.xml"`
- EventBus tests are also runnable through the same Unity command, for example:
  - `-testFilter "TowerBreak.EventBus.Tests.EventBusTests.Publish_InvokesSubscribedHandlerWithPayload"`

## Command Notes
- `dotnet build` works in this repo and is the fastest non-Unity verification step.
- The generated C# projects include Unity analyzers, so `dotnet build` is the closest thing to linting here.
- There is no `.editorconfig`, no dedicated formatter config, and no standalone lint script checked in.
- Prefer Unity batchmode for actual test execution; `dotnet test` is not the primary workflow for these Unity EditMode tests.
- The current `TowerBreak.sln` only includes DI and GameData generated projects; EventBus code exists in asmdefs but does not currently have a generated `TowerBreak.EventBus*.csproj` in the repo root.
- If you touch EventBus code, rely on Unity compilation and Unity EditMode tests rather than expecting `dotnet build "TowerBreak.sln"` to cover it.
- Unity batchmode test runs fail if the same project is already open in another Unity instance.
- At the time of analysis, Unity was already running for this project, so CLI EditMode execution was blocked until that instance closes.

## Files And Boundaries
- Keep runtime-safe code in runtime assemblies such as `Assets/Scripts/DI/`, `Assets/Scripts/GameData/`, and `Assets/Scripts/EventBus/`.
- Keep Unity Editor dependencies in editor-only assemblies such as `Assets/Scripts/GameData/Editor/`.
- Keep tests under `Assets/Tests/EditMode/...` and wire them through asmdefs.
- Respect asmdef boundaries when adding references; runtime code must not depend on editor assemblies.
- `TowerBreak.EventBus.asmdef` sets `noEngineReferences` to `true`; keep EventBus as plain C# with no UnityEngine dependency.
- Do not move Excel reader or editor menu code into runtime assemblies.

## Generated Files
- Do not manually edit `TowerBreak.DI.csproj`, `TowerBreak.GameData.csproj`, `TowerBreak.GameData.Editor.csproj`, `TowerBreak.*.Tests.csproj`, or similar generated solution artifacts.
- If project structure changes are needed, prefer editing asmdefs, package manifests, or Unity-side assets that regenerate those files.
- Treat `Temp/`, `obj/`, `Library/`, and generated logs as derived outputs, not hand-edited sources.

## Code Style
- Use C# with Allman braces.
- Use block-scoped namespaces, not file-scoped namespaces.
- Keep one production type per file.
- Prefer `public sealed class` for leaf types unless inheritance is required.
- Use `readonly struct` when value semantics and immutability are intentional, as in `CellValue` and `ExcelParseContext`.
- Use expression-bodied members sparingly; the current codebase mostly uses full method and property bodies.
- Prefer explicit types when they improve readability.
- Target-typed `new()` is already used and acceptable when the left-hand side makes the type obvious.
- Keep members ordered simply: constructors, public API, non-public helpers, nested helper types.
- Add blank lines between logical blocks, but keep files compact.

## Implementation Principles
- Implement with SOLID principles in mind; keep responsibilities narrow, depend on abstractions where it helps, and avoid forcing callers to depend on APIs they do not use.
- Follow YAGNI closely; do not add extension points, configuration layers, wrappers, or generalized systems before the repo actually needs them.
- Follow KISS; prefer the smallest clear implementation that matches existing patterns over clever abstractions or framework-like indirection.
- Reuse the project's existing `DI` module for object wiring and lifecycle-managed dependency registration instead of introducing a second dependency management pattern.
- Reuse the project's existing `EventBus` module for publish/subscribe communication instead of adding ad-hoc static globals, custom signal hubs, or tightly coupled cross-module callbacks.
- When a feature needs cross-object collaboration, prefer constructor injection or `DIContainer` registration/resolution for dependencies and use `EventBus<T>` only for event-style communication, not as a service locator.
- Keep dependency flow explicit: runtime services should be registered through the DI module, event notifications should flow through EventBus, and direct concrete-type coupling between unrelated systems should be minimized.

## Imports And Formatting
- Put `System` usings first.
- Then place framework/library usings such as `NUnit.Framework`, `ExcelDataReader`, `UnityEngine`, or `UnityEditor`.
- Keep one `using` per line.
- Leave a blank line before the namespace declaration.
- Avoid unnecessary usings and remove them when you touch a file.
- Preserve the local file's grouping style if a file already has a stable pattern.

## Naming
- Use PascalCase for types, methods, properties, public fields, and attributes.
- Use camelCase for private instance fields, parameters, and local variables.
- Private static readonly fields may be PascalCase in this repo when treated as shared named state; match nearby code.
- Test methods use `MethodOrScenario_Condition_ExpectedOutcome` style names, for example `ResolveFromRegistered_ReturnsNewestRegisteredContainerValue`.
- Interface names use the standard `I` prefix, for example `IWorkbookReader` and `IExcelValueParser`.
- Attribute types end with `Attribute` even when usage sites omit the suffix.

## Types And API Design
- Prefer small, focused APIs with narrow responsibilities.
- Prefer immutable get-only properties for data models such as `WorkbookData`, `SheetData`, and `CellValue`.
- Use constructor injection for plain model state when possible.
- In Unity-serialized data shapes, fields are acceptable and often preferred over properties.
- For runtime registries and collections, expose `IReadOnlyList<T>` instead of mutable lists when callers should not mutate state.
- Keep generic APIs simple and type-safe; the DI container uses both generic and `Type`-based overloads.
- Prefer plain C# classes unless Unity lifecycle participation is required.

## Nullability, Validation, And Errors
- The codebase currently uses runtime guards rather than nullable reference type annotations.
- Validate public method arguments aggressively.
- Throw `ArgumentNullException` for null references.
- Throw `ArgumentException` for invalid values such as empty paths or incompatible instances.
- Throw `FileNotFoundException` when a required file path is missing.
- Use `InvalidOperationException` when lookup or state assumptions fail.
- Include concrete context in exception messages such as type names, keys, or file paths.
- Fail fast; do not silently swallow registration, parsing, or file-system errors.
- When a `Try*` method exists, return `false` with a default/null output instead of throwing for expected misses.
- Keep side effects after validation, not before it.

## Unity-Specific Guidance
- Use `MonoBehaviour` only where Unity lifecycle participation is required, like `DIInstaller`.
- Keep editor-only dependencies out of runtime asmdefs.
- Use fields, not properties, for Unity-serialized data and test fixture DTOs when serialization matters.
- Prefer EditMode tests unless the behavior truly needs PlayMode.
- For editor/importer features, keep Excel/NuGet dependency handling in editor-only code and scripts such as `Tools/setup-excel-reader.ps1`.

## Testing Guidance
- Add tests next to the relevant feature area under `Assets/Tests/EditMode/`.
- Use NUnit attributes like `[Test]`, `[SetUp]`, and `[TearDown]`.
- Keep tests deterministic and isolated.
- Reset static or global state in setup or teardown when needed; `Assets/Tests/EditMode/DI/DIContainerTests.cs` is the model to follow.
- Assert behavior rather than implementation details unless reflection is necessary for test isolation.
- When you cannot run Unity tests because the editor holds the project lock, say so explicitly and still provide the exact batchmode command to run later.

## What To Mirror From Existing Code
- Small classes with narrow responsibilities.
- Straight-line control flow with early argument validation.
- Minimal comments; the current code mostly relies on clear naming instead.
- Explicit exception types and actionable error messages.
- No unnecessary abstraction layers.

## What To Avoid
- Do not introduce file-scoped namespaces into otherwise block-scoped areas.
- Do not add heavy service-locator patterns outside the existing DI module.
- Do not edit generated Unity project files.
- Do not mix runtime and editor code in the same assembly.
- Do not add silent fallbacks that hide bad data.
- Do not add formatting-only churn unless the user asks for cleanup.

## Suggested Agent Workflow
- Read the relevant asmdef and nearby files before changing assembly boundaries.
- Make the smallest coherent change.
- Run `dotnet build "TowerBreak.sln"` after DI/GameData code changes whenever possible.
- If you touch EventBus, verify in Unity because the current generated solution does not cover it.
- Run Unity batchmode EditMode tests for the affected area when Unity is not already holding the project lock.
- If CLI tests are blocked, say that clearly and include the exact deferred test command.

## Milestone And Documentation Workflow
- Organize substantial implementation work by milestone, not by ad-hoc file edits.
- Each milestone should get its own folder under `docs/plans/<date>-<feature>/milestones/<milestone-slug>/`.
- Each milestone folder should usually contain:
  - `README.md` for milestone overview and status
  - `references.md` for source documents and code references
  - `research.md` for findings from repo/doc/code investigation before implementation
  - `checklist.md` for implementation and verification progress
- If one file is enough for a very small milestone, keep it compact, but still preserve the same sections inside that file.
- Before implementing a milestone, always research first:
  - relevant plan documents
  - relevant direction/spec docs
  - nearby production code
  - nearby tests
  - asmdef and dependency boundaries
  - previously completed milestone outputs
- Record that research in the milestone docs before or alongside implementation. Do not rely on memory-only context for follow-up sessions.
- Every milestone document must include a `Reference Documents` section with clickable repo-relative paths.
- Every milestone document must include a `Checklist` section with explicit status markers such as `Todo`, `Doing`, `Blocked`, and `Done`.
- Update milestone docs as work progresses, not only at the end.
- When a milestone is completed and verified, move its folder to an archive location such as `docs/plans/<date>-<feature>/archive/<milestone-slug>/` and update the parent overview/status document to point at the archived location.
- When archiving, preserve the full research trail, decisions, verification notes, and any follow-up items so the next session can resume from the archive without re-discovery.
- The parent feature overview must track:
  - active milestones
  - archived milestones
  - current in-progress item
  - blocked items
  - next recommended action
- If implementation is blocked by Unity editor lock, missing assets, or manual inspector work, record that in the milestone checklist and parent overview immediately.
- Use milestone docs as the source of truth for session handoff. Final chat summaries should match the milestone docs rather than replace them.
