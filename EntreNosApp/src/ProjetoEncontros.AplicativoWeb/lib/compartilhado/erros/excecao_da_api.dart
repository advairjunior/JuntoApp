class ExcecaoDaApi implements Exception {
  const ExcecaoDaApi({
    required this.mensagem,
    this.codigoHttp,
  });

  final String mensagem;
  final int? codigoHttp;

  bool get usuarioNaoEstaAutenticado => codigoHttp == 401;

  @override
  String toString() => mensagem;
}
