class ParticipanteDoEncontro {
  const ParticipanteDoEncontro({
    required this.identificadorDoUsuario,
    required this.nome,
    required this.papel,
    required this.situacao,
    required this.usuarioAtual,
    this.urlDaFotoDePerfil,
  });

  factory ParticipanteDoEncontro.deJson(Map<String, dynamic> json) {
    return ParticipanteDoEncontro(
      identificadorDoUsuario: json['identificadorDoUsuario'] as String,
      nome: json['nome'] as String,
      urlDaFotoDePerfil: json['urlDaFotoDePerfil'] as String?,
      papel: json['papel'] as String,
      situacao: json['situacao'] as String,
      usuarioAtual: json['usuarioAtual'] as bool,
    );
  }

  final String identificadorDoUsuario;
  final String nome;
  final String? urlDaFotoDePerfil;
  final String papel;
  final String situacao;
  final bool usuarioAtual;

  String get iniciais {
    List<String> partes = nome
        .trim()
        .split(RegExp(r'\s+'))
        .where((String parte) => parte.isNotEmpty)
        .toList();

    if (partes.isEmpty) {
      return '?';
    }

    if (partes.length == 1) {
      return partes.first.substring(0, 1).toUpperCase();
    }

    return '${partes.first.substring(0, 1)}${partes.last.substring(0, 1)}'
        .toUpperCase();
  }
}
