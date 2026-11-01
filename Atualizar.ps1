[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$PastaProjeto)
$ErrorActionPreference = 'Stop'
$destino = (Resolve-Path -LiteralPath $PastaProjeto).Path
$nomes = @('CursorCores.cs','PointerStyles.cs','CursorPersistence.cs','compilar.ps1')
foreach ($nome in $nomes) {
    if (-not (Test-Path -LiteralPath (Join-Path $destino $nome))) { throw "Falta $nome na pasta do projeto original: $destino" }
}
$exe = Join-Path $destino 'Cursor Cores.exe'
if (Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exe }) {
    throw 'Feche o Cursor Cores e encerre seu monitor na bandeja antes de atualizar.'
}
$fontePath = Join-Path $destino 'CursorCores.cs'
$fonte = [IO.File]::ReadAllText($fontePath)
$lista = 'new\s+int\s*\[\s*\]\s*\{\s*24\s*,\s*48\s*,\s*64\s*\}'
if ($fonte -notmatch 'static readonly int\[\] SupportedSizes') {
    if ($fonte -notmatch 'partial class CursorCores\s*:\s*Form\s*\{') { throw 'Estrutura do fonte não reconhecida. Nenhum arquivo foi alterado.' }
    $rotulos = 'sizes\.Items\.AddRange\(new object\[\]\{[^}]*24 px[^}]*48 px[^}]*64 px[^}]*\}\);sizes\.SelectedIndex=0;'
    if ($fonte -notmatch $rotulos) { throw 'Seletor de tamanhos não reconhecido. Nenhum arquivo foi alterado.' }
    $fonte = [regex]::Replace($fonte, '(partial class CursorCores\s*:\s*Form\s*\{)', ('$1' + "`r`n static readonly int[] SupportedSizes={24,32,40,48,56,64};"), 1)
    $fonte = [regex]::Replace($fonte,$rotulos,'foreach(int size in SupportedSizes)sizes.Items.Add(size+" px");sizes.SelectedIndex=Array.IndexOf(SupportedSizes,32);')
    $fonte = [regex]::Replace($fonte,$lista,'SupportedSizes')
} elseif ($fonte -notmatch 'SupportedSizes\s*=\s*\{24,32,40,48,56,64\}') {
    throw 'Já existe uma lista personalizada de tamanhos. Confira o fonte antes de substituir.'
}
$monitorPath = Join-Path $destino 'CursorPersistence.cs'
$monitor = [regex]::Replace([IO.File]::ReadAllText($monitorPath),$lista,'SupportedSizes')
$backup = Join-Path $destino ('Backup-tamanhos-' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Path $backup | Out-Null
foreach ($nome in @('CursorCores.cs','CursorPersistence.cs','Cursor Cores.exe')) {
    $arquivo = Join-Path $destino $nome
    if (Test-Path -LiteralPath $arquivo) { Copy-Item -LiteralPath $arquivo -Destination $backup }
}
try {
    $utf8 = New-Object Text.UTF8Encoding($true)
    [IO.File]::WriteAllText($fontePath,$fonte,$utf8)
    [IO.File]::WriteAllText($monitorPath,$monitor,$utf8)
    Push-Location $destino
    try {
        $global:LASTEXITCODE = 0
        & (Join-Path $destino 'compilar.ps1')
        if ($LASTEXITCODE -ne 0) { throw "O compilador retornou $LASTEXITCODE." }
        if (-not (Test-Path -LiteralPath $exe)) { throw 'O executável não foi criado pelo compilador original.' }
    } finally { Pop-Location }
    Write-Host 'Atualização concluída: 24, 32, 40, 48, 56 e 64 px.'
    Write-Host "Backup da versão anterior: $backup"
} catch {
    foreach ($nome in @('CursorCores.cs','CursorPersistence.cs','Cursor Cores.exe')) {
        $anterior = Join-Path $backup $nome
        if (Test-Path -LiteralPath $anterior) { Copy-Item -LiteralPath $anterior -Destination (Join-Path $destino $nome) -Force }
    }
    throw
}
