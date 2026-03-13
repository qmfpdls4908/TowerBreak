# Addressables Provider Checklist

## Reference Documents

- `docs/plans/2026-03-13-tower-breaker-vertical-slice/00-overview.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/01-foundation-and-bootstrap.md`
- `docs/plans/2026-03-13-unity-excel-gamedata-converter.md`
- `docs/plans/2026-03-13-implementation-workflow.md`
- `docs/plans/2026-03-13-tower-breaker-vertical-slice/archive/data-schema/README.md`

## Checklist

- Done: create milestone folder and seed milestone docs
- Done: confirm Addressables package is already installed
- Done: research DI and schema dependencies that this milestone will build on
- Done: inspect DI registration code in detail and record the pattern
- Done: define the minimum provider interface and missing-key validation behavior
- Done: write failing tests for provider behavior
- Done: implement provider runtime code
- Done: split scene installer and global installer responsibilities
- Done: add DI global installer runtime path
- Done: register the provider through the global installer path
- Done: identify why Unity targeted batch tests were not producing result XML
- Done: capture decisive target-test evidence for `AddressableAssetProviderTests`
- Done: capture decisive target-test evidence for `DIGlobalInstallerTests` and `AddressablesGlobalInstallerTests`
- Todo: apply the corrected no-`-quit` Unity test invocation to remaining blocked milestones

## Verification

- Passed command: `dotnet build "TowerBreak.GameData.Tests.csproj"`
- Passed command: `dotnet build "TowerBreak.GameData.csproj"`
- Passed command: `dotnet build "TowerBreak.DI.Tests.csproj"`
- Failed RED compile (expected): Unity batch compile before runtime types existed, with missing `TowerBreak.GameData.Addressables` types reported in `Logs\addressables-provider-red.log`
- Observed GREEN compile: after adding runtime types and package references, Unity script compilation completed successfully in `Logs\addressables-provider-tests.log`
- Failed RED compile (expected): Unity batch compile before global installer types and references existed, with missing `DIGlobalInstaller` / `TowerBreak.DI` errors reported in `Logs\global-installer-red.log`
- Observed GREEN compile: after adding `DIGlobalInstaller`, `DIGlobalContext`, asmdef references, and `AddressablesGlobalInstaller`, Unity script compilation completed successfully in `Logs\global-installer-green.log`
- Root cause found: Unity 6 command-line docs state that `-quit` can make the editor quit immediately before `-runTests` completes, which explains the missing result XML behavior
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.DI.Tests.DIGlobalInstallerTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\di-global-tests-noquit.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\di-global-tests-noquit.xml"`
- Evidence: `Logs\di-global-tests-noquit.xml` shows `total="3" passed="3" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.GameData.Tests.Addressables.AddressableAssetProviderTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\addressable-provider-noquit.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\addressable-provider-noquit.xml"`
- Evidence: `Logs\addressable-provider-noquit.xml` shows `total="5" passed="5" failed="0"`
- Passed command: `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\admin\Desktop\Fork\TowerBreak" -runTests -testPlatform EditMode -testFilter "TowerBreak.GameData.Tests.Addressables.AddressablesGlobalInstallerTests" -logFile "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\addressable-global-noquit.log" -testResults "C:\Users\admin\Desktop\Fork\TowerBreak\Logs\addressable-global-noquit.xml"`
- Evidence: `Logs\addressable-global-noquit.xml` shows `total="1" passed="1" failed="0"`

## Manual Follow-Ups

- Later confirm whether initial Addressables groups or sample keys need editor-side bootstrap work
