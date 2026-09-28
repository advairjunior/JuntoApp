import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cartao_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estado_vazio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/titulo_de_secao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/configuracao/configuracao_do_ambiente.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/foto_de_perfil.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/imagem_privada.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/midias/video_privado.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/modelos/memoria_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/modelos/midia_da_memoria.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/telas/tela_de_midias_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/pessoas_frequentes/componentes/perfil_resumido_da_pessoa.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/pessoas_frequentes/dados/repositorio_de_pessoas_frequentes.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/pessoas_frequentes/modelos/historico_com_pessoa.dart';

class TelaDeHistoricoComPessoa extends ConsumerStatefulWidget {
  const TelaDeHistoricoComPessoa({
    required this.identificadorDaPessoa,
    super.key,
  });

  final String identificadorDaPessoa;

  @override
  ConsumerState<TelaDeHistoricoComPessoa> createState() =>
      _EstadoDaTelaDeHistoricoComPessoa();
}

class _EstadoDaTelaDeHistoricoComPessoa
    extends ConsumerState<TelaDeHistoricoComPessoa> {
  HistoricoComPessoa? _historico;
  String? _mensagemDeErro;
  bool _estaCarregando = true;
  bool _estaCarregandoMais = false;
  bool _estaCarregandoTodasMemorias = false;
  bool _mostreTodosProximos = false;

  @override
  void initState() {
    super.initState();
    Future<void>.microtask(_carregueAsync);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: CoresDoAplicativo.fundoExterno,
      body: Center(
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 1000),
          child: ColoredBox(
            color: CoresDoAplicativo.fundoPrincipal,
            child: SafeArea(
              child: Column(
                children: <Widget>[
                  _BarraSuperior(aoVoltar: () => context.pop()),
                  Expanded(child: _construaConteudo()),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _construaConteudo() {
    if (_estaCarregando) {
      return const Center(child: CircularProgressIndicator());
    }

    if (_historico == null) {
      return Center(
        child: EstadoVazio(
          icone: Icons.people_outline_rounded,
          titulo: 'Histórico indisponível',
          descricao: _mensagemDeErro ??
              'Não encontramos encontros em comum acessíveis.',
          acao: TextButton(
            onPressed: _carregueAsync,
            child: const Text('Tentar novamente'),
          ),
        ),
      );
    }

    HistoricoComPessoa historico = _historico!;

    return RefreshIndicator(
      onRefresh: _carregueAsync,
      child: ListView(
        key: const Key('historico-com-pessoa'),
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.fromLTRB(
          EspacamentosDoAplicativo.padrao,
          EspacamentosDoAplicativo.pequeno,
          EspacamentosDoAplicativo.padrao,
          EspacamentosDoAplicativo.extraGrande,
        ),
        children: <Widget>[
          _CabecalhoDaPessoa(
            historico: historico,
            aoAmpliarFoto: () => mostreFotoDaPessoaAmpliadaAsync(
              context,
              _convertaPessoa(historico),
            ),
            aoConvidar: () => _convideAsync(historico),
          ),
          const SizedBox(height: EspacamentosDoAplicativo.extraGrande),
          _SecaoVocesDois(historico: historico),
          if (historico.proximosEncontros.isNotEmpty) ...<Widget>[
            const SizedBox(height: EspacamentosDoAplicativo.extraGrande),
            _SecaoDeProximosEncontros(
              encontros: historico.proximosEncontros,
              mostreTodos: _mostreTodosProximos,
              aoAlternar: () {
                setState(() {
                  _mostreTodosProximos = !_mostreTodosProximos;
                });
              },
              aoAbrir: _abraEncontro,
            ),
          ],
          if (_temEstatisticas(historico.estatisticas)) ...<Widget>[
            const SizedBox(height: EspacamentosDoAplicativo.extraGrande),
            _SecaoSobreVoces(estatisticas: historico.estatisticas),
          ],
          const SizedBox(height: EspacamentosDoAplicativo.extraGrande),
          _SecaoDoHistorico(
            historico: historico.historico,
            estaCarregandoMais: _estaCarregandoMais,
            aoAbrir: _abraEncontro,
            aoCarregarMais: _carregueMaisAsync,
          ),
          const SizedBox(height: EspacamentosDoAplicativo.extraGrande),
          _SecaoDeMemorias(
            nomeDaPessoa: historico.nome,
            memorias: historico.memorias.take(6).toList(),
            temMais: historico.temMaisMemorias,
            estaCarregandoTodas: _estaCarregandoTodasMemorias,
            aoVerTodas: _carregueTodasMemoriasAsync,
            aoAbrir: (MemoriaDoEncontro memoria) =>
                mostrePublicacaoDaMemoriaAsync(
              context,
              memoria,
              identificadorDaPessoaMarcada: historico.identificadorDaPessoa,
            ),
          ),
          const SizedBox(height: EspacamentosDoAplicativo.extraGrande),
          SizedBox(
            width: double.infinity,
            child: FilledButton.icon(
              key: const Key('convidar-pessoa-do-historico'),
              onPressed: () => _convideAsync(historico),
              icon: const Icon(Icons.person_add_alt_1_outlined),
              label: const Text('Convidar para outro encontro'),
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _carregueAsync() async {
    if (mounted) {
      setState(() {
        _estaCarregando = true;
        _mensagemDeErro = null;
      });
    }

    try {
      HistoricoComPessoa historico = await ref
          .read(provedorDoRepositorioDePessoasFrequentes)
          .obtenhaHistoricoAsync(
            identificadorDaPessoa: widget.identificadorDaPessoa,
          );

      if (mounted) {
        setState(() {
          _historico = historico;
          _estaCarregando = false;
          _mostreTodosProximos = false;
        });
      }
    } on ExcecaoDaApi catch (excecao) {
      _registreErro(excecao.mensagem);
    } catch (_) {
      _registreErro('Não foi possível carregar este histórico.');
    }
  }

  Future<void> _carregueMaisAsync() async {
    HistoricoComPessoa? atual = _historico;

    if (atual == null ||
        !atual.historico.temProximaPagina ||
        _estaCarregandoMais) {
      return;
    }

    setState(() {
      _estaCarregandoMais = true;
    });

    try {
      HistoricoComPessoa pagina = await ref
          .read(provedorDoRepositorioDePessoasFrequentes)
          .obtenhaHistoricoAsync(
            identificadorDaPessoa: widget.identificadorDaPessoa,
            pagina: atual.historico.pagina + 1,
            tamanho: atual.historico.tamanho,
          );

      if (mounted) {
        setState(() {
          _historico = atual.acrescenteHistorico(pagina);
          _estaCarregandoMais = false;
        });
      }
    } on ExcecaoDaApi catch (excecao) {
      _mostreErroTemporario(excecao.mensagem);
    } catch (_) {
      _mostreErroTemporario('Não foi possível carregar mais encontros.');
    }
  }

  Future<void> _carregueTodasMemoriasAsync() async {
    HistoricoComPessoa? atual = _historico;

    if (atual == null ||
        !atual.temMaisMemorias ||
        _estaCarregandoTodasMemorias) {
      return;
    }

    setState(() {
      _estaCarregandoTodasMemorias = true;
    });

    try {
      HistoricoComPessoa resultado = await ref
          .read(provedorDoRepositorioDePessoasFrequentes)
          .obtenhaHistoricoAsync(
            identificadorDaPessoa: widget.identificadorDaPessoa,
            pagina: atual.historico.pagina,
            tamanho: atual.historico.tamanho,
            limiteDeMemorias: 50,
          );

      if (mounted) {
        setState(() {
          _estaCarregandoTodasMemorias = false;
        });
        await _mostreTodasMemoriasAsync(resultado);
      }
    } on ExcecaoDaApi catch (excecao) {
      _finalizeCarregamentoDasMemoriasComErro(excecao.mensagem);
    } catch (_) {
      _finalizeCarregamentoDasMemoriasComErro(
        'Não foi possível carregar todas as memórias.',
      );
    }
  }

  Future<void> _mostreTodasMemoriasAsync(
    HistoricoComPessoa historico,
  ) {
    return showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      useSafeArea: true,
      builder: (BuildContext contextoDaFolha) {
        return FractionallySizedBox(
          heightFactor: 0.9,
          child: Column(
            children: <Widget>[
              Padding(
                padding: const EdgeInsets.fromLTRB(
                  EspacamentosDoAplicativo.padrao,
                  EspacamentosDoAplicativo.padrao,
                  EspacamentosDoAplicativo.pequeno,
                  EspacamentosDoAplicativo.pequeno,
                ),
                child: Row(
                  children: <Widget>[
                    Expanded(
                      child: Text(
                        'Memórias com ${historico.nome}',
                        style: Theme.of(contextoDaFolha).textTheme.titleLarge,
                      ),
                    ),
                    IconButton(
                      tooltip: 'Fechar',
                      onPressed: () => Navigator.of(contextoDaFolha).pop(),
                      icon: const Icon(Icons.close_rounded),
                    ),
                  ],
                ),
              ),
              Expanded(
                child: GridView.builder(
                  padding: const EdgeInsets.all(
                    EspacamentosDoAplicativo.padrao,
                  ),
                  itemCount: historico.memorias.length,
                  gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                    crossAxisCount:
                        MediaQuery.sizeOf(contextoDaFolha).width >= 700 ? 6 : 3,
                    crossAxisSpacing: 3,
                    mainAxisSpacing: 3,
                  ),
                  itemBuilder: (BuildContext context, int indice) {
                    MemoriaDoEncontro memoria = historico.memorias[indice];
                    return _MiniaturaDaMemoria(
                      memoria: memoria,
                      aoAbrir: () => mostrePublicacaoDaMemoriaAsync(
                        contextoDaFolha,
                        memoria,
                        identificadorDaPessoaMarcada:
                            historico.identificadorDaPessoa,
                      ),
                    );
                  },
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  void _finalizeCarregamentoDasMemoriasComErro(String mensagem) {
    if (!mounted) {
      return;
    }

    setState(() {
      _estaCarregandoTodasMemorias = false;
    });
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(mensagem)),
    );
  }

  void _registreErro(String mensagem) {
    if (mounted) {
      setState(() {
        _historico = null;
        _mensagemDeErro = mensagem;
        _estaCarregando = false;
      });
    }
  }

  void _mostreErroTemporario(String mensagem) {
    if (!mounted) {
      return;
    }

    setState(() {
      _estaCarregandoMais = false;
    });
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(mensagem)),
    );
  }

  Future<void> _convideAsync(HistoricoComPessoa historico) {
    return mostreConviteParaOutroEncontroAsync(
      context: context,
      pessoa: _convertaPessoa(historico),
    );
  }

  PessoaDoEncontro _convertaPessoa(HistoricoComPessoa historico) {
    return PessoaDoEncontro(
      identificadorDoUsuario: historico.identificadorDaPessoa,
      nome: historico.nome,
      urlDaFotoDePerfil: historico.urlDaFotoDePerfil,
    );
  }

  void _abraEncontro(String identificadorDoEncontro) {
    context.push<void>('/encontros/$identificadorDoEncontro');
  }

  bool _temEstatisticas(EstatisticasComPessoa estatisticas) {
    return estatisticas.quantidadeDeEncontrosRealizadosJuntos > 0 ||
        estatisticas.quantidadeDeEncontrosJuntosNesteAno > 0 ||
        estatisticas.mediaDeDiasEntreEncontros != null ||
        estatisticas.maiorIntervaloEmDias != null ||
        estatisticas.tipoMaisFrequente != null ||
        estatisticas.diaDaSemanaMaisFrequente != null ||
        estatisticas.localMaisFrequente != null;
  }
}

class _BarraSuperior extends StatelessWidget {
  const _BarraSuperior({required this.aoVoltar});

  final VoidCallback aoVoltar;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(
        horizontal: EspacamentosDoAplicativo.pequeno,
        vertical: EspacamentosDoAplicativo.minimo,
      ),
      child: Row(
        children: <Widget>[
          IconButton(
            tooltip: 'Voltar',
            onPressed: aoVoltar,
            icon: const Icon(Icons.arrow_back_ios_new_rounded),
          ),
          const SizedBox(width: EspacamentosDoAplicativo.pequeno),
          Text(
            'Pessoa',
            style: Theme.of(context).textTheme.titleLarge,
          ),
        ],
      ),
    );
  }
}

class _CabecalhoDaPessoa extends StatelessWidget {
  const _CabecalhoDaPessoa({
    required this.historico,
    required this.aoAmpliarFoto,
    required this.aoConvidar,
  });

  final HistoricoComPessoa historico;
  final VoidCallback aoAmpliarFoto;
  final VoidCallback aoConvidar;

  @override
  Widget build(BuildContext context) {
    return CartaoDoAplicativo(
      elevado: true,
      filho: LayoutBuilder(
        builder: (BuildContext context, BoxConstraints limites) {
          bool ehCompacto = limites.maxWidth < 620;

          if (ehCompacto) {
            return Column(
              children: <Widget>[
                _IdentidadeDaPessoa(
                  historico: historico,
                  aoAmpliarFoto: aoAmpliarFoto,
                ),
                const SizedBox(height: EspacamentosDoAplicativo.grande),
                SizedBox(
                  width: double.infinity,
                  child: FilledButton.icon(
                    onPressed: aoConvidar,
                    icon: const Icon(Icons.person_add_alt_1_outlined),
                    label: const Text('Convidar para outro encontro'),
                  ),
                ),
              ],
            );
          }

          return Row(
            children: <Widget>[
              Expanded(
                child: _IdentidadeDaPessoa(
                  historico: historico,
                  aoAmpliarFoto: aoAmpliarFoto,
                ),
              ),
              const SizedBox(width: EspacamentosDoAplicativo.grande),
              SizedBox(
                width: 236,
                child: FilledButton.icon(
                  onPressed: aoConvidar,
                  icon: const Icon(Icons.person_add_alt_1_outlined),
                  label: const Text('Convidar para outro encontro'),
                ),
              ),
            ],
          );
        },
      ),
    );
  }
}

class _IdentidadeDaPessoa extends StatelessWidget {
  const _IdentidadeDaPessoa({
    required this.historico,
    required this.aoAmpliarFoto,
  });

  final HistoricoComPessoa historico;
  final VoidCallback aoAmpliarFoto;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: <Widget>[
        Semantics(
          button: historico.urlDaFotoDePerfil?.isNotEmpty ?? false,
          label: 'Ampliar foto de ${historico.nome}',
          child: InkResponse(
            key: const Key('ampliar-foto-da-pessoa'),
            onTap: historico.urlDaFotoDePerfil?.isNotEmpty ?? false
                ? aoAmpliarFoto
                : null,
            radius: 48,
            child: FotoDePerfil(
              url: historico.urlDaFotoDePerfil,
              iniciais: historico.iniciais,
              dimensao: 82,
              tamanhoDasIniciais: 28,
            ),
          ),
        ),
        const SizedBox(width: EspacamentosDoAplicativo.medio),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: <Widget>[
              Text(
                historico.nome,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
                style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                      fontWeight: FontWeight.w700,
                    ),
              ),
              const SizedBox(height: EspacamentosDoAplicativo.minimo),
              Text(
                historico.quantidadeDeEncontrosEmComum == 1
                    ? '1 encontro em comum registrado'
                    : '${historico.quantidadeDeEncontrosEmComum} '
                        'encontros em comum registrados',
                style: const TextStyle(
                  color: CoresDoAplicativo.textoSecundario,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}

class _SecaoVocesDois extends StatelessWidget {
  const _SecaoVocesDois({required this.historico});

  final HistoricoComPessoa historico;

  @override
  Widget build(BuildContext context) {
    List<_IndicadorDaRelacao> indicadores = <_IndicadorDaRelacao>[
      if (historico.quantidadeDeEncontrosRealizadosJuntos > 0)
        _IndicadorDaRelacao(
          icone: Icons.event_available_outlined,
          valor: '${historico.quantidadeDeEncontrosRealizadosJuntos}',
          rotulo: historico.quantidadeDeEncontrosRealizadosJuntos == 1
              ? 'realizado em comum'
              : 'realizados em comum',
        ),
      if (historico.ultimoEncontroEm != null)
        _IndicadorDaRelacao(
          icone: Icons.history_rounded,
          valor: _formateDataCurta(historico.ultimoEncontroEm!),
          rotulo: 'último registrado',
        ),
      if (historico.proximoEncontroEm != null)
        _IndicadorDaRelacao(
          icone: Icons.upcoming_outlined,
          valor: _formateDataCurta(historico.proximoEncontroEm!),
          rotulo: 'próximo confirmado',
        ),
      if (historico.primeiroEncontroEm != null)
        _IndicadorDaRelacao(
          icone: Icons.flag_outlined,
          valor: _formateDataCurta(historico.primeiroEncontroEm!),
          rotulo: 'primeiro registrado',
        ),
    ];

    if (indicadores.isEmpty) {
      return const SizedBox.shrink();
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: <Widget>[
        const TituloDeSecao(titulo: 'Vocês dois'),
        const SizedBox(height: EspacamentosDoAplicativo.medio),
        LayoutBuilder(
          builder: (BuildContext context, BoxConstraints limites) {
            int colunas = limites.maxWidth >= 760 ? 4 : 2;
            double largura =
                (limites.maxWidth - ((colunas - 1) * 10)) / colunas;

            return Wrap(
              spacing: 10,
              runSpacing: 10,
              children: indicadores
                  .map(
                    (_IndicadorDaRelacao indicador) => SizedBox(
                      width: largura,
                      child: indicador,
                    ),
                  )
                  .toList(),
            );
          },
        ),
      ],
    );
  }
}

class _IndicadorDaRelacao extends StatelessWidget {
  const _IndicadorDaRelacao({
    required this.icone,
    required this.valor,
    required this.rotulo,
  });

  final IconData icone;
  final String valor;
  final String rotulo;

  @override
  Widget build(BuildContext context) {
    return CartaoDoAplicativo(
      preenchimento: const EdgeInsets.all(EspacamentosDoAplicativo.medio),
      filho: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: <Widget>[
          Icon(icone, size: 20, color: CoresDoAplicativo.azulInteracao),
          const SizedBox(height: EspacamentosDoAplicativo.pequeno),
          Text(
            valor,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: Theme.of(context).textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.w700,
                ),
          ),
          const SizedBox(height: 2),
          Text(
            rotulo,
            maxLines: 2,
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(
              color: CoresDoAplicativo.textoTerciario,
              fontSize: 12,
            ),
          ),
        ],
      ),
    );
  }
}

class _SecaoDeProximosEncontros extends StatelessWidget {
  const _SecaoDeProximosEncontros({
    required this.encontros,
    required this.mostreTodos,
    required this.aoAlternar,
    required this.aoAbrir,
  });

  final List<ProximoEncontroComPessoa> encontros;
  final bool mostreTodos;
  final VoidCallback aoAlternar;
  final ValueChanged<String> aoAbrir;

  @override
  Widget build(BuildContext context) {
    List<ProximoEncontroComPessoa> visiveis =
        mostreTodos ? encontros : encontros.take(3).toList();

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: <Widget>[
        TituloDeSecao(
          titulo: 'Próximos encontros',
          acao: encontros.length > 3
              ? TextButton(
                  onPressed: aoAlternar,
                  child: Text(mostreTodos ? 'Mostrar menos' : 'Ver todos'),
                )
              : null,
        ),
        const SizedBox(height: EspacamentosDoAplicativo.medio),
        _GradeDeEncontros(
          quantidade: visiveis.length,
          construa: (int indice) {
            ProximoEncontroComPessoa encontro = visiveis[indice];
            return _CartaoDeEncontro(
              titulo: encontro.titulo,
              inicioEm: encontro.inicioEm,
              local: encontro.local,
              tipo: encontro.tipo,
              descricao: encontro.descricao,
              urlDaImagemDeCapa: encontro.urlDaImagemDeCapa,
              resumoDaPresenca:
                  encontro.situacaoDoUsuarioAtual == 'Confirmado' &&
                          encontro.situacaoDaPessoa == 'Confirmado'
                      ? 'Vocês dois confirmaram presença'
                      : null,
              aoTocar: () => aoAbrir(encontro.identificadorDoEncontro),
            );
          },
        ),
      ],
    );
  }
}

class _SecaoSobreVoces extends StatelessWidget {
  const _SecaoSobreVoces({required this.estatisticas});

  final EstatisticasComPessoa estatisticas;

  @override
  Widget build(BuildContext context) {
    List<String> textos = <String>[
      if (estatisticas.quantidadeDeEncontrosJuntosNesteAno > 0)
        estatisticas.quantidadeDeEncontrosJuntosNesteAno == 1
            ? 'Vocês têm 1 encontro realizado em comum neste ano.'
            : 'Vocês têm ${estatisticas.quantidadeDeEncontrosJuntosNesteAno} '
                'encontros realizados em comum neste ano.',
      if (estatisticas.mediaDeDiasEntreEncontros != null)
        'Os encontros registrados costumam acontecer a cada '
            '${estatisticas.mediaDeDiasEntreEncontros!.round()} dias.',
      if (estatisticas.maiorIntervaloEmDias != null)
        'O maior intervalo registrado foi de '
            '${estatisticas.maiorIntervaloEmDias} dias.',
      if (estatisticas.tipoMaisFrequente != null)
        'O tipo mais comum é ${estatisticas.tipoMaisFrequente}.',
      if (estatisticas.diaDaSemanaMaisFrequente != null)
        '${estatisticas.diaDaSemanaMaisFrequente} é o dia mais frequente.',
      if (estatisticas.localMaisFrequente != null)
        'O local mais frequente é ${estatisticas.localMaisFrequente}.',
    ];

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: <Widget>[
        const TituloDeSecao(titulo: 'Sobre vocês'),
        const SizedBox(height: EspacamentosDoAplicativo.medio),
        CartaoDoAplicativo(
          filho: Column(
            children: textos
                .map(
                  (String texto) => Padding(
                    padding: const EdgeInsets.symmetric(
                      vertical: EspacamentosDoAplicativo.pequeno,
                    ),
                    child: Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: <Widget>[
                        const Icon(
                          Icons.auto_awesome_outlined,
                          size: 18,
                          color: CoresDoAplicativo.azulInteracao,
                        ),
                        const SizedBox(
                          width: EspacamentosDoAplicativo.pequeno,
                        ),
                        Expanded(child: Text(texto)),
                      ],
                    ),
                  ),
                )
                .toList(),
          ),
        ),
      ],
    );
  }
}

class _SecaoDoHistorico extends StatelessWidget {
  const _SecaoDoHistorico({
    required this.historico,
    required this.estaCarregandoMais,
    required this.aoAbrir,
    required this.aoCarregarMais,
  });

  final PaginaDoHistoricoComPessoa historico;
  final bool estaCarregandoMais;
  final ValueChanged<String> aoAbrir;
  final VoidCallback aoCarregarMais;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: <Widget>[
        const TituloDeSecao(titulo: 'Histórico juntos'),
        const SizedBox(height: EspacamentosDoAplicativo.medio),
        if (historico.itens.isEmpty)
          const CartaoDoAplicativo(
            filho: Text(
              'Ainda não há encontros realizados com presença confirmada '
              'pelos dois.',
              style: TextStyle(color: CoresDoAplicativo.textoSecundario),
            ),
          )
        else
          _GradeDeEncontros(
            quantidade: historico.itens.length,
            construa: (int indice) {
              EncontroDoHistoricoComPessoa encontro = historico.itens[indice];
              return _CartaoDeEncontro(
                titulo: encontro.titulo,
                inicioEm: encontro.inicioEm,
                local: encontro.local,
                tipo: encontro.tipo,
                urlDaImagemDeCapa: encontro.urlDaImagemDeCapa,
                aoTocar: () => aoAbrir(encontro.identificadorDoEncontro),
              );
            },
          ),
        if (historico.temProximaPagina) ...<Widget>[
          const SizedBox(height: EspacamentosDoAplicativo.medio),
          Center(
            child: TextButton.icon(
              key: const Key('carregar-mais-historico'),
              onPressed: estaCarregandoMais ? null : aoCarregarMais,
              icon: estaCarregandoMais
                  ? const SizedBox.square(
                      dimension: 16,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Icon(Icons.expand_more_rounded),
              label: const Text('Carregar mais'),
            ),
          ),
        ],
      ],
    );
  }
}

class _GradeDeEncontros extends StatelessWidget {
  const _GradeDeEncontros({
    required this.quantidade,
    required this.construa,
  });

  final int quantidade;
  final Widget Function(int indice) construa;

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (BuildContext context, BoxConstraints limites) {
        bool useDuasColunas = limites.maxWidth >= 720;
        double largura =
            useDuasColunas ? (limites.maxWidth - 12) / 2 : limites.maxWidth;

        return Wrap(
          spacing: 12,
          runSpacing: 12,
          children: List<Widget>.generate(
            quantidade,
            (int indice) => SizedBox(
              width: largura,
              child: construa(indice),
            ),
          ),
        );
      },
    );
  }
}

class _CartaoDeEncontro extends StatelessWidget {
  const _CartaoDeEncontro({
    required this.titulo,
    required this.inicioEm,
    required this.aoTocar,
    this.local,
    this.tipo,
    this.descricao,
    this.urlDaImagemDeCapa,
    this.resumoDaPresenca,
  });

  final String titulo;
  final DateTime inicioEm;
  final String? local;
  final String? tipo;
  final String? descricao;
  final String? urlDaImagemDeCapa;
  final String? resumoDaPresenca;
  final VoidCallback aoTocar;

  @override
  Widget build(BuildContext context) {
    return CartaoDoAplicativo(
      aoTocar: aoTocar,
      preenchimento: EdgeInsets.zero,
      filho: Row(
        children: <Widget>[
          SizedBox(
            width: 112,
            height: 132,
            child: _ImagemDoEncontro(
              url: urlDaImagemDeCapa,
              tipo: tipo,
            ),
          ),
          Expanded(
            child: Padding(
              padding: const EdgeInsets.all(
                EspacamentosDoAplicativo.medio,
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: <Widget>[
                  Text(
                    titulo,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: Theme.of(context).textTheme.titleMedium?.copyWith(
                          fontWeight: FontWeight.w700,
                        ),
                  ),
                  const SizedBox(height: EspacamentosDoAplicativo.minimo),
                  Text(
                    DateFormat(
                      "dd 'de' MMM 'de' yyyy · HH:mm",
                      'pt_BR',
                    ).format(inicioEm),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      color: CoresDoAplicativo.textoSecundario,
                      fontSize: 12,
                    ),
                  ),
                  if (local?.trim().isNotEmpty ?? false) ...<Widget>[
                    const SizedBox(height: 3),
                    Text(
                      local!,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(
                        color: CoresDoAplicativo.textoTerciario,
                        fontSize: 12,
                      ),
                    ),
                  ],
                  if (descricao?.trim().isNotEmpty ?? false) ...<Widget>[
                    const SizedBox(height: EspacamentosDoAplicativo.pequeno),
                    Text(
                      descricao!,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(fontSize: 12),
                    ),
                  ],
                  if (resumoDaPresenca != null) ...<Widget>[
                    const SizedBox(height: EspacamentosDoAplicativo.pequeno),
                    Text(
                      resumoDaPresenca!,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(
                        color: CoresDoAplicativo.verdeDestaque,
                        fontSize: 12,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ],
                ],
              ),
            ),
          ),
          const Padding(
            padding: EdgeInsets.only(right: EspacamentosDoAplicativo.pequeno),
            child: Icon(
              Icons.chevron_right_rounded,
              color: CoresDoAplicativo.textoTerciario,
            ),
          ),
        ],
      ),
    );
  }
}

class _ImagemDoEncontro extends StatelessWidget {
  const _ImagemDoEncontro({required this.url, required this.tipo});

  final String? url;
  final String? tipo;

  @override
  Widget build(BuildContext context) {
    String recurso = ConfiguracaoDoAmbiente.crieUrlAbsoluta(url);

    if (recurso.isNotEmpty) {
      return ImagemPrivada(
        recurso: recurso,
        ajuste: BoxFit.cover,
        construaSubstituta: (_) => _substituta(),
      );
    }

    return _substituta();
  }

  Widget _substituta() {
    return ColoredBox(
      color: CoresDoAplicativo.fundoDaInteracao,
      child: Center(
        child: Icon(
          tipo?.toLowerCase() == 'aniversário'
              ? Icons.cake_outlined
              : Icons.event_outlined,
          color: CoresDoAplicativo.azulInteracao,
          size: 32,
        ),
      ),
    );
  }
}

class _SecaoDeMemorias extends StatelessWidget {
  const _SecaoDeMemorias({
    required this.nomeDaPessoa,
    required this.memorias,
    required this.temMais,
    required this.estaCarregandoTodas,
    required this.aoVerTodas,
    required this.aoAbrir,
  });

  final String nomeDaPessoa;
  final List<MemoriaDoEncontro> memorias;
  final bool temMais;
  final bool estaCarregandoTodas;
  final VoidCallback aoVerTodas;
  final ValueChanged<MemoriaDoEncontro> aoAbrir;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: <Widget>[
        TituloDeSecao(
          titulo: 'Memórias com $nomeDaPessoa',
          acao: temMais
              ? TextButton(
                  key: const Key('ver-todas-memorias-em-comum'),
                  onPressed: estaCarregandoTodas ? null : aoVerTodas,
                  child: estaCarregandoTodas
                      ? const SizedBox.square(
                          dimension: 16,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : const Text('Ver todas'),
                )
              : null,
        ),
        const SizedBox(height: EspacamentosDoAplicativo.medio),
        if (memorias.isEmpty)
          const EstadoVazio(
            icone: Icons.photo_library_outlined,
            titulo: 'Nenhuma memória juntos ainda',
            descricao:
                'Fotos e vídeos em que esta pessoa for marcada aparecerão aqui.',
          )
        else
          LayoutBuilder(
            builder: (BuildContext context, BoxConstraints limites) {
              int colunas = limites.maxWidth >= 700 ? 6 : 3;

              return GridView.builder(
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                itemCount: memorias.length,
                gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                  crossAxisCount: colunas,
                  crossAxisSpacing: 3,
                  mainAxisSpacing: 3,
                ),
                itemBuilder: (BuildContext context, int indice) {
                  MemoriaDoEncontro memoria = memorias[indice];
                  return _MiniaturaDaMemoria(
                    memoria: memoria,
                    aoAbrir: () => aoAbrir(memoria),
                  );
                },
              );
            },
          ),
      ],
    );
  }
}

class _MiniaturaDaMemoria extends StatelessWidget {
  const _MiniaturaDaMemoria({
    required this.memoria,
    required this.aoAbrir,
  });

  final MemoriaDoEncontro memoria;
  final VoidCallback aoAbrir;

  @override
  Widget build(BuildContext context) {
    MidiaDaMemoria midia = memoria.midias.first;
    String recurso = ConfiguracaoDoAmbiente.crieUrlAbsoluta(midia.url);
    bool ehVideo = midia.tipoDeConteudo.toLowerCase().startsWith('video/');

    return Material(
      color: CoresDoAplicativo.fundoDoCartao,
      child: InkWell(
        key: Key('memoria-comum-${memoria.identificador}'),
        onTap: aoAbrir,
        child: Stack(
          fit: StackFit.expand,
          children: <Widget>[
            ehVideo
                ? VideoPrivado(
                    recurso: recurso,
                    tipoDeConteudo: midia.tipoDeConteudo,
                    exibaControles: false,
                  )
                : ImagemPrivada(
                    recurso: recurso,
                    ajuste: BoxFit.cover,
                    construaSubstituta: (_) => const ColoredBox(
                      color: CoresDoAplicativo.fundoDoCartaoSuave,
                      child: Icon(Icons.broken_image_outlined),
                    ),
                  ),
            if (ehVideo)
              const Positioned(
                top: 6,
                right: 6,
                child: Icon(
                  Icons.play_circle_fill_rounded,
                  color: Colors.white,
                  size: 20,
                ),
              ),
            if (memoria.midias.length > 1)
              const Positioned(
                top: 6,
                left: 6,
                child: Icon(
                  Icons.collections_rounded,
                  color: Colors.white,
                  size: 19,
                ),
              ),
          ],
        ),
      ),
    );
  }
}

String _formateDataCurta(DateTime data) {
  return DateFormat('dd MMM yyyy', 'pt_BR').format(data);
}
