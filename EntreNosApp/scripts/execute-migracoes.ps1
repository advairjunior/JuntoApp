param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("Development", "Homologacao", "Production")]
    [string]$Ambiente,

    [switch]$Aplicar,

    [switch]$ConfirmarProducao,

    [string]$MigracaoAlvo,

    [string]$BancoEsperado
)

$ErrorActionPreference = "Stop"
$env:ASPNETCORE_ENVIRONMENT = $Ambiente

$argumentos = @(
    "run",
    "--no-restore",
    "--no-launch-profile",
    "--project",
    "src\ProjetoEncontros.Api\ProjetoEncontros.Api.csproj",
    "--",
    "--migrar-banco"
)

if ($Aplicar) {
    if ([string]::IsNullOrWhiteSpace($MigracaoAlvo)) {
        throw "Informe -MigracaoAlvo ao aplicar uma migracao."
    }

    if ([string]::IsNullOrWhiteSpace($BancoEsperado)) {
        throw "Informe -BancoEsperado ao aplicar uma migracao."
    }

    $argumentos += "--aplicar"
    $argumentos += "--migracao-alvo=$MigracaoAlvo"
    $argumentos += "--banco-esperado=$BancoEsperado"
} else {
    $argumentos += "--verificar"
}

if ($ConfirmarProducao) {
    $argumentos += "--confirmar-producao"
}

try {
    & dotnet $argumentos

    if ($LASTEXITCODE -ne 0) {
        throw "A execucao de migracoes terminou com o codigo $LASTEXITCODE."
    }
} finally {
    Remove-Item Env:\ASPNETCORE_ENVIRONMENT -ErrorAction SilentlyContinue
}
