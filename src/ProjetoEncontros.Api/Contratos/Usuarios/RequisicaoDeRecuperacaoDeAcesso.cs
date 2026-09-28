namespace ProjetoEncontros.Api.Contratos.Usuarios;

public sealed record RequisicaoDeRecuperacaoDeAcesso(
    string NovoNumeroDeCelular,
    string PinTemporario);
