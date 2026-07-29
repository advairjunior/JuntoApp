param(
    [int]$Porta = 5391
)

$ErrorActionPreference = 'Stop'
$pastaDoProjeto = Join-Path $PSScriptRoot '..\src\ProjetoEncontros.AplicativoWeb'
$endereco = "http://localhost:$Porta"

Write-Host ''
Write-Host 'Junto - aplicativo web local' -ForegroundColor Green
Write-Host "Abra no seu navegador normal: $endereco" -ForegroundColor Cyan
Write-Host 'A sessao sera preservada pelo cookie do navegador por ate 30 dias.'
Write-Host 'Pressione Ctrl+C para encerrar o servidor.'
Write-Host ''

Push-Location $pastaDoProjeto

try {
    flutter run -d web-server --web-hostname localhost --web-port $Porta
}
finally {
    Pop-Location
}
