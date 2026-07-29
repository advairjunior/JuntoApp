class ConviteDoEncontroResumo {
  const ConviteDoEncontroResumo({
    required this.identificadorDoEncontro,
    required this.titulo,
    required this.inicioEm,
    required this.situacao,
    required this.convidadoEm,
    this.local,
  });

  factory ConviteDoEncontroResumo.deJson(Map<String, dynamic> json) {
    return ConviteDoEncontroResumo(
      identificadorDoEncontro: json['identificadorDoEncontro'] as String,
      titulo: json['titulo'] as String,
      local: json['local'] as String?,
      inicioEm: DateTime.parse(json['inicioEm'] as String).toLocal(),
      situacao: json['situacao'] as String,
      convidadoEm: DateTime.parse(json['convidadoEm'] as String).toLocal(),
    );
  }

  final String identificadorDoEncontro;
  final String titulo;
  final String? local;
  final DateTime inicioEm;
  final String situacao;
  final DateTime convidadoEm;
}
