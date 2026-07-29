import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/encontro_resumo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/convite_do_encontro_resumo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/usuario_atual.dart';

enum SituacaoDaPaginaInicial { carregando, carregada, falhou }

class EstadoDaPaginaInicial {
  const EstadoDaPaginaInicial({
    required this.situacao,
    this.usuarioAtual,
    this.encontros = const <EncontroResumo>[],
    this.convitesPendentes = const <ConviteDoEncontroResumo>[],
    this.mensagemDeErro,
    this.identificadorDoConviteEmAtualizacao,
  });

  const EstadoDaPaginaInicial.carregando()
      : this(situacao: SituacaoDaPaginaInicial.carregando);

  final SituacaoDaPaginaInicial situacao;
  final UsuarioAtual? usuarioAtual;
  final List<EncontroResumo> encontros;
  final List<ConviteDoEncontroResumo> convitesPendentes;
  final String? mensagemDeErro;
  final String? identificadorDoConviteEmAtualizacao;
}
