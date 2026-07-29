class ItemDoEncontro {
  const ItemDoEncontro({
    required this.identificador,
    required this.identificadorDoEncontro,
    required this.descricao,
    required this.situacao,
    required this.identificadorDoUsuarioQueCriou,
    required this.usuarioAtualEhResponsavel,
    required this.criadoEm,
    required this.atualizadoEm,
    this.identificadorDoUsuarioResponsavel,
    this.nomeDoResponsavel,
    this.urlDaFotoDePerfilDoResponsavel,
  });

  factory ItemDoEncontro.deJson(Map<String, dynamic> json) {
    return ItemDoEncontro(
      identificador: json['identificador'] as String,
      identificadorDoEncontro: json['identificadorDoEncontro'] as String,
      descricao: json['descricao'] as String,
      situacao: json['situacao'] as String,
      identificadorDoUsuarioQueCriou:
          json['identificadorDoUsuarioQueCriou'] as String,
      identificadorDoUsuarioResponsavel:
          json['identificadorDoUsuarioResponsavel'] as String?,
      nomeDoResponsavel: json['nomeDoResponsavel'] as String?,
      urlDaFotoDePerfilDoResponsavel:
          json['urlDaFotoDePerfilDoResponsavel'] as String?,
      usuarioAtualEhResponsavel:
          json['usuarioAtualEhResponsavel'] as bool? ?? false,
      criadoEm: DateTime.parse(json['criadoEm'] as String).toLocal(),
      atualizadoEm: DateTime.parse(json['atualizadoEm'] as String).toLocal(),
    );
  }

  final String identificador;
  final String identificadorDoEncontro;
  final String descricao;
  final String situacao;
  final String identificadorDoUsuarioQueCriou;
  final String? identificadorDoUsuarioResponsavel;
  final String? nomeDoResponsavel;
  final String? urlDaFotoDePerfilDoResponsavel;
  final bool usuarioAtualEhResponsavel;
  final DateTime criadoEm;
  final DateTime atualizadoEm;

  bool get estaResolvido => situacao.toLowerCase() == 'resolvido';
}
