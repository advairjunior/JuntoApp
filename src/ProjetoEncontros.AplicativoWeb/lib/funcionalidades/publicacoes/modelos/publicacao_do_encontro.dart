class PublicacaoDoEncontro {
  const PublicacaoDoEncontro({
    required this.identificador,
    required this.identificadorDoEncontro,
    required this.identificadorDoUsuarioAutor,
    required this.nomeDoAutor,
    required this.publicadoEm,
    required this.ehAtualizacaoDoSistema,
    required this.usuarioAtual,
    this.urlDaFotoDePerfilDoAutor,
    this.texto,
    this.urlDaMidia,
    this.nomeOriginalDaMidia,
    this.tipoDeConteudoDaMidia,
    this.tamanhoDaMidiaEmBytes,
  });

  factory PublicacaoDoEncontro.deJson(Map<String, dynamic> json) {
    return PublicacaoDoEncontro(
      identificador: json['identificador'] as String,
      identificadorDoEncontro: json['identificadorDoEncontro'] as String,
      identificadorDoUsuarioAutor:
          json['identificadorDoUsuarioAutor'] as String,
      nomeDoAutor: json['nomeDoAutor'] as String,
      urlDaFotoDePerfilDoAutor: json['urlDaFotoDePerfilDoAutor'] as String?,
      texto: json['texto'] as String?,
      urlDaMidia: json['urlDaMidia'] as String?,
      nomeOriginalDaMidia: json['nomeOriginalDaMidia'] as String?,
      tipoDeConteudoDaMidia: json['tipoDeConteudoDaMidia'] as String?,
      tamanhoDaMidiaEmBytes: json['tamanhoDaMidiaEmBytes'] as int?,
      publicadoEm: DateTime.parse(json['publicadoEm'] as String).toLocal(),
      ehAtualizacaoDoSistema: json['ehAtualizacaoDoSistema'] as bool,
      usuarioAtual: json['usuarioAtual'] as bool,
    );
  }

  final String identificador;
  final String identificadorDoEncontro;
  final String identificadorDoUsuarioAutor;
  final String nomeDoAutor;
  final String? urlDaFotoDePerfilDoAutor;
  final String? texto;
  final String? urlDaMidia;
  final String? nomeOriginalDaMidia;
  final String? tipoDeConteudoDaMidia;
  final int? tamanhoDaMidiaEmBytes;
  final DateTime publicadoEm;
  final bool ehAtualizacaoDoSistema;
  final bool usuarioAtual;

  bool get temMidia => urlDaMidia != null && urlDaMidia!.trim().isNotEmpty;

  bool get ehImagem =>
      temMidia &&
      tipoDeConteudoDaMidia != null &&
      tipoDeConteudoDaMidia!.toLowerCase().startsWith('image/');
}
