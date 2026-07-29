namespace ProjetoEncontros.Aplicacao.PessoasFrequentes.Contratos;

public sealed record ObtenhaHistoricoComPessoaComando(
    Guid IdentificadorDoUsuario,
    Guid IdentificadorDaPessoa,
    int Pagina,
    int Tamanho,
    int LimiteDeMemorias);
