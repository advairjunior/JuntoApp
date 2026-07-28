import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/instalacao/contrato_do_servico_de_instalacao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/instalacao/servico_de_instalacao_stub.dart'
    if (dart.library.js_interop) 'package:projeto_encontros_aplicativo_web/compartilhado/instalacao/servico_de_instalacao_web.dart'
    as implementacao;

final Provider<IServicoDeInstalacao> provedorDoServicoDeInstalacao =
    Provider<IServicoDeInstalacao>((Ref referencia) {
  return implementacao.crieServicoDeInstalacao();
});
