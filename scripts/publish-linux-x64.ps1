# Publish and Zip script for TechnicalSupportService (Linux x64)
$ErrorActionPreference = "Stop"

$basePath      = "C:\dev\TechnicalSupportService"
$runtime       = "linux-x64"
$projectName   = "TechnicalSupportService.SUTP"
$projectPath   = "$basePath\$projectName\$projectName.csproj"
$publishDir    = "$basePath\$projectName\release\$runtime"
$artifactsDir  = "$basePath\artifacts"
$innerZipName  = "$projectName.zip"
$outerZipName  = "SUTP.zip"
$innerZipPath  = "$artifactsDir\$innerZipName"
$outerZipPath  = "$artifactsDir\$outerZipName"
$scriptsDir    = "$basePath\scripts"

Write-Host "--- Building $projectName for $runtime ---" -ForegroundColor Cyan

# 1. Clean old publish output
if (Test-Path $publishDir) {
    Write-Host "Cleaning $publishDir..." -ForegroundColor Yellow
    Remove-Item $publishDir -Recurse -Force
}
dotnet clean $projectPath -c Release -r $runtime --nologo -v q | Out-Null

# 2. Publish
dotnet publish $projectPath -c Release -r $runtime --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o $publishDir

# 3. Prepare artifacts directory
if (-not (Test-Path $artifactsDir)) { New-Item -ItemType Directory -Path $artifactsDir -Force | Out-Null }
if (Test-Path $innerZipPath) { Remove-Item $innerZipPath }
if (Test-Path $outerZipPath) { Remove-Item $outerZipPath }

# 4. Create inner ZIP (project files only)
Write-Host "Archiving project files to $innerZipName..." -ForegroundColor Yellow

Push-Location $publishDir
try {
    $filesToZip = @()
    if (Test-Path $projectName) { $filesToZip += $projectName }
    if (Test-Path "$projectName.exe") { $filesToZip += "$projectName.exe" }
    if (Test-Path "appsettings.json") { $filesToZip += "appsettings.json" }
    if (Test-Path "wwwroot") { $filesToZip += "wwwroot" }

    Add-Type -AssemblyName "System.IO.Compression.FileSystem"
    $zipArchive = [System.IO.Compression.ZipFile]::Open($innerZipPath, "Create")

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

# 5. Create outer SUTP.zip (project archive + deployment scripts)
Write-Host "Creating $outerZipName with deployment scripts..." -ForegroundColor Yellow

Add-Type -AssemblyName "System.IO.Compression.FileSystem"
$outerArchive = [System.IO.Compression.ZipFile]::Open($outerZipPath, "Create")
try {
    # Add inner project zip
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($outerArchive, $innerZipPath, $innerZipName)
    Write-Host "  + $innerZipName" -ForegroundColor Gray

    # Add shell scripts
    foreach ($script in @("install-sutp.sh", "update-sutp.sh", "remove-sutp.sh")) {
        $scriptPath = Join-Path $scriptsDir $script
        if (Test-Path $scriptPath) {
            [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($outerArchive, $scriptPath, $script)
            Write-Host "  + $script" -ForegroundColor Gray
        } else {
            Write-Host "  WARNING: $script not found at $scriptPath" -ForegroundColor Yellow
        }
    }
}
finally {
    $outerArchive.Dispose()
}

Write-Host "" -ForegroundColor White
Write-Host "Done:" -ForegroundColor Green
Write-Host "  Inner ZIP: $innerZipPath" -ForegroundColor Green
Write-Host "  Outer ZIP: $outerZipPath" -ForegroundColor Green
