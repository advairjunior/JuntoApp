$ErrorActionPreference = 'Stop'

$raizDoProjeto = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$inicializador = Join-Path $raizDoProjeto 'rodar.ps1'

if (-not (Test-Path -LiteralPath $inicializador -PathType Leaf))
{
    throw 'O inicializador rodar.ps1 deve existir na raiz do projeto.'
}

$pastaDeDadosLocais = [Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)
$pastaTemporaria = Join-Path $pastaDeDadosLocais ("Temp\entre-nos-inicializador-{0}" -f [Guid]::NewGuid())
$caminhoAnterior = $env:PATH

try
{
    $pastaDosComandos = Join-Path $pastaTemporaria 'comandos'
    $pastaDaApi = Join-Path $pastaTemporaria 'src\ProjetoEncontros.Api'
    $pastaDoAplicativo = Join-Path $pastaTemporaria 'src\ProjetoEncontros.AplicativoWeb'

    New-Item -ItemType Directory -Path $pastaDosComandos -Force | Out-Null
    New-Item -ItemType Directory -Path $pastaDaApi -Force | Out-Null
    New-Item -ItemType Directory -Path $pastaDoAplicativo -Force | Out-Null

    Copy-Item -LiteralPath $inicializador -Destination (Join-Path $pastaTemporaria 'rodar.ps1')
    New-Item -ItemType File -Path (Join-Path $pastaTemporaria 'docker-compose.yml') | Out-Null
    New-Item -ItemType File -Path (Join-Path $pastaDaApi 'ProjetoEncontros.Api.csproj') | Out-Null
    Set-Content -LiteralPath (Join-Path $pastaTemporaria '.env') -Value @(
        'POSTGRES_DB=projeto_encontros'
        'POSTGRES_USER=projeto_encontros'
        'POSTGRES_PASSWORD=senha-local'
    )

    foreach ($comando in @('docker', 'dotnet', 'flutter'))
    {
        Set-Content -LiteralPath (Join-Path $pastaDosComandos "$comando.cmd") -Value '@exit /b 0'
    }

    $env:PATH = "$pastaDosComandos;$caminhoAnterior"
    $saida = & pwsh -NoProfile -File (Join-Path $pastaTemporaria 'rodar.ps1') -SomenteValidar 2>&1

    if ($LASTEXITCODE -ne 0)
    {
        throw "A validacao do inicializador falhou: $($saida -join [Environment]::NewLine)"
    }

    if (($saida -join [Environment]::NewLine) -notmatch 'Inicializacao validada com sucesso')
    {
        throw 'O inicializador nao confirmou que o ambiente foi validado.'
    }
}
finally
{
    $env:PATH = $caminhoAnterior
    Remove-Item -LiteralPath $pastaTemporaria -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host 'Testes do inicializador concluidos com sucesso.' -ForegroundColor Green
