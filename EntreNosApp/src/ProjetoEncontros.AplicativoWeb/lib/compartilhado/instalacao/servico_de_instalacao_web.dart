import 'dart:js_interop';

import 'package:projeto_encontros_aplicativo_web/compartilhado/instalacao/contrato_do_servico_de_instalacao.dart';

@JS('juntoEstaInstalado')
external bool _estaInstalado();

@JS('juntoPodeInstalar')
external bool _podeInstalar();

@JS('juntoEhIos')
external bool _ehIos();

@JS('juntoSoliciteInstalacao')
external JSPromise<JSBoolean> _soliciteInstalacao();

IServicoDeInstalacao crieServicoDeInstalacao() {
  return ServicoDeInstalacaoWeb();
}

class ServicoDeInstalacaoWeb implements IServicoDeInstalacao {
  @override
  SituacaoDaInstalacao obtenhaSituacao() {
    if (_estaInstalado()) {
      return SituacaoDaInstalacao.instalada;
    }

    if (_podeInstalar()) {
      return SituacaoDaInstalacao.podeSolicitar;
    }

    if (_ehIos()) {
      return SituacaoDaInstalacao.requerOrientacaoNoIos;
    }

    return SituacaoDaInstalacao.requerOrientacaoGenerica;
  }

  @override
  Future<bool> soliciteInstalacaoAsync() async {
    JSBoolean resultado = await _soliciteInstalacao().toDart;
    return resultado.toDart;
  }
}
