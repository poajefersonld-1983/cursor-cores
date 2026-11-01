$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
Push-Location $raiz
try {
    $tokens = $null; $erros = $null
    [void][Management.Automation.Language.Parser]::ParseFile((Join-Path $raiz 'Atualizar.ps1'),[ref]$tokens,[ref]$erros)
    if ($erros.Count -gt 0) { throw ($erros | Out-String) }
    $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
    if (-not (Test-Path $csc)) { throw 'Compilador .NET Framework 4 não encontrado.' }
    $argumentos = @('/nologo','/target:exe','/main:TestWindowsSizes','/reference:System.Drawing.dll','/reference:System.Windows.Forms.dll','/out:tests\WindowsSizeTests.exe','CursorCores.cs','PointerStyles.cs','CursorPersistence.cs','tests\TestWindowsSizes.cs')
    foreach ($arquivo in Get-ChildItem Base -File) { $argumentos += ('/resource:'+$arquivo.FullName+',Base.'+$arquivo.Name) }
    & $csc $argumentos
    if ($LASTEXITCODE -ne 0) { throw 'Falha na compilação dos testes.' }
    & .\compilar.ps1
    New-Item -ItemType Directory -Force docs\imagens,verificacao | Out-Null
    & .\tests\WindowsSizeTests.exe | Tee-Object verificacao\tamanhos-windows.txt
    if ($LASTEXITCODE -ne 0) { throw 'Falha nos testes Windows.' }
} finally { Pop-Location }
