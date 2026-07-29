param(
    [int]$PortaDaApi = 5281,
    [int]$PortaDoAplicativo = 5391
)

$ErrorActionPreference = 'Stop'
$pastaDoProjeto = Split-Path -Parent $PSScriptRoot
$pastaDosTestes = Join-Path $pastaDoProjeto 'tests\ProjetoEncontros.TestesNavegador'
$pastaDosRegistros = Join-Path $pastaDoProjeto '.verificacao\testes-navegador\registros'
$registroFlutterSaida = Join-Path $pastaDosRegistros 'flutter-saida.log'
$arquivoDeAmbiente = Join-Path $pastaDoProjeto '.env'
$nomeDoBancoDeTestes = 'projeto_encontros_navegador_testes'
$processoDaApi = $null
$processoDoAplicativo = $null

function ObtenhaConfiguracaoDoAmbiente {
    param(
        [string]$Nome
    )

    $linha = Get-Content -LiteralPath $arquivoDeAmbiente |
        Where-Object { $_ -match "^\s*$([regex]::Escape($Nome))=" } |
        Select-Object -Last 1

    if ($null -eq $linha) {
        throw "A configuracao $Nome nao foi encontrada no arquivo .env."
    }

    return ($linha -split '=', 2)[1].Trim()
}

function ConfirmePortaLivre {
    param(
        [int]$Porta
    )

    $conexoes = @(
        Get-NetTCPConnection -State Listen -ErrorAction SilentlyContinue |
            Where-Object { $_.LocalPort -eq $Porta }
    )

    if ($conexoes.Count -gt 0) {
        throw "A porta $Porta ja esta em uso. Encerre o projeto aberto nela e tente novamente."
    }
}

function EncerreProcessoDaPorta {
    param(
        [int]$Porta
    )

    $expressoes = netstat -ano |
        Select-String -Pattern "[:.]$Porta\s+.*LISTENING\s+(\d+)\s*$"

    foreach ($expressao in $expressoes) {
        $identificadorDoProcesso = [int]$expressao.Matches[0].Groups[1].Value
        Stop-Process -Id $identificadorDoProcesso -Force -ErrorAction SilentlyContinue
    }
}

function GarantaDockerDisponivel {
    docker info *> $null

    if ($LASTEXITCODE -eq 0) {
        return
    }

    $executavelDoDockerDesktop =
        'C:\Program Files\Docker\Docker\Docker Desktop.exe'

    if (-not (Test-Path -LiteralPath $executavelDoDockerDesktop)) {
        throw 'O Docker nao esta disponivel e o Docker Desktop nao foi encontrado.'
    }

    Start-Process `
        -FilePath $executavelDoDockerDesktop `
        -WindowStyle Hidden | Out-Null

    $limite = (Get-Date).AddMinutes(3)

    while ((Get-Date) -lt $limite) {
        Start-Sleep -Seconds 2
        docker info *> $null

        if ($LASTEXITCODE -eq 0) {
            return
        }
    }

    throw 'O Docker Desktop foi iniciado, mas o mecanismo nao ficou disponivel.'
}

function AguardeEndereco {
    param(
        [string]$Endereco,
        [AllowNull()]
        [System.Diagnostics.Process]$Processo,
        [string]$Nome
    )

    $limite = (Get-Date).AddMinutes(3)

    while ((Get-Date) -lt $limite) {
        if ($null -ne $Processo -and $Processo.HasExited) {
            throw "$Nome foi encerrado antes de ficar disponivel."
        }

        try {
            Invoke-WebRequest `
                -Uri $Endereco `
                -UseBasicParsing `
                -SkipHttpErrorCheck `
                -TimeoutSec 3 | Out-Null
            return
        }
        catch {
            Start-Sleep -Seconds 1
        }
    }

    throw "$Nome nao respondeu em $Endereco dentro do tempo esperado."
}

function AguardeBanco {
    param(
        [string]$Usuario
    )

    $limite = (Get-Date).AddMinutes(2)

    while ((Get-Date) -lt $limite) {
        docker compose exec -T postgres pg_isready `
            -U $Usuario `
            -d postgres *> $null

        if ($LASTEXITCODE -eq 0) {
            return
        }

        Start-Sleep -Seconds 1
    }

    throw 'O PostgreSQL nao ficou disponivel dentro do tempo esperado.'
}

