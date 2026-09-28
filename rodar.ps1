param(
    [int]$PortaApi = 5281,
    [int]$PortaWeb = 5391,
    [switch]$SomenteValidar
)

$ErrorActionPreference = 'Stop'

$raizDoProjeto = $PSScriptRoot
$arquivoDeAmbiente = Join-Path $raizDoProjeto '.env'
$arquivoDoDocker = Join-Path $raizDoProjeto 'docker-compose.yml'
$projetoDaApi = Join-Path $raizDoProjeto 'src\ProjetoEncontros.Api\ProjetoEncontros.Api.csproj'
$pastaDoAplicativo = Join-Path $raizDoProjeto 'src\ProjetoEncontros.AplicativoWeb'
$enderecoDaApi = "http://127.0.0.1:$PortaApi"
$enderecoDoAplicativo = "http://localhost:$PortaWeb"
$processoDaApi = $null
$cadeiaDeConexaoAnterior = $env:ConnectionStrings__DefaultConnection
$chaveJwtAnterior = $env:Jwt__Chave
$ambienteAnterior = $env:ASPNETCORE_ENVIRONMENT

function Exija-Comando
{
    param([string]$Nome)

    $comando = Get-Command $Nome -ErrorAction SilentlyContinue

    if ($null -eq $comando)
    {
        throw "O comando '$Nome' nao foi encontrado. Instale-o e tente novamente."
    }

    return $comando.Source
}

function Exija-Arquivo
{
    param(
        [string]$Caminho,
        [string]$Descricao
    )

    if (-not (Test-Path -LiteralPath $Caminho -PathType Leaf))
    {
        throw "$Descricao nao encontrado em '$Caminho'."
    }
}

function Exija-Pasta
{
    param(
        [string]$Caminho,
        [string]$Descricao
    )

    if (-not (Test-Path -LiteralPath $Caminho -PathType Container))
    {
        throw "$Descricao nao encontrada em '$Caminho'."
    }
}

function Leia-ConfiguracaoDoAmbiente
{
    param([string]$Caminho)

    $configuracao = @{}

    foreach ($linhaOriginal in Get-Content -LiteralPath $Caminho)
    {
        $linha = $linhaOriginal.Trim()

        if ([string]::IsNullOrWhiteSpace($linha) -or $linha.StartsWith('#'))
        {
            continue
        }

        $partes = $linha.Split('=', 2)

        if ($partes.Count -ne 2)
        {
            continue
        }

        $chave = $partes[0].Trim()
        $valor = $partes[1].Trim()

        if (($valor.StartsWith('"') -and $valor.EndsWith('"')) -or
            ($valor.StartsWith("'") -and $valor.EndsWith("'")))
        {
            $valor = $valor.Substring(1, $valor.Length - 2)
        }

        $configuracao[$chave] = $valor
    }

    return $configuracao
}

function Exija-Configuracao
{
    param(
        [hashtable]$Configuracao,
        [string]$Chave
    )

    if (-not $Configuracao.ContainsKey($Chave) -or
        [string]::IsNullOrWhiteSpace($Configuracao[$Chave]))
    {
        throw "A configuracao '$Chave' deve ser informada no arquivo .env."
    }

    return $Configuracao[$Chave]
}

function Formate-ValorDaCadeiaDeConexao
{
    param([string]$Valor)

    return '"' + $Valor.Replace('"', '""') + '"'
}

function Teste-ApiDisponivel
{
    try
    {
        Invoke-RestMethod -Uri "$enderecoDaApi/health/live" -TimeoutSec 1 | Out-Null
        return $true
    }
    catch
    {
        return $false
    }
}

function Espere-BancoDeDados
{
    param(
        [string]$Usuario,
        [string]$Banco
    )

    for ($tentativa = 0; $tentativa -lt 60; $tentativa++)
    {
        & docker compose exec -T postgres pg_isready -U $Usuario -d $Banco *> $null

        if ($LASTEXITCODE -eq 0)
        {
            return
        }

        Start-Sleep -Milliseconds 500
    }

    throw 'O PostgreSQL nao ficou disponivel dentro do tempo esperado.'
}

Exija-Arquivo -Caminho $arquivoDeAmbiente -Descricao 'Arquivo de configuracao .env'
Exija-Arquivo -Caminho $arquivoDoDocker -Descricao 'Arquivo do Docker Compose'
Exija-Arquivo -Caminho $projetoDaApi -Descricao 'Projeto da API'
Exija-Pasta -Caminho $pastaDoAplicativo -Descricao 'Pasta do aplicativo Flutter'

