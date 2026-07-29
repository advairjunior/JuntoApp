class EncontroResumo {
  const EncontroResumo({
    required this.identificador,
    required this.titulo,
    required this.inicioEm,
    required this.situacao,
    required this.quantidadeDePresencasConfirmadas,
    required this.quantidadeDeNovidades,
    required this.usuarioAtualConfirmouPresenca,
    this.local,
    this.urlDaImagemDeCapa,
    this.tipo,
  });

  factory EncontroResumo.deJson(Map<String, dynamic> json) {
    return EncontroResumo(
      identificador: json['identificador'] as String,
      titulo: json['titulo'] as String,
      local: json['local'] as String?,
      urlDaImagemDeCapa: json['urlDaImagemDeCapa'] as String?,
      inicioEm: DateTime.parse(json['inicioEm'] as String).toLocal(),
      situacao: json['situacao'] as String,
      quantidadeDePresencasConfirmadas:
          json['quantidadeDePresencasConfirmadas'] as int,
      quantidadeDeNovidades: json['quantidadeDeNovidades'] as int,
      usuarioAtualConfirmouPresenca:
          json['usuarioAtualConfirmouPresenca'] as bool,
      tipo: json['tipo'] as String?,
    );
  }

  final String identificador;
  final String titulo;
  final String? local;
  final String? urlDaImagemDeCapa;
  final DateTime inicioEm;
  final String situacao;
  final int quantidadeDePresencasConfirmadas;
  final int quantidadeDeNovidades;
  final bool usuarioAtualConfirmouPresenca;
  final String? tipo;
}
