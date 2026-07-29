import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/encontro_detalhado.dart';

enum SituacaoDoDetalheDoEncontro { carregando, carregado, falhou }

class EstadoDoDetalheDoEncontro {
  const EstadoDoDetalheDoEncontro({
    required this.situacao,
    this.encontro,
    this.mensagemDeErro,
    this.mensagemDeSucesso,
    this.estaAtualizandoPresenca = false,
    this.estaExecutandoAcaoDoOrganizador = false,
  });

  const EstadoDoDetalheDoEncontro.carregando()
      : this(situacao: SituacaoDoDetalheDoEncontro.carregando);

  final SituacaoDoDetalheDoEncontro situacao;
  final EncontroDetalhado? encontro;
  final String? mensagemDeErro;
  final String? mensagemDeSucesso;
  final bool estaAtualizandoPresenca;
  final bool estaExecutandoAcaoDoOrganizador;
}
