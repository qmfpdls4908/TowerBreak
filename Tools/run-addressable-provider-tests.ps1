$ErrorActionPreference = 'Stop'

[System.Reflection.Assembly]::LoadFrom('C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.dll') | Out-Null
[System.Reflection.Assembly]::LoadFrom('C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Data\Managed\UnityEngine\UnityEngine.dll') | Out-Null
[System.Reflection.Assembly]::LoadFrom('C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Data\UnityReferenceAssemblies\unity-4.8-api\Facades\netstandard.dll') | Out-Null
[System.Reflection.Assembly]::LoadFrom('C:\Users\admin\Desktop\Fork\TowerBreak\Library\PackageCache\com.unity.ext.nunit@d8c07649098d\net40\unity-custom\nunit.framework.dll') | Out-Null
[System.Reflection.Assembly]::LoadFrom('C:\Users\admin\Desktop\Fork\TowerBreak\Library\ScriptAssemblies\TowerBreak.GameData.dll') | Out-Null

$testsAssembly = [System.Reflection.Assembly]::LoadFrom('C:\Users\admin\Desktop\Fork\TowerBreak\Library\ScriptAssemblies\TowerBreak.GameData.Tests.dll')
$type = $testsAssembly.GetType('TowerBreak.GameData.Tests.Addressables.AddressableAssetProviderTests')
$instance = [Activator]::CreateInstance($type)

$testNames = @(
    'ValidateRequired_WithBlankKey_ThrowsArgumentException',
    'ValidateRequired_WithValue_ReturnsOriginalKey',
    'LoadAssetAsync_WithBlankKey_ThrowsArgumentException',
    'LoadAssetAsync_WhenLoaderReturnsNull_ThrowsInvalidOperationException',
    'LoadAssetAsync_WhenLoaderReturnsAsset_ReturnsLoadedAsset'
)

foreach ($testName in $testNames)
{
    $method = $type.GetMethod($testName)

    try
    {
        $result = $method.Invoke($instance, @())
        if ($result -is [System.Threading.Tasks.Task])
        {
            $result.GetAwaiter().GetResult()
        }

        Write-Output "PASS $testName"
    }
    catch
    {
        $exception = $_.Exception
        if ($exception -is [System.Reflection.TargetInvocationException] -and $exception.InnerException)
        {
            $exception = $exception.InnerException
        }

        Write-Output "FAIL $testName :: $($exception.GetType().FullName) :: $($exception.Message)"
        exit 1
    }
}
