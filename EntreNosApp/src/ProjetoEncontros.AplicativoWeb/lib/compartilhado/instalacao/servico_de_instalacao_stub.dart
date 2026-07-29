import 'package:projeto_encontros_aplicativo_web/compartilhado/instalacao/contrato_do_servico_de_instalacao.dart';

IServicoDeInstalacao crieServicoDeInstalacao() {
  return ServicoDeInstalacaoStub();
}

class ServicoDeInstalacaoStub implements IServicoDeInstalacao {
  @override
  SituacaoDaInstalacao obtenhaSituacao() {
    return SituacaoDaInstalacao.requerOrientacaoGenerica;
  }

  @override
  Future<bool> soliciteInstalacaoAsync() async {
    return false;
  }
}
