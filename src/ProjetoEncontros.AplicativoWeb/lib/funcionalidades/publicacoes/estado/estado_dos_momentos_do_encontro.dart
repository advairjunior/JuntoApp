import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/encontro_detalhado.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/publicacoes/modelos/publicacao_do_encontro.dart';

enum SituacaoDosMomentosDoEncontro { carregando, carregado, falhou }

class EstadoDosMomentosDoEncontro {
  const EstadoDosMomentosDoEncontro({
    required this.situacao,
    this.encontro,
    this.publicacoes = const <PublicacaoDoEncontro>[],
    this.estaPublicando = false,
    this.estaAtualizandoPresenca = false,
    this.mensagemDeErro,
  });

  const EstadoDosMomentosDoEncontro.carregando()
      : this(situacao: SituacaoDosMomentosDoEncontro.carregando);

  final SituacaoDosMomentosDoEncontro situacao;
  final EncontroDetalhado? encontro;
  final List<PublicacaoDoEncontro> publicacoes;
  final bool estaPublicando;
  final bool estaAtualizandoPresenca;
  final String? mensagemDeErro;
}
