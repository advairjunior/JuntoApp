param(
    [switch]$PreservarAmbiente
)

$ErrorActionPreference = "Stop"

$projetoCompose = "encontros-ensaio-v103"
$servicoPostgres = "postgres-ensaio"
$usuario = "projeto_encontros"
$senha = "projeto_encontros_ensaio"
$porta = 55433
$bancoDeOrigem = "projeto_encontros_ensaio_migracoes"
$bancoRestaurado = "projeto_encontros_ensaio_restaurado"
$migracaoBase = "20260714123531_V07TipoOpcionalDoEncontro"
$migracaoFinal = "20260720134707_V103AlertasDaCotaDeArmazenamento"
$identificadorDaTrava = "48792026072001"
$arquivoTemporarioNoConteiner = "/tmp/projeto-encontros-ensaio.dump"
$raizDoProjeto = Split-Path $PSScriptRoot -Parent
$arquivoCompose = Join-Path $raizDoProjeto "docker-compose.ensaio-migracoes.yml"
$pastaDeEvidencias = Join-Path $raizDoProjeto ".verificacao\migracoes"
$arquivoDeBackup = Join-Path $pastaDeEvidencias "ensaio-v07.dump"
$localizacaoAnterior = Get-Location
$identificadorDoConteiner = $null

function ExecuteDocker([string[]]$argumentos) {
    & docker @argumentos

    if ($LASTEXITCODE -ne 0) {
        throw "O comando Docker falhou com o codigo $LASTEXITCODE."
    }
}

function ExecuteCompose([string[]]$argumentos) {
    [string[]]$argumentosCompletos = @(
        "compose", "--project-name", $projetoCompose, "--file", $arquivoCompose
    ) + $argumentos
    ExecuteDocker $argumentosCompletos
}

function ConfirmeNomeDescartavel([string]$nomeDoBanco) {
    if ($nomeDoBanco -notin @($bancoDeOrigem, $bancoRestaurado)) {
        throw "O banco '$nomeDoBanco' nao pertence ao ensaio descartavel."
    }
}

function ConsulteValor([string]$nomeDoBanco, [string]$consulta) {
    [string[]]$saida = @(& docker exec $identificadorDoConteiner psql `
        --username $usuario `
        --dbname $nomeDoBanco `
        --tuples-only `
        --no-align `
        --set ON_ERROR_STOP=1 `
        --command $consulta)

    if ($LASTEXITCODE -ne 0) {
        throw "A consulta de validacao falhou no banco '$nomeDoBanco'."
    }

    return ($saida -join "`n").Trim()
}

function CrieBancoDescartavel([string]$nomeDoBanco) {
    ConfirmeNomeDescartavel $nomeDoBanco
    ExecuteDocker @(
        "exec", $identificadorDoConteiner,
        "dropdb", "--username", $usuario, "--if-exists", "--force", $nomeDoBanco
    ) | Out-Null
    ExecuteDocker @(
        "exec", $identificadorDoConteiner,
        "createdb", "--username", $usuario, $nomeDoBanco
    ) | Out-Null
}

function ApliqueMigracao([string]$nomeDoBanco, [string]$migracaoAlvo) {
    ConfirmeNomeDescartavel $nomeDoBanco
    $env:ConnectionStrings__DefaultConnection =
        "Host=127.0.0.1;Port=$porta;Database=$nomeDoBanco;Username=$usuario;Password=$senha"
    & "$PSScriptRoot\execute-migracoes.ps1" `
        -Ambiente Homologacao `
        -Aplicar `
        -MigracaoAlvo $migracaoAlvo `
        -BancoEsperado $nomeDoBanco
}

function VerifiqueSemPendencias([string]$nomeDoBanco) {
    ConfirmeNomeDescartavel $nomeDoBanco
    $env:ConnectionStrings__DefaultConnection =
        "Host=127.0.0.1;Port=$porta;Database=$nomeDoBanco;Username=$usuario;Password=$senha"
    & "$PSScriptRoot\execute-migracoes.ps1" -Ambiente Homologacao
}

