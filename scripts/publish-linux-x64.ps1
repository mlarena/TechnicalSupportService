# ============================================================
# Publish and Zip script for TechnicalSupportService (Linux x64)
# ============================================================
# Сборка ASP.NET Core MVC приложения для развёртывания на Linux.
# Результат: ZIP-архив с forward-slash путями, готовый к деплою.
# ============================================================

$ErrorActionPreference = "Stop"

# --- Настройки -----------------------------------------------------------
$basePath       = "C:\dev\TechnicalSupportService"
$runtime        = "linux-x64"
$configuration  = "Release"
$projectName    = "TechnicalSupportService.SUTP"
$projectPath    = "$basePath\$projectName\$projectName.csproj"
$publishDir     = "$basePath\$projectName\publish\$runtime"
$outputZipDir   = "$basePath\artifacts"
$zipFileName    = "$projectName-$runtime.zip"
$zipPath        = "$outputZipDir\$zipFileName"

# --- 1. Очистка предыдущей сборки ---------------------------------------
Write-Host "=== Очистка предыдущей сборки ===" -ForegroundColor DarkGray

if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
    Write-Host "  Удалена папка: $publishDir" -ForegroundColor DarkGray
}

if (-not (Test-Path $outputZipDir)) {
    New-Item -ItemType Directory -Path $outputZipDir -Force | Out-Null
}

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
    Write-Host "  Удалён старый архив: $zipPath" -ForegroundColor DarkGray
}

# --- 2. Публикация -------------------------------------------------------
Write-Host ""
Write-Host "=== Публикация $projectName для $runtime ===" -ForegroundColor Cyan

dotnet publish $projectPath `
    -c $configuration `
    -r $runtime `
    --self-contained true `
    -o $publishDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "ОШИБКА: Публикация завершилась с кодом $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "  Публикация завершена: $publishDir" -ForegroundColor Green

# --- 3. Создание ZIP-архива с forward-slash путями -----------------------
Write-Host ""
Write-Host "=== Создание архива $zipFileName ===" -ForegroundColor Yellow

Add-Type -AssemblyName "System.IO.Compression.FileSystem"
Add-Type -AssemblyName "System.IO.Compression"

$zipArchive = [System.IO.Compression.ZipFile]::Open($zipPath, "Create")

try {
    # Собираем все файлы из папки публикации
    $allFiles = Get-ChildItem $publishDir -Recurse -File

    $fileCount = 0
    foreach ($file in $allFiles) {
        # Вычисляем относительный путь
        $relativePath = $file.FullName.Substring($publishDir.Length + 1)

        # Конвертируем обратные слеши в прямые для Linux
        $relativePath = $relativePath.Replace('\', '/')

        # Добавляем файл в архив с Linux-путём
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
            $zipArchive,
            $file.FullName,
            $relativePath,
            [System.IO.Compression.CompressionLevel]::Optimal
        ) | Out-Null

        $fileCount++
    }

    Write-Host "  Добавлено файлов: $fileCount" -ForegroundColor Green
}
finally {
    $zipArchive.Dispose()
}

# --- 4. Информация о результате ------------------------------------------
$zipSize = (Get-Item $zipPath).Length
$zipSizeMB = [math]::Round($zipSize / 1MB, 2)

Write-Host ""
Write-Host "=== Готово! ===" -ForegroundColor Green
Write-Host "  Архив: $zipPath" -ForegroundColor White
Write-Host "  Размер: $zipSizeMB MB" -ForegroundColor White
Write-Host "  Файлов: $fileCount" -ForegroundColor White
Write-Host ""

# --- 5. Проверка содержимого архива (первые 30 записей) ------------------
Write-Host "=== Содержимое архива (первые 30 записей) ===" -ForegroundColor DarkCyan

$readArchive = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $entries = $readArchive.Entries | Select-Object -First 30
    foreach ($entry in $entries) {
        $size = if ($entry.Length -gt 0) { "$($entry.Length) bytes" } else { "[dir]" }
        Write-Host "  $($entry.FullName)  ($size)" -ForegroundColor DarkGray
    }

    if ($readArchive.Entries.Count -gt 30) {
        Write-Host "  ... и ещё $($readArchive.Entries.Count - 30) записей" -ForegroundColor DarkGray
    }
}
finally {
    $readArchive.Dispose()
}

Write-Host ""
Write-Host "Для развёртывания на Linux:" -ForegroundColor Cyan
Write-Host "  1. Скопируйте архив на сервер" -ForegroundColor White
Write-Host "  2. Распакуйте: unzip $zipFileName -d /opt/sutp" -ForegroundColor White
Write-Host "  3. Отредактируйте appsettings.json (connection string, FileStorage:LocalPath)" -ForegroundColor White
Write-Host "  4. Запустите: dotnet TechnicalSupportService.SUTP.dll" -ForegroundColor White
Write-Host ""
