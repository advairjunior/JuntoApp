enum SituacaoDaInstalacao {
  instalada,
  podeSolicitar,
  requerOrientacaoNoIos,
  requerOrientacaoGenerica,
}

abstract interface class IServicoDeInstalacao {
  SituacaoDaInstalacao obtenhaSituacao();

  Future<bool> soliciteInstalacaoAsync();
}
