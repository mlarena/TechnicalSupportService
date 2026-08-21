# Publish and Zip script for TechnicalSupportService (Linux x64)
$ErrorActionPreference = "Stop"

$basePath      = "C:\dev\TechnicalSupportService"
$runtime       = "linux-x64"
$projectName   = "TechnicalSupportService.SUTP"
$projectPath   = "$basePath\$projectName\$projectName.csproj"
$publishDir    = "$basePath\$projectName\release\$runtime"
$zipPath       = "$basePath\artifacts\$projectName.zip"

Write-Host "--- Building $projectName for $runtime ---" -ForegroundColor Cyan

# 1. Clean old publish output
if (Test-Path $publishDir) {
    Write-Host "Cleaning $publishDir..." -ForegroundColor Yellow
    Remove-Item $publishDir -Recurse -Force
}
dotnet clean $projectPath -c Release -r $runtime --nologo -v q | Out-Null

# 2. Publish
dotnet publish $projectPath -c Release -r $runtime --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o $publishDir

# 3. Create ZIP
if (-not (Test-Path (Split-Path $zipPath))) { New-Item -ItemType Directory -Path (Split-Path $zipPath) -Force | Out-Null }
if (Test-Path $zipPath) { Remove-Item $zipPath }

Write-Host "Archiving to $projectName.zip..." -ForegroundColor Yellow

Push-Location $publishDir
try {
    $filesToZip = @()
    if (Test-Path $projectName) { $filesToZip += $projectName }
    if (Test-Path "$projectName.exe") { $filesToZip += "$projectName.exe" }
    if (Test-Path "appsettings.json") { $filesToZip += "appsettings.json" }
    if (Test-Path "wwwroot") { $filesToZip += "wwwroot" }

    Add-Type -AssemblyName "System.IO.Compression.FileSystem"
    $zipArchive = [System.IO.Compression.ZipFile]::Open($zipPath, "Create")

    foreach ($item in $filesToZip) {
        if (Test-Path $item -PathType Container) {
            $files = Get-ChildItem $item -Recurse
            foreach ($file in $files) {
                if (-not $file.PSIsContainer) {
                    $relativeName = $file.FullName.Substring($publishDir.Length + 1).Replace('\', '/')
                    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zipArchive, $file.FullName, $relativeName)
                }
            }
        } else {
            $relativeName = $item.Replace('\', '/')
            [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zipArchive, (Join-Path $publishDir $item), $relativeName)
        }
    }
    $zipArchive.Dispose()
}
finally {
    Pop-Location
}
Write-Host "Done: $zipPath" -ForegroundColor Green
