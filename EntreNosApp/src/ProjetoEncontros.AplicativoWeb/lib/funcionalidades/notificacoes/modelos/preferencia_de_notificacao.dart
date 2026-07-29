class PreferenciaDeNotificacao {
  const PreferenciaDeNotificacao({
    required this.notificacoesDeConviteAtivas,
    required this.lembretesDeEncontroAtivos,
    required this.notificacoesDeAlteracaoAtivas,
    required this.notificacoesDeCombinadosAtivas,
  });

  factory PreferenciaDeNotificacao.deJson(Map<String, dynamic> json) {
    return PreferenciaDeNotificacao(
      notificacoesDeConviteAtivas:
          json['notificacoesDeConviteAtivas'] as bool? ?? true,
      lembretesDeEncontroAtivos:
          json['lembretesDeEncontroAtivos'] as bool? ?? true,
      notificacoesDeAlteracaoAtivas:
          json['notificacoesDeAlteracaoAtivas'] as bool? ?? true,
      notificacoesDeCombinadosAtivas:
          json['notificacoesDeCombinadosAtivas'] as bool? ?? true,
    );
  }

  final bool notificacoesDeConviteAtivas;
  final bool lembretesDeEncontroAtivos;
  final bool notificacoesDeAlteracaoAtivas;
  final bool notificacoesDeCombinadosAtivas;

  Map<String, dynamic> paraJson() {
    return <String, dynamic>{
      'notificacoesDeConviteAtivas': notificacoesDeConviteAtivas,
      'lembretesDeEncontroAtivos': lembretesDeEncontroAtivos,
      'notificacoesDeAlteracaoAtivas': notificacoesDeAlteracaoAtivas,
      'notificacoesDeCombinadosAtivas': notificacoesDeCombinadosAtivas,
    };
  }
}
