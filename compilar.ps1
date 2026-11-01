$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
    if (-not (Test-Path $csc)) { $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
    if (-not (Test-Path $csc)) { throw 'Instale o .NET Framework 4.8 para compilar.' }
    $argumentos = @('/nologo','/target:winexe','/optimize+','/out:Cursor Cores.exe','/reference:System.Drawing.dll','/reference:System.Windows.Forms.dll','CursorCores.cs','PointerStyles.cs','CursorPersistence.cs','AssemblyInfo.cs')
    foreach ($arquivo in Get-ChildItem Base -File) { $argumentos += ('/resource:'+$arquivo.FullName+',Base.'+$arquivo.Name) }
    & $csc $argumentos
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao compilar o executável Windows.' }
    Write-Host 'Cursor Cores.exe compilado com os recursos e o monitor incorporados.'
} finally { Pop-Location }
