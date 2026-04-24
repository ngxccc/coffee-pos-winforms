<#
.SYNOPSIS
    Script quản lý build và chạy dự án CoffeePOS.
#>

# 1. TỰ ĐỘNG KIỂM TRA VÀ NÂNG QUYỀN (Nếu cần)
# Nếu script đang chạy mà bị chặn bởi Policy, đoạn này sẽ khởi động lại chính nó với quyền Bypass
if ((Get-ExecutionPolicy) -gt 'RemoteSigned' -and $PSId -ne 'Bypass') {
    Start-Process powershell -ArgumentList "-ExecutionPolicy Bypass -File `"$PSCommandPath`" $($args -join ' ')" -Wait
    exit
}

# 2. XÁC ĐỊNH ĐƯỜNG DẪN GỐC (ROOT)
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$RootPath = Split-Path -Parent $ScriptPath
Set-Location $RootPath

# Kiểm tra xem có đang ở đúng root không (check file .slnx hoặc src)
if (-not (Test-Path "src/CoffeePOS")) {
    Write-Host "[ERROR] Khong tim thay thu muc src. Hay chac chan ban dang chay script tu thu muc goc cua CoffeePOS." -ForegroundColor Red
    exit
}

Write-Host "--- CoffeePOS CLI: $pwd ---" -ForegroundColor Cyan

# 3. ĐỊNH NGHĨA CÁC LỆNH (Tương tự npm scripts)
function Build-Release {
    Write-Host ">>> Building Release (ReadyToRun)..." -ForegroundColor Green
    dotnet publish "src/CoffeePOS/CoffeePOS.csproj" `
        -c Release `
        -r win-x64 `
        --self-contained `
        /p:PublishReadyToRun=true `
        /p:PublishSingleFile=true
}

function Run-Migrator {
    Write-Host ">>> Running Database Migrator..." -ForegroundColor Yellow
    dotnet run --project "src/Migrator/Migrator.csproj"
}

function Run-Dev {
    Write-Host ">>> Starting App in Development mode..." -ForegroundColor Magenta
    dotnet run --project "src/CoffeePOS/CoffeePOS.csproj"
}

# 4. XỬ LÝ THAM SỐ TRUYỀN VÀO
$command = $args[0]

switch ($command) {
    "build"   { Build-Release }
    "migrate" { Run-Migrator }
    "dev"     { Run-Dev }
    "help"    {
        Write-Host "Cac lenh co san:"
        Write-Host "  ./run.ps1 dev     : Chay app mode debug"
        Write-Host "  ./run.ps1 build   : Build file exe duy nhat (R2R)"
        Write-Host "  ./run.ps1 migrate : Chay khoi tao database"
    }
    Default {
        if (-not $command) { Write-Host "Hay nhap lenh (Vi du: ./run.ps1 dev). Go 'help' de xem chi tiet." -ForegroundColor Cyan }
        else { Write-Host "Lenh '$command' khong hop le." -ForegroundColor Red }
    }
}
