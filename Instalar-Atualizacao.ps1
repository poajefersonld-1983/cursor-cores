[CmdletBinding()]
param([string]$PastaDestino = (Join-Path $env:LOCALAPPDATA 'Programs\CursorCores'))
$ErrorActionPreference = 'Stop'
$origem = Join-Path $PSScriptRoot 'Cursor Cores.exe'
if (-not (Test-Path -LiteralPath $origem)) { throw 'Extraia o pacote completo: Cursor Cores.exe deve estar ao lado deste instalador.' }
$sid = [Security.Principal.WindowsIdentity]::GetCurrent().User.Value
# Stop only this application's processes owned by the current Windows account.
foreach ($processo in Get-CimInstance Win32_Process -Filter "Name='Cursor Cores.exe'") {
    $dono = Invoke-CimMethod -InputObject $processo -MethodName GetOwnerSid
    if ($dono.Sid -eq $sid) { Stop-Process -Id $processo.ProcessId -Force -ErrorAction Stop }
}
New-Item -ItemType Directory -Path $PastaDestino -Force | Out-Null
$destino = Join-Path $PastaDestino 'Cursor Cores.exe'
if (([IO.Path]::GetFullPath($origem)) -ne ([IO.Path]::GetFullPath($destino))) {
    if (Test-Path -LiteralPath $destino) { Copy-Item -LiteralPath $destino -Destination ($destino+'.'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'.bak') }
    Copy-Item -LiteralPath $origem -Destination $destino -Force
}
$startup = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey('Software\Microsoft\Windows\CurrentVersion\Run')
$monitorAtivo = $false
try {
    foreach ($nome in $startup.GetValueNames()) {
        $valor = [string]$startup.GetValue($nome)
        if ($nome -eq 'CursorCores' -or $valor -match '(?i)Cursor\s*Cores\.exe"?\s+--guard') {
            $monitorAtivo = $true
            $startup.DeleteValue($nome,$false)
        }
    }
    if ($monitorAtivo) { $startup.SetValue('CursorCores','"'+$destino+'" --guard') }
} finally { $startup.Dispose() }
$shell = New-Object -ComObject WScript.Shell
$atalho = $shell.CreateShortcut((Join-Path ([Environment]::GetFolderPath('Programs')) 'Cursor Cores.lnk'))
$atalho.TargetPath = $destino
$atalho.WorkingDirectory = $PastaDestino
$atalho.IconLocation = $destino
$atalho.Save()
if ($monitorAtivo) { Start-Process -FilePath $destino -ArgumentList '--guard' }
Start-Process -FilePath $destino
Write-Host "Cursor Cores atualizado e aberto: $destino"
Write-Host 'Escolha 32 ou 40 px e clique em Aplicar. Cor e formato anteriores foram preservados.'