function ValideEstruturaFinal([string]$nomeDoBanco) {
    $migracaoAtual = ConsulteValor $nomeDoBanco `
        'SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId" DESC LIMIT 1;'
    $quantidadeDeCotas = ConsulteValor $nomeDoBanco `
        'SELECT count(*) FROM cotas_de_armazenamento WHERE limite_em_bytes = 8589934592 AND bytes_ativos = 0 AND bytes_reservados = 0;'
    $indiceDeIdempotencia = ConsulteValor $nomeDoBanco `
        'SELECT count(*) FROM pg_indexes WHERE tablename = ''notificacoes_do_usuario'' AND indexdef LIKE ''%chave_de_idempotencia%'';'
    $quantidadeDeRestricoesDaLocalizacao = ConsulteValor $nomeDoBanco `
        'SELECT count(*) FROM pg_constraint WHERE conname LIKE ''ck_encontros_%'';'
    $evidencia = ConsulteValor $nomeDoBanco `
        'SELECT descricao FROM evidencia_do_ensaio WHERE identificador = 1;'

    if ($migracaoAtual -ne $migracaoFinal) {
        throw "O banco '$nomeDoBanco' nao chegou a migracao final esperada."
    }

    if ($quantidadeDeCotas -ne "1") {
        throw "A cota inicial da V102 nao foi validada no banco '$nomeDoBanco'."
    }

    if ($indiceDeIdempotencia -ne "1") {
        throw "O indice de idempotencia da V103 nao foi validado no banco '$nomeDoBanco'."
    }

    if ($quantidadeDeRestricoesDaLocalizacao -ne "4") {
        throw "As restricoes de localizacao da V101 nao foram validadas no banco '$nomeDoBanco'."
    }

    if ($evidencia -ne "backup-validado") {
        throw "A evidencia de dados nao foi preservada no banco '$nomeDoBanco'."
    }
}

New-Item -ItemType Directory -Force -Path $pastaDeEvidencias | Out-Null
Set-Location $raizDoProjeto

try {
    Write-Host "Iniciando PostgreSQL efemero e isolado na porta $porta..."
    ExecuteCompose @("up", "--detach", "--wait") | Out-Null
    [string[]]$saidaDoConteiner = @(& docker compose `
        --project-name $projetoCompose `
        --file $arquivoCompose `
        ps --quiet $servicoPostgres)
    $identificadorDoConteiner = ($saidaDoConteiner -join "").Trim()

    if ([string]::IsNullOrWhiteSpace($identificadorDoConteiner)) {
        throw "Nao foi possivel identificar o conteiner do ensaio."
    }

    CrieBancoDescartavel $bancoDeOrigem
    CrieBancoDescartavel $bancoRestaurado

    $identidadeDoServidor = ConsulteValor "postgres" `
        'SELECT current_database() || ''|'' || current_user;'
    [string[]]$saidaDaPorta = @(& docker port $identificadorDoConteiner "5432/tcp")
    $portaPublicada = ($saidaDaPorta -join "").Trim()

    if ($identidadeDoServidor -ne "postgres|$usuario" -or
        $portaPublicada -ne "127.0.0.1:$porta") {
        throw "A identidade interna do PostgreSQL efemero nao corresponde ao esperado."
    }

    Write-Host "Preparando a base descartavel ate $migracaoBase..."
    ApliqueMigracao $bancoDeOrigem $migracaoBase

    ExecuteDocker @(
        "exec", $identificadorDoConteiner,
        "psql", "--username", $usuario, "--dbname", $bancoDeOrigem,
        "--set", "ON_ERROR_STOP=1",
        "--command",
        "CREATE TABLE evidencia_do_ensaio (identificador integer PRIMARY KEY, descricao text NOT NULL); INSERT INTO evidencia_do_ensaio VALUES (1, 'backup-validado');"
    ) | Out-Null

    Write-Host "Gerando e inspecionando backup do estado V07..."
    ExecuteDocker @(
        "exec", $identificadorDoConteiner,
        "pg_dump", "--username", $usuario, "--dbname", $bancoDeOrigem,
        "--format=custom", "--no-owner", "--no-privileges",
        "--file=$arquivoTemporarioNoConteiner"
    ) | Out-Null
    ExecuteDocker @(
        "exec", $identificadorDoConteiner,
        "pg_restore", "--list", $arquivoTemporarioNoConteiner
    ) | Out-Null
    ExecuteDocker @(
        "cp", "${identificadorDoConteiner}:$arquivoTemporarioNoConteiner", $arquivoDeBackup
    ) | Out-Null
    $hashDoBackup = (Get-FileHash -LiteralPath $arquivoDeBackup -Algorithm SHA256).Hash

    Write-Host "Validando a trava contra uma segunda execucao..."
    ExecuteDocker @(
        "exec", "--detach", "--env", "PGAPPNAME=ensaio_trava_migracoes",
        $identificadorDoConteiner,
        "psql", "--username", $usuario, "--dbname", $bancoDeOrigem,
        "--command", "SELECT pg_advisory_lock($identificadorDaTrava); SELECT pg_sleep(30);"
    ) | Out-Null
    Start-Sleep -Seconds 2
    $execucaoConcorrenteFoiRecusada = $false

    try {
        ApliqueMigracao $bancoDeOrigem $migracaoFinal
    } catch {
        if ($_.Exception.Message -notlike "*execucao de migracoes terminou*") {
            throw
        }

        $execucaoConcorrenteFoiRecusada = $true
    } finally {
        ExecuteDocker @(
            "exec", $identificadorDoConteiner,
            "psql", "--username", $usuario, "--dbname", "postgres",
            "--set", "ON_ERROR_STOP=1",
            "--command",
            "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE application_name = 'ensaio_trava_migracoes';"
        ) | Out-Null
    }

    if (!$execucaoConcorrenteFoiRecusada) {
        throw "O executor nao recusou a aplicacao concorrente."
    }

    Write-Host "Aplicando V101, V102 e V103 no banco de origem..."
    ApliqueMigracao $bancoDeOrigem $migracaoFinal
    ApliqueMigracao $bancoDeOrigem $migracaoFinal
    VerifiqueSemPendencias $bancoDeOrigem
    ValideEstruturaFinal $bancoDeOrigem

    Write-Host "Restaurando o backup em outro banco vazio..."
    ExecuteDocker @(
        "cp", $arquivoDeBackup, "${identificadorDoConteiner}:$arquivoTemporarioNoConteiner"
    ) | Out-Null
    ExecuteDocker @(
        "exec", $identificadorDoConteiner,
        "pg_restore", "--username", $usuario, "--dbname", $bancoRestaurado,
        "--exit-on-error", "--single-transaction", "--no-owner", "--no-privileges",
        $arquivoTemporarioNoConteiner
    ) | Out-Null

    $migracaoRestaurada = ConsulteValor $bancoRestaurado `
        'SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId" DESC LIMIT 1;'

    if ($migracaoRestaurada -ne $migracaoBase) {
        throw "A restauracao nao preservou a migracao-base esperada."
    }

    Write-Host "Aplicando V101, V102 e V103 sobre o backup restaurado..."
    ApliqueMigracao $bancoRestaurado $migracaoFinal
    VerifiqueSemPendencias $bancoRestaurado
    ValideEstruturaFinal $bancoRestaurado

    Write-Host "Ensaio aprovado."
    Write-Host "Migracao-base:  $migracaoBase"
    Write-Host "Migracao final: $migracaoFinal"
    Write-Host "SHA-256:        $hashDoBackup"
    Write-Host "Backup:         $arquivoDeBackup"
} finally {
    Remove-Item Env:\ConnectionStrings__DefaultConnection -ErrorAction SilentlyContinue

    if (!$PreservarAmbiente) {
        ExecuteCompose @("down", "--volumes", "--remove-orphans") | Out-Null
    }

    Set-Location $localizacaoAnterior
}
