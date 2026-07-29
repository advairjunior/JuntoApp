class ItemDaLinhaDoTempo {
  const ItemDaLinhaDoTempo({
    required this.identificadorDoEncontro,
    required this.titulo,
    required this.inicio,
    required this.situacao,
    required this.quantidadeDeParticipantes,
    required this.quantidadeDeMemorias,
    required this.quantidadeDePublicacoes,
    required this.nomesDosParticipantesEmDestaque,
    this.descricao,
    this.local,
    this.urlDaImagem,
  });

  factory ItemDaLinhaDoTempo.deJson(Map<String, dynamic> json) {
    List<dynamic> nomes =
        json['nomesDosParticipantesEmDestaque'] as List<dynamic>? ??
            <dynamic>[];

    return ItemDaLinhaDoTempo(
      identificadorDoEncontro: json['identificadorDoEncontro'] as String,
      titulo: json['titulo'] as String,
      descricao: json['descricao'] as String?,
      local: json['local'] as String?,
      inicio: DateTime.parse(json['inicio'] as String).toLocal(),
      situacao: json['situacao'] as String,
      urlDaImagem: json['urlDaImagem'] as String?,
      quantidadeDeParticipantes: json['quantidadeDeParticipantes'] as int,
      quantidadeDeMemorias: json['quantidadeDeMemorias'] as int,
      quantidadeDePublicacoes: json['quantidadeDePublicacoes'] as int,
      nomesDosParticipantesEmDestaque:
          nomes.map((dynamic nome) => nome as String).toList(),
    );
  }

  final String identificadorDoEncontro;
  final String titulo;
  final String? descricao;
  final String? local;
  final DateTime inicio;
  final String situacao;
  final String? urlDaImagem;
  final int quantidadeDeParticipantes;
  final int quantidadeDeMemorias;
  final int quantidadeDePublicacoes;
  final List<String> nomesDosParticipantesEmDestaque;
}

class LinhaDoTempo {
  const LinhaDoTempo({required this.filtro, required this.itens});

  factory LinhaDoTempo.deJson(Map<String, dynamic> json) {
    List<dynamic> itensRecebidos =
        json['itens'] as List<dynamic>? ?? <dynamic>[];

    return LinhaDoTempo(
      filtro: json['filtro'] as String,
      itens: itensRecebidos
          .map(
            (dynamic item) => ItemDaLinhaDoTempo.deJson(
              Map<String, dynamic>.from(item as Map<dynamic, dynamic>),
            ),
          )
          .toList(),
    );
  }

  final String filtro;
  final List<ItemDaLinhaDoTempo> itens;
}
