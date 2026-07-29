class EncontroCriado {
  const EncontroCriado({
    required this.identificador,
    required this.titulo,
    required this.inicioEm,
    required this.situacao,
    this.descricao,
    this.local,
    this.tipo,
  });

  factory EncontroCriado.deJson(Map<String, dynamic> json) {
    return EncontroCriado(
      identificador: json['identificador'] as String,
      titulo: json['titulo'] as String,
      descricao: json['descricao'] as String?,
      local: json['local'] as String?,
      inicioEm: DateTime.parse(json['inicioEm'] as String).toLocal(),
      situacao: json['situacao'] as String,
      tipo: json['tipo'] as String?,
    );
  }

  final String identificador;
  final String titulo;
  final String? descricao;
  final String? local;
  final DateTime inicioEm;
  final String situacao;
  final String? tipo;
}
