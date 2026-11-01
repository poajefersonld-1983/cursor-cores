$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
Push-Location $raiz
try {
    $tokens = $null; $erros = $null
    [void][Management.Automation.Language.Parser]::ParseFile((Join-Path $raiz 'Instalar-Atualizacao.ps1'),[ref]$tokens,[ref]$erros)
    if ($erros.Count -gt 0) { throw ($erros | Out-String) }
    [void][Management.Automation.Language.Parser]::ParseFile((Join-Path $raiz 'Atualizar.ps1'),[ref]$tokens,[ref]$erros)
    if ($erros.Count -gt 0) { throw ($erros | Out-String) }
    $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
    if (-not (Test-Path $csc)) { throw 'Compilador .NET Framework 4 não encontrado.' }
    $argumentos = @('/nologo','/target:exe','/main:TestWindowsSizes','/reference:System.Drawing.dll','/reference:System.Windows.Forms.dll','/out:tests\WindowsSizeTests.exe','CursorCores.cs','PointerStyles.cs','CursorPersistence.cs','AssemblyInfo.cs','tests\TestWindowsSizes.cs')
    foreach ($arquivo in Get-ChildItem Base -File) { $argumentos += ('/resource:'+$arquivo.FullName+',Base.'+$arquivo.Name) }
    & $csc $argumentos
    if ($LASTEXITCODE -ne 0) { throw 'Falha na compilação dos testes.' }
    & .\compilar.ps1
    New-Item -ItemType Directory -Force docs\imagens,verificacao | Out-Null
    & .\tests\WindowsSizeTests.exe | Tee-Object verificacao\tamanhos-windows.txt
    if ($LASTEXITCODE -ne 0) { throw 'Falha nos testes Windows.' }
    $isolado = Join-Path $env:TEMP ('CursorCores-portatil-'+[Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory $isolado | Out-Null
    try {
        Copy-Item 'Cursor Cores.exe' $isolado
        $exeIsolado = Join-Path $isolado 'Cursor Cores.exe'
        foreach ($opcao in @('--verify','--test-persistence')) {
            $p = Start-Process -FilePath $exeIsolado -ArgumentList $opcao -Wait -PassThru
            if ($p.ExitCode -ne 0) { throw "O executável de produção falhou em $opcao." }
        }
        Get-Content (Join-Path $isolado 'verificacao.txt'),(Join-Path $isolado 'verificacao-persistencia.txt') -Encoding UTF8 | Set-Content verificacao\auto-verificacao-exe.txt -Encoding UTF8
    } finally { Remove-Item -LiteralPath $isolado -Recurse -Force }

} finally { Pop-Location }
