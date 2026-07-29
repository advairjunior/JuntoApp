namespace ProjetoEncontros.Api.Contratos.Convites;

public sealed record RespostaDeConviteRespondido(Guid Identificador, Guid IdentificadorDoGrupo, string Situacao);
