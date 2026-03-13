$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$dependencyRoot = Join-Path $root 'Assets/Plugins/Editor/ExcelDataReader'
$tempRoot = Join-Path $env:TEMP ('towerbreak-excel-' + [guid]::NewGuid().ToString('N'))

$packages = @(
    @{ Name = 'ExcelDataReader'; Version = '3.8.0'; Dll = 'ExcelDataReader.dll' },
    @{ Name = 'ExcelDataReader.DataSet'; Version = '3.8.0'; Dll = 'ExcelDataReader.DataSet.dll' },
    @{ Name = 'System.Text.Encoding.CodePages'; Version = '9.0.10'; Dll = 'System.Text.Encoding.CodePages.dll' }
)

$preferredFrameworks = @('netstandard2.0', 'net462', 'netstandard2.1', 'net8.0', 'net9.0')

Add-Type -AssemblyName System.IO.Compression.FileSystem

New-Item -ItemType Directory -Path $dependencyRoot -Force | Out-Null
New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

try {
    foreach ($package in $packages) {
        $packageNameLower = $package.Name.ToLowerInvariant()
        $nupkgPath = Join-Path $tempRoot ($package.Name + '.' + $package.Version + '.nupkg')
        $extractPath = Join-Path $tempRoot ($package.Name + '.' + $package.Version)
        $downloadUrl = "https://api.nuget.org/v3-flatcontainer/$packageNameLower/$($package.Version)/$packageNameLower.$($package.Version).nupkg"

        Invoke-WebRequest -Uri $downloadUrl -OutFile $nupkgPath
        $zip = [System.IO.Compression.ZipFile]::OpenRead($nupkgPath)
        try {
            $entry = $null
            foreach ($framework in $preferredFrameworks) {
                $entry = $zip.Entries | Where-Object { $_.FullName -eq ("lib/{0}/{1}" -f $framework, $package.Dll) } | Select-Object -First 1
                if ($entry) {
                    break
                }
            }

            if (-not $entry) {
                throw "Could not locate $($package.Dll) in package $($package.Name) $($package.Version)."
            }

            $destinationPath = Join-Path $dependencyRoot $package.Dll
            [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $destinationPath, $true)
        }
        finally {
            $zip.Dispose()
        }
    }

    Write-Host "Excel DLLs downloaded to $dependencyRoot"
}
finally {
    if (Test-Path $tempRoot) {
        Remove-Item -Path $tempRoot -Recurse -Force
    }
}