function AguardeCompilacaoDoFlutter {
    $limite = (Get-Date).AddMinutes(3)

    while ((Get-Date) -lt $limite) {
        if (Test-Path -LiteralPath $registroFlutterSaida) {
            $aplicativoFoiServido = Select-String `
                -LiteralPath $registroFlutterSaida `
                -Pattern 'is being served at' `
                -Quiet

            if ($aplicativoFoiServido) {
                return
            }
        }

        Start-Sleep -Seconds 1
    }

    throw 'O Flutter nao concluiu a compilacao dentro do tempo esperado.'
}

if (-not (Test-Path -LiteralPath $arquivoDeAmbiente)) {
    throw 'Crie o arquivo .env local antes de executar os testes de navegador.'
}

ConfirmePortaLivre -Porta $PortaDaApi
ConfirmePortaLivre -Porta $PortaDoAplicativo

$usuarioDoBanco = ObtenhaConfiguracaoDoAmbiente -Nome 'POSTGRES_USER'
$senhaDoBanco = ObtenhaConfiguracaoDoAmbiente -Nome 'POSTGRES_PASSWORD'

New-Item -ItemType Directory -Force -Path $pastaDosRegistros | Out-Null

Push-Location $pastaDoProjeto

try {
    GarantaDockerDisponivel
    docker compose up -d postgres

    if ($LASTEXITCODE -ne 0) {
        throw 'Nao foi possivel iniciar o PostgreSQL pelo Docker Compose.'
    }

    AguardeBanco -Usuario $usuarioDoBanco

    $bancoExiste = docker compose exec -T postgres psql `
        -U $usuarioDoBanco `
        -d postgres `
        -tAc "SELECT 1 FROM pg_database WHERE datname = '$nomeDoBancoDeTestes';"

    if ($LASTEXITCODE -ne 0) {
        throw 'Nao foi possivel consultar o banco exclusivo dos testes.'
    }

    if ([string]::IsNullOrWhiteSpace($bancoExiste) -or $bancoExiste.Trim() -ne '1') {
        docker compose exec -T postgres createdb `
            -U $usuarioDoBanco `
            $nomeDoBancoDeTestes

        if ($LASTEXITCODE -ne 0) {
            throw 'Nao foi possivel criar o banco exclusivo dos testes.'
        }
    }

    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:ASPNETCORE_URLS = "http://localhost:$PortaDaApi"
    $env:ConnectionStrings__DefaultConnection =
        "Host=localhost;Port=5432;Database=$nomeDoBancoDeTestes;Username=$usuarioDoBanco;Password=$senhaDoBanco"
    $env:Jwt__Emissor = 'ProjetoEncontros.TestesNavegador'
    $env:Jwt__Publico = 'ProjetoEncontros.TestesNavegador'
    $env:Jwt__Chave = 'chave-exclusiva-dos-testes-de-navegador-2026'

    $executavelDotnet = (Get-Command dotnet).Source
    $executavelFlutter = (Get-Command flutter).Source

    $processoDaApi = Start-Process `
        -FilePath $executavelDotnet `
        -ArgumentList @(
            'run',
            '--no-launch-profile',
            '--project',
            'src\ProjetoEncontros.Api\ProjetoEncontros.Api.csproj'
        ) `
        -WorkingDirectory $pastaDoProjeto `
        -WindowStyle Hidden `
        -RedirectStandardOutput (Join-Path $pastaDosRegistros 'api-saida.log') `
        -RedirectStandardError (Join-Path $pastaDosRegistros 'api-erro.log') `
        -PassThru

    $processoDoAplicativo = Start-Process `
        -FilePath $executavelFlutter `
        -ArgumentList @(
            'run',
            '-d',
            'web-server',
            '--web-hostname',
            'localhost',
            '--web-port',
            $PortaDoAplicativo
        ) `
        -WorkingDirectory (Join-Path $pastaDoProjeto 'src\ProjetoEncontros.AplicativoWeb') `
        -WindowStyle Hidden `
        -RedirectStandardOutput $registroFlutterSaida `
        -RedirectStandardError (Join-Path $pastaDosRegistros 'flutter-erro.log') `
        -PassThru

    AguardeEndereco `
        -Endereco "http://localhost:$PortaDaApi/" `
        -Processo $processoDaApi `
        -Nome 'A API'
    AguardeEndereco `
        -Endereco "http://localhost:$PortaDoAplicativo/" `
        -Processo $null `
        -Nome 'O aplicativo Flutter'
    AguardeCompilacaoDoFlutter

    Push-Location $pastaDosTestes

    try {
        npm test

        if ($LASTEXITCODE -ne 0) {
            throw "Os testes de navegador falharam com o codigo $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}
finally {
    if ($null -ne $processoDoAplicativo -and -not $processoDoAplicativo.HasExited) {
        Stop-Process -Id $processoDoAplicativo.Id -Force
    }

    if ($null -ne $processoDaApi -and -not $processoDaApi.HasExited) {
        Stop-Process -Id $processoDaApi.Id -Force
    }

    EncerreProcessoDaPorta -Porta $PortaDoAplicativo
    EncerreProcessoDaPorta -Porta $PortaDaApi

    Pop-Location
}
