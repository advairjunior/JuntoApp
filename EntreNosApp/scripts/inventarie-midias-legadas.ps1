param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("Development", "Homologacao", "Production")]
    [string]$Ambiente,

    [Parameter(Mandatory = $true)]
    [string]$PastaDeOrigem,

    [Parameter(Mandatory = $true)]
    [string]$BancoEsperado,

    [string]$ArquivoDoManifesto = ".verificacao\midias\manifesto.json",

    [switch]$ConfirmarProducao
)

$ErrorActionPreference = "Stop"
$ambienteAnterior = $env:ASPNETCORE_ENVIRONMENT
$env:ASPNETCORE_ENVIRONMENT = $Ambiente
$diretorioAtual = (Get-Location).Path
$pastaDeOrigemAbsoluta = (Resolve-Path -LiteralPath $PastaDeOrigem).Path
$arquivoDoManifestoAbsoluto = [System.IO.Path]::GetFullPath(
    $ArquivoDoManifesto,
    $diretorioAtual)

$argumentos = @(
    "run",
    "--no-restore",
    "--no-launch-profile",
    "--project",
    "src\ProjetoEncontros.Api\ProjetoEncontros.Api.csproj",
    "--",
    "--inventariar-midias-legadas",
    "--pasta-origem=$pastaDeOrigemAbsoluta",
    "--banco-esperado=$BancoEsperado",
    "--arquivo-manifesto=$arquivoDoManifestoAbsoluto"
)

if ($ConfirmarProducao) {
    $argumentos += "--confirmar-producao"
}

try {
    & dotnet $argumentos

    if ($LASTEXITCODE -ne 0) {
        throw "O inventario terminou bloqueado. Consulte o manifesto e os erros acima."
    }
} finally {
    if ($null -eq $ambienteAnterior) {
        Remove-Item Env:\ASPNETCORE_ENVIRONMENT -ErrorAction SilentlyContinue
    } else {
        $env:ASPNETCORE_ENVIRONMENT = $ambienteAnterior
    }
}
