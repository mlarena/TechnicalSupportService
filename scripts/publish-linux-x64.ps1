# Publish and Zip script for TechnicalSupportService (Linux x64)
$ErrorActionPreference = "Stop"

$basePath      = "C:\dev\TechnicalSupportService"
$runtime       = "linux-x64"
$projectName   = "TechnicalSupportService.SUTP"
$projectPath   = "$basePath\$projectName\$projectName.csproj"
$publishDir    = "$basePath\$projectName\release\$runtime"
$zipPath       = "$basePath\artifacts\$projectName.zip"


Write-Host "--- Building $projectName for $runtime ---" -ForegroundColor Cyan

# 1. Publish
dotnet publish $projectPath -c Release -r $runtime --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o $publishDir

# 2. Create ZIP
if (-not (Test-Path (Split-Path $zipPath))) { New-Item -ItemType Directory -Path (Split-Path $zipPath) -Force | Out-Null }
if (Test-Path $zipPath) { Remove-Item $zipPath }

Write-Host "Archiving to $projectName.zip..." -ForegroundColor Yellow

Push-Location $publishDir
try {
    Add-Type -AssemblyName "System.IO.Compression.FileSystem"
    $zipArchive = [System.IO.Compression.ZipFile]::Open($zipPath, "Create")

    $allFiles = Get-ChildItem $publishDir -Recurse -File
    foreach ($file in $allFiles) {
        $relativeName = $file.FullName.Substring($publishDir.Length + 1).Replace('\', '/')
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zipArchive, $file.FullName, $relativeName)
    }

    $zipArchive.Dispose()
}
finally {
    Pop-Location
}
Write-Host "Done: $zipPath" -ForegroundColor Green
