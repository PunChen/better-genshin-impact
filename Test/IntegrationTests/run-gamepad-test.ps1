param(
    [string]$Case = (Join-Path $PSScriptRoot "Cases\teleport_mondstadt.json"),
    [string]$Exe = (Join-Path $PSScriptRoot "..\..\BetterGenshinImpact\bin\x64\Debug\net8.0-windows10.0.22621.0\BetterGI.exe")
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $Exe)) {
    throw "BetterGI.exe 不存在：$Exe，请先构建 x64 版本"
}

if (-not (Test-Path $Case)) {
    throw "测试步骤文件不存在：$Case"
}

$Exe = (Resolve-Path $Exe).Path
$Case = (Resolve-Path $Case).Path

Write-Host "启动 BGI 集成测试："
Write-Host "  EXE : $Exe"
Write-Host "  CASE: $Case"

$process = Start-Process -FilePath $Exe -ArgumentList @("--gamepad-test", $Case) -PassThru -Wait
exit $process.ExitCode