$docker = Exija-Comando -Nome 'docker'
$dotnet = Exija-Comando -Nome 'dotnet'
$flutter = Exija-Comando -Nome 'flutter'
$configuracaoDoAmbiente = Leia-ConfiguracaoDoAmbiente -Caminho $arquivoDeAmbiente
$nomeDoBanco = Exija-Configuracao -Configuracao $configuracaoDoAmbiente -Chave 'POSTGRES_DB'
$usuarioDoBanco = Exija-Configuracao -Configuracao $configuracaoDoAmbiente -Chave 'POSTGRES_USER'
$senhaDoBanco = Exija-Configuracao -Configuracao $configuracaoDoAmbiente -Chave 'POSTGRES_PASSWORD'

if ($SomenteValidar)
{
    Write-Host 'Inicializacao validada com sucesso.' -ForegroundColor Green
    return
}

try
{
    Write-Host 'Iniciando o PostgreSQL...' -ForegroundColor Cyan
    Push-Location $raizDoProjeto

    try
    {
        & $docker compose up -d postgres

        if ($LASTEXITCODE -ne 0)
        {
            throw "O Docker Compose foi encerrado com o codigo $LASTEXITCODE."
        }

        Espere-BancoDeDados -Usuario $usuarioDoBanco -Banco $nomeDoBanco
    }
    finally
    {
        Pop-Location
    }

    $bancoFormatado = Formate-ValorDaCadeiaDeConexao -Valor $nomeDoBanco
    $usuarioFormatado = Formate-ValorDaCadeiaDeConexao -Valor $usuarioDoBanco
    $senhaFormatada = Formate-ValorDaCadeiaDeConexao -Valor $senhaDoBanco
    $env:ConnectionStrings__DefaultConnection =
        "Host=127.0.0.1;Port=5432;Database=$bancoFormatado;Username=$usuarioFormatado;Password=$senhaFormatada"
    $env:Jwt__Chave = 'chave-local-exclusiva-do-entre-nos-2026'
    $env:ASPNETCORE_ENVIRONMENT = 'Development'

    if (-not (Teste-ApiDisponivel))
    {
        Write-Host 'Iniciando a API do EntreNos...' -ForegroundColor Cyan
        $argumentosDaApi = @(
            'run',
            '--project', $projetoDaApi,
            '--no-launch-profile',
            '--urls', "http://0.0.0.0:$PortaApi"
        )
        $processoDaApi = Start-Process `
            -FilePath $dotnet `
            -ArgumentList $argumentosDaApi `
            -WorkingDirectory $raizDoProjeto `
            -WindowStyle Hidden `
            -PassThru

        $apiDisponivel = $false

        for ($tentativa = 0; $tentativa -lt 120; $tentativa++)
        {
            if ($processoDaApi.HasExited)
            {
                throw 'A API foi encerrada antes de ficar disponivel.'
            }

            if (Teste-ApiDisponivel)
            {
                $apiDisponivel = $true
                break
            }

            Start-Sleep -Milliseconds 500
        }

        if (-not $apiDisponivel)
        {
            throw "A API nao respondeu em $enderecoDaApi."
        }
    }
    else
    {
        Write-Host "A API ja esta disponivel em $enderecoDaApi."
    }

    Write-Host ''
    Write-Host 'EntreNos iniciado.' -ForegroundColor Green
    Write-Host "Abra no navegador: $enderecoDoAplicativo" -ForegroundColor Yellow
    Write-Host 'Pressione Ctrl+C para encerrar o aplicativo e a API.'
    Write-Host ''

    Push-Location $pastaDoAplicativo

    try
    {
        & $flutter run `
            -d web-server `
            --web-hostname localhost `
            --web-port $PortaWeb `
            --dart-define="URL_DA_API=$enderecoDaApi"

        if ($LASTEXITCODE -ne 0)
        {
            throw "O Flutter foi encerrado com o codigo $LASTEXITCODE."
        }
    }
    finally
    {
        Pop-Location
    }
}
finally
{
    if ($null -ne $processoDaApi -and -not $processoDaApi.HasExited)
    {
        Write-Host 'Encerrando a API do EntreNos...'
        Stop-Process -Id $processoDaApi.Id
        [void]$processoDaApi.WaitForExit(5000)
    }

    $env:ConnectionStrings__DefaultConnection = $cadeiaDeConexaoAnterior
    $env:Jwt__Chave = $chaveJwtAnterior
    $env:ASPNETCORE_ENVIRONMENT = $ambienteAnterior
}
