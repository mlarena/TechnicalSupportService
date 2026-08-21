# Publish and Zip script for Linux x64
$ErrorActionPreference = "Stop"

$basePath = "C:\git\ComplexesMonitoring"
$runtime = "linux-x64"
$publishDirName = "release\$runtime"

$projects = @(
    @{ Name = "ComplexesMonitoring.TCM"; Path = "$basePath\ComplexesMonitoring.TCM\ComplexesMonitoring.TCM.csproj"; Out = "$basePath\ComplexesMonitoring.TCM\$publishDirName" },
    @{ Name = "ComplexesMonitoring.Worker"; Path = "$basePath\ComplexesMonitoring.Worker\ComplexesMonitoring.Worker.csproj"; Out = "$basePath\ComplexesMonitoring.Worker\$publishDirName" },
    @{ Name = "ComplexesMonitoring.VideoMonitoring"; Path = "$basePath\ComplexesMonitoring.VideoMonitoring\ComplexesMonitoring.VideoMonitoring.csproj"; Out = "$basePath\ComplexesMonitoring.VideoMonitoring\$publishDirName" },
    @{ Name = "ComplexesMonitoring.Api"; Path = "$basePath\ComplexesMonitoring.Api\ComplexesMonitoring.Api.csproj"; Out = "$basePath\ComplexesMonitoring.Api\$publishDirName" }
)
foreach ($project in $projects) {
    Write-Host "--- Building $($project.Name) for $runtime ---" -ForegroundColor Cyan
    
    # 1. Publish
    dotnet publish $project.Path -c Release -r $runtime --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o $project.Out
    
    # 2. Create ZIP
    $zipPath = Join-Path $project.Out "$($project.Name).zip"
    if (Test-Path $zipPath) { Remove-Item $zipPath }
    
    Write-Host "Archiving to $($project.Name).zip..." -ForegroundColor Yellow
    
    Push-Location $project.Out
    try {
        $filesToZip = @()
        if (Test-Path $project.Name) { $filesToZip += $project.Name }
        if (Test-Path "appsettings.json") { $filesToZip += "appsettings.json" }
        if (Test-Path "wwwroot") { $filesToZip += "wwwroot" }

        Add-Type -AssemblyName "System.IO.Compression.FileSystem"
        $zipArchive = [System.IO.Compression.ZipFile]::Open($zipPath, "Create")
        
        foreach ($item in $filesToZip) {
            if (Test-Path $item -PathType Container) {
                $files = Get-ChildItem $item -Recurse
                foreach ($file in $files) {
                    if (-not $file.PSIsContainer) {
                        $relativeName = $file.FullName.Substring($project.Out.Length + 1).Replace('\', '/')
                        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zipArchive, $file.FullName, $relativeName)
                    }
                }
            } else {
                $relativeName = $item.Replace('\', '/')
                [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zipArchive, (Join-Path $project.Out $item), $relativeName)
            }
        }
        $zipArchive.Dispose()
    }
    finally {
        Pop-Location
    }
    Write-Host "Done: $zipPath" -ForegroundColor Green
}

Write-Host "--- Creating ptz.zip ---" -ForegroundColor Cyan
$ptzSourcePath = "$basePath\ptz"
$ptzZipPath = "$ptzSourcePath\ptz.zip"
if (Test-Path $ptzZipPath) { Remove-Item $ptzZipPath }

$ptzFiles = @("ptz_onvif.py", "ptz_srv.py", "requirements.txt")
Add-Type -AssemblyName "System.IO.Compression.FileSystem"
$zipArchive = [System.IO.Compression.ZipFile]::Open($ptzZipPath, "Create")
try {
    foreach ($fileName in $ptzFiles) {
        $filePath = Join-Path $ptzSourcePath $fileName
        if (Test-Path $filePath) {
            [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zipArchive, $filePath, $fileName)
        }
    }
}
finally {
    $zipArchive.Dispose()
}
Write-Host "Done: $ptzZipPath" -ForegroundColor Green

Write-Host "--- Creating Deploy Bundle ---" -ForegroundColor Cyan
& "$PSScriptRoot\create_deploy_bundle_x64.ps1"
