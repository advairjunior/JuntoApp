class NotificacaoDoUsuario {
  const NotificacaoDoUsuario({
    required this.identificador,
    required this.tipo,
    required this.titulo,
    required this.mensagem,
    required this.situacao,
    required this.criadaEm,
    this.identificadorDoEncontro,
    this.identificadorDoConvite,
    this.identificadorDoItem,
    this.lidaEm,
  });

  factory NotificacaoDoUsuario.deJson(Map<String, dynamic> json) {
    String? lidaEm = json['lidaEm'] as String?;

    return NotificacaoDoUsuario(
      identificador: json['identificador'] as String,
      tipo: json['tipo'] as String,
      titulo: json['titulo'] as String,
      mensagem: json['mensagem'] as String,
      identificadorDoEncontro: json['identificadorDoEncontro'] as String?,
      identificadorDoConvite: json['identificadorDoConvite'] as String?,
      identificadorDoItem: json['identificadorDoItem'] as String?,
      situacao: json['situacao'] as String,
      criadaEm: DateTime.parse(json['criadaEm'] as String).toLocal(),
      lidaEm: lidaEm == null ? null : DateTime.parse(lidaEm).toLocal(),
    );
  }

  final String identificador;
  final String tipo;
  final String titulo;
  final String mensagem;
  final String? identificadorDoEncontro;
  final String? identificadorDoConvite;
  final String? identificadorDoItem;
  final String situacao;
  final DateTime criadaEm;
  final DateTime? lidaEm;

  bool get estaLida => lidaEm != null || situacao.toLowerCase() == 'lida';
}

class ListaDeNotificacoes {
  const ListaDeNotificacoes({
    required this.quantidadeNaoLida,
    required this.notificacoes,
  });

  factory ListaDeNotificacoes.deJson(Map<String, dynamic> json) {
    List<dynamic> itens = json['notificacoes'] as List<dynamic>? ?? <dynamic>[];

    return ListaDeNotificacoes(
      quantidadeNaoLida: json['quantidadeNaoLida'] as int? ?? 0,
      notificacoes: itens
          .map(
            (dynamic item) => NotificacaoDoUsuario.deJson(
              Map<String, dynamic>.from(item as Map<dynamic, dynamic>),
            ),
          )
          .toList(),
    );
  }

  final int quantidadeNaoLida;
  final List<NotificacaoDoUsuario> notificacoes;
}
