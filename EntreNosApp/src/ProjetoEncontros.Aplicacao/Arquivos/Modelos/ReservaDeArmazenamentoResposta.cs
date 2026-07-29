using ProjetoEncontros.Dominio.Arquivos;

namespace ProjetoEncontros.Aplicacao.Arquivos.Modelos;

public sealed record ReservaDeArmazenamentoResposta(
    Guid IdentificadorDaReserva,
    string ChaveDoObjeto,
    DateTimeOffset ExpiraEm,
    SituacaoDoArquivoArmazenado Situacao,
    bool PodeEnviar);
