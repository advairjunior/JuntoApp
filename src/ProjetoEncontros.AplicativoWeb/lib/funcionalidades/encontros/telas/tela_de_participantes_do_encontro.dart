import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cabecalho_da_pagina.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cartao_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estado_vazio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estrutura_responsiva_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/indicador_de_situacao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/foto_de_perfil.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/dados/repositorio_de_encontros.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/estado/controlador_do_detalhe_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/estado/estado_do_detalhe_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/encontro_detalhado.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/participante_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/pessoas_frequentes/dados/repositorio_de_pessoas_frequentes.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/pessoas_frequentes/modelos/pessoa_frequente.dart';

enum FiltroDeParticipantes {
  todos,
  confirmados,
  talvez,
  aguardando,
  naoVao,
}

class TelaDeParticipantesDoEncontro extends ConsumerStatefulWidget {
  const TelaDeParticipantesDoEncontro({
    required this.identificadorDoEncontro,
    super.key,
  });

  final String identificadorDoEncontro;

  @override
  ConsumerState<TelaDeParticipantesDoEncontro> createState() =>
      _EstadoDaTelaDeParticipantesDoEncontro();
}

class _EstadoDaTelaDeParticipantesDoEncontro
    extends ConsumerState<TelaDeParticipantesDoEncontro> {
  FiltroDeParticipantes _filtro = FiltroDeParticipantes.todos;

  @override
  Widget build(BuildContext context) {
    EstadoDoDetalheDoEncontro estado = ref.watch(
      provedorDoControladorDoDetalheDoEncontro(
        widget.identificadorDoEncontro,
      ),
    );

    return Scaffold(
      body: EstruturaResponsivaDoAplicativo(
        filho: SafeArea(
          child: switch (estado.situacao) {
            SituacaoDoDetalheDoEncontro.carregando =>
              const Center(child: CircularProgressIndicator()),
            SituacaoDoDetalheDoEncontro.falhou => _ErroDeParticipantes(
                mensagem: estado.mensagemDeErro ??
                    'Não foi possível carregar os participantes.',
                aoVoltar: _volte,
                aoTentarNovamente: () => ref
                    .read(
                      provedorDoControladorDoDetalheDoEncontro(
                        widget.identificadorDoEncontro,
                      ).notifier,
                    )
                    .carregueAsync(),
              ),
            SituacaoDoDetalheDoEncontro.carregado => _construaConteudo(estado),
          },
        ),
      ),
    );
  }

  Widget _construaConteudo(EstadoDoDetalheDoEncontro estado) {
    EncontroDetalhado encontro = estado.encontro!;
    List<ParticipanteDoEncontro> organizadores = encontro.participantes
        .where(
          (ParticipanteDoEncontro participante) =>
              _participanteEhOrganizador(participante) &&
              _participantePassaNoFiltro(participante),
        )
        .toList();
    List<ParticipanteDoEncontro> demaisParticipantes = encontro.participantes
        .where(
          (ParticipanteDoEncontro participante) =>
              !_participanteEhOrganizador(participante) &&
              participante.situacao.toLowerCase() != 'convidado' &&
              _participantePassaNoFiltro(participante),
        )
        .toList();
    List<ParticipanteDoEncontro> convidados = encontro.participantes
        .where(
          (ParticipanteDoEncontro participante) =>
              participante.situacao.toLowerCase() == 'convidado' &&
              _participantePassaNoFiltro(participante),
        )
        .toList();
    bool filtroEstaVazio = organizadores.isEmpty &&
        demaisParticipantes.isEmpty &&
        convidados.isEmpty;

    return Column(
      children: <Widget>[
        _CabecalhoDeParticipantes(
          tituloDoEncontro: encontro.titulo,
          quantidade: encontro.participantes.length,
          aoVoltar: _volte,
        ),
        _FiltrosDeParticipantes(
          filtro: _filtro,
          participantes: encontro.participantes,
          aoSelecionar: (FiltroDeParticipantes filtro) {
            setState(() {
              _filtro = filtro;
            });
          },
        ),
        if (estado.mensagemDeErro != null)
          _MensagemDaTela(
            mensagem: estado.mensagemDeErro!,
            cor: CoresDoAplicativo.coral,
          ),
        if (estado.mensagemDeSucesso != null)
          _MensagemDaTela(
            mensagem: estado.mensagemDeSucesso!,
            cor: CoresDoAplicativo.verdeDestaque,
          ),
        Expanded(
          child: RefreshIndicator(
            onRefresh: () => ref
                .read(
                  provedorDoControladorDoDetalheDoEncontro(
                    widget.identificadorDoEncontro,
                  ).notifier,
                )
                .carregueAsync(),
            child: ListView(
              physics: const AlwaysScrollableScrollPhysics(),
              padding: const EdgeInsets.fromLTRB(16, 12, 16, 96),
              children: <Widget>[
                if (organizadores.isNotEmpty)
                  _SecaoDeParticipantes(
                    titulo: 'Organização',
                    participantes: organizadores,
                  ),
                if (demaisParticipantes.isNotEmpty) ...<Widget>[
                  const SizedBox(height: EspacamentosDoAplicativo.grande),
                  _SecaoDeParticipantes(
                    titulo: 'Participantes',
                    participantes: demaisParticipantes,
                  ),
                ],
                if (convidados.isNotEmpty) ...<Widget>[
                  const SizedBox(height: EspacamentosDoAplicativo.grande),
                  _SecaoDeParticipantes(
                    titulo: 'Aguardando resposta',
                    participantes: convidados,
                  ),
                ],
                if (filtroEstaVazio)
                  _EstadoVazioDoFiltro(
                    filtroEhTodos: _filtro == FiltroDeParticipantes.todos,
                    aoVerTodos: () {
                      setState(() {
                        _filtro = FiltroDeParticipantes.todos;
                      });
                    },
                  ),
              ],
            ),
          ),
        ),
        if (encontro.podeEditar)
          _AcaoDeConvite(
            estaEnviando: estado.estaExecutandoAcaoDoOrganizador,
            aoConvidar: () => _abraConviteAsync(encontro),
          ),
      ],
    );
  }

  bool _participanteEhOrganizador(ParticipanteDoEncontro participante) {
    String papel = participante.papel.toLowerCase();
    return papel == 'dono' || papel == 'organizador';
  }

  bool _participantePassaNoFiltro(ParticipanteDoEncontro participante) {
    String situacao = participante.situacao.toLowerCase();

    return switch (_filtro) {
      FiltroDeParticipantes.todos => true,
      FiltroDeParticipantes.confirmados => situacao == 'confirmado',
      FiltroDeParticipantes.talvez => situacao == 'talvez',
      FiltroDeParticipantes.aguardando => situacao == 'convidado',
      FiltroDeParticipantes.naoVao => situacao == 'naovai',
    };
  }

  void _volte() {
    if (context.canPop()) {
      context.pop();
      return;
    }

    context.go('/encontros/${widget.identificadorDoEncontro}');
  }

  Future<void> _abraConviteAsync(EncontroDetalhado encontro) async {
    await showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      backgroundColor: CoresDoAplicativo.fundoDoCartao,
      showDragHandle: true,
      builder: (BuildContext context) {
        return Consumer(
          builder: (BuildContext context, WidgetRef referencia, Widget? filho) {
            AsyncValue<List<PessoaFrequente>> pessoas = referencia.watch(
              provedorDasPessoasFrequentes,
            );
            Set<String> participantes = encontro.participantes
                .map(
                  (ParticipanteDoEncontro participante) =>
                      participante.identificadorDoUsuario,
                )
                .toSet();
            List<PessoaFrequente> sugestoes =
                (pessoas.valueOrNull ?? <PessoaFrequente>[])
                    .where(
                      (PessoaFrequente pessoa) => !participantes.contains(
                        pessoa.identificadorDoUsuario,
                      ),
                    )
                    .take(5)
                    .toList();

            return _FormularioDeConvite(
              pessoasFrequentes: sugestoes,
              pessoasEstaoCarregando: pessoas.isLoading,
              aoConvidarPessoaFrequente: (PessoaFrequente pessoa) =>
                  _convidePessoaFrequenteAsync(pessoa),
              aoEnviar: (String email) async {
                bool enviou = await ref
                    .read(
                      provedorDoControladorDoDetalheDoEncontro(
                        widget.identificadorDoEncontro,
                      ).notifier,
                    )
                    .convidePessoaAsync(email);

                return (
                  enviou: enviou,
                  mensagem: ref
                      .read(
                        provedorDoControladorDoDetalheDoEncontro(
                          widget.identificadorDoEncontro,
                        ),
                      )
                      .mensagemDeErro,
                );
              },
            );
          },
        );
      },
    );
  }

  Future<ResultadoDoConvite> _convidePessoaFrequenteAsync(
    PessoaFrequente pessoa,
  ) async {
    try {
      await ref
          .read(provedorDoRepositorioDeEncontros)
          .convidePessoaFrequenteAsync(
            identificador: widget.identificadorDoEncontro,
            identificadorDoUsuario: pessoa.identificadorDoUsuario,
          );
      await ref
          .read(
            provedorDoControladorDoDetalheDoEncontro(
              widget.identificadorDoEncontro,
            ).notifier,
          )
          .carregueAsync();
      ref.invalidate(provedorDasPessoasFrequentes);

      return (enviou: true, mensagem: null);
    } on ExcecaoDaApi catch (excecao) {
      return (enviou: false, mensagem: excecao.mensagem);
    } catch (_) {
      return (
        enviou: false,
        mensagem: 'Não foi possível enviar o convite.',
      );
    }
  }
}

class _CabecalhoDeParticipantes extends StatelessWidget {
  const _CabecalhoDeParticipantes({
    required this.tituloDoEncontro,
    required this.quantidade,
    required this.aoVoltar,
  });

  final String tituloDoEncontro;
  final int quantidade;
  final VoidCallback aoVoltar;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(8, 12, 16, 14),
      child: CabecalhoDaPagina(
        titulo: 'Participantes',
        subtitulo:
            '$tituloDoEncontro • $quantidade ${quantidade == 1 ? 'pessoa' : 'pessoas'}',
        inicio: IconButton.filledTonal(
          tooltip: 'Voltar',
          onPressed: aoVoltar,
          icon: const Icon(Icons.arrow_back_rounded),
        ),
      ),
    );
  }
}

class _FiltrosDeParticipantes extends StatelessWidget {
  const _FiltrosDeParticipantes({
    required this.filtro,
    required this.participantes,
    required this.aoSelecionar,
  });

  final FiltroDeParticipantes filtro;
  final List<ParticipanteDoEncontro> participantes;
  final ValueChanged<FiltroDeParticipantes> aoSelecionar;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 48,
      child: ListView(
        key: const Key('lista-de-filtros-de-participantes'),
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(horizontal: 16),
        children: FiltroDeParticipantes.values
            .map(
              (FiltroDeParticipantes item) => Padding(
                padding: const EdgeInsets.only(right: 8),
                child: ChoiceChip(
                  key: Key('filtro-${item.name}'),
                  selected: filtro == item,
                  onSelected: (_) => aoSelecionar(item),
                  label: Text('${_rotulo(item)} ${_quantidade(item)}'),
                ),
              ),
            )
            .toList(),
      ),
    );
  }

  String _rotulo(FiltroDeParticipantes item) {
    return switch (item) {
      FiltroDeParticipantes.todos => 'Todos',
      FiltroDeParticipantes.confirmados => 'Vão',
      FiltroDeParticipantes.talvez => 'Talvez',
      FiltroDeParticipantes.aguardando => 'Aguardando',
      FiltroDeParticipantes.naoVao => 'Não vão',
    };
  }

  int _quantidade(FiltroDeParticipantes item) {
    return participantes.where((ParticipanteDoEncontro participante) {
      String situacao = participante.situacao.toLowerCase();

      return switch (item) {
        FiltroDeParticipantes.todos => true,
        FiltroDeParticipantes.confirmados => situacao == 'confirmado',
        FiltroDeParticipantes.talvez => situacao == 'talvez',
        FiltroDeParticipantes.aguardando => situacao == 'convidado',
        FiltroDeParticipantes.naoVao => situacao == 'naovai',
      };
    }).length;
  }
}

class _SecaoDeParticipantes extends StatelessWidget {
  const _SecaoDeParticipantes({
    required this.titulo,
    required this.participantes,
  });

  final String titulo;
  final List<ParticipanteDoEncontro> participantes;

  @override
  Widget build(BuildContext context) {
    return CartaoDoAplicativo(
      preenchimento: const EdgeInsets.symmetric(
        horizontal: EspacamentosDoAplicativo.padrao,
        vertical: EspacamentosDoAplicativo.medio,
      ),
      filho: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: <Widget>[
          Text(titulo, style: Theme.of(context).textTheme.titleMedium),
          const SizedBox(height: EspacamentosDoAplicativo.pequeno),
          ...participantes.map(
            (ParticipanteDoEncontro participante) => _LinhaDeParticipante(
              participante: participante,
            ),
          ),
        ],
      ),
    );
  }
}

class _LinhaDeParticipante extends StatelessWidget {
  const _LinhaDeParticipante({required this.participante});

  final ParticipanteDoEncontro participante;

  @override
  Widget build(BuildContext context) {
    bool useDisposicaoCompacta = MediaQuery.sizeOf(context).width <= 360;

    return Container(
      key: Key('participante-${participante.identificadorDoUsuario}'),
      constraints: const BoxConstraints(minHeight: 60),
      decoration: const BoxDecoration(
        border: Border(
          bottom: BorderSide(color: CoresDoAplicativo.bordaSuave),
        ),
      ),
      child: Row(
        children: <Widget>[
          _FotoDoParticipante(participante: participante),
          const SizedBox(width: EspacamentosDoAplicativo.medio),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: <Widget>[
                Text(
                  participante.usuarioAtual
                      ? '${participante.nome} (você)'
                      : participante.nome,
                  style: const TextStyle(fontWeight: FontWeight.w600),
                ),
                if (_ehOrganizador(participante))
                  Text(
                    participante.papel,
                    style: const TextStyle(
                      color: CoresDoAplicativo.verdeDestaque,
                      fontSize: 12,
                    ),
                  ),
                if (useDisposicaoCompacta) ...<Widget>[
                  const SizedBox(height: EspacamentosDoAplicativo.minimo),
                  Align(
                    alignment: Alignment.centerLeft,
                    child: _construaSituacao(),
                  ),
                ],
              ],
            ),
          ),
          if (!useDisposicaoCompacta) ...<Widget>[
            const SizedBox(width: EspacamentosDoAplicativo.pequeno),
            _construaSituacao(),
          ],
        ],
      ),
    );
  }

  Widget _construaSituacao() {
    return IndicadorDeSituacao(
      texto: _formateSituacao(participante.situacao),
      cor: _corDaSituacao(participante.situacao),
      icone: _iconeDaSituacao(participante.situacao),
    );
  }

  bool _ehOrganizador(ParticipanteDoEncontro participante) {
    String papel = participante.papel.toLowerCase();
    return papel == 'dono' || papel == 'organizador';
  }

  String _formateSituacao(String situacao) {
    return switch (situacao.toLowerCase()) {
      'confirmado' => 'Confirmado',
      'convidado' => 'Aguardando',
      'talvez' => 'Talvez',
      'naovai' => 'Não vai',
      _ => situacao,
    };
  }

  Color _corDaSituacao(String situacao) {
    return switch (situacao.toLowerCase()) {
      'confirmado' => CoresDoAplicativo.verdeDestaque,
      'convidado' => CoresDoAplicativo.ambar,
      'talvez' => CoresDoAplicativo.ambar,
      'naovai' => CoresDoAplicativo.coral,
      _ => CoresDoAplicativo.textoSecundario,
    };
  }

  IconData _iconeDaSituacao(String situacao) {
    return switch (situacao.toLowerCase()) {
      'confirmado' => Icons.check_rounded,
      'convidado' => Icons.schedule_rounded,
      'talvez' => Icons.help_outline_rounded,
      'naovai' => Icons.close_rounded,
      _ => Icons.person_outline_rounded,
    };
  }
}

class _FotoDoParticipante extends StatelessWidget {
  const _FotoDoParticipante({required this.participante});

  final ParticipanteDoEncontro participante;

  @override
  Widget build(BuildContext context) {
    return FotoDePerfil(
      url: participante.urlDaFotoDePerfil,
      iniciais: participante.iniciais,
      dimensao: 44,
    );
  }
}

class _EstadoVazioDoFiltro extends StatelessWidget {
  const _EstadoVazioDoFiltro({
    required this.filtroEhTodos,
    required this.aoVerTodos,
  });

  final bool filtroEhTodos;
  final VoidCallback aoVerTodos;

  @override
  Widget build(BuildContext context) {
    return EstadoVazio(
      icone: Icons.people_outline_rounded,
      titulo: filtroEhTodos
          ? 'Ninguém foi convidado ainda.'
          : 'Ninguém está nesta situação.',
      descricao: filtroEhTodos
          ? 'Convide as pessoas que farão parte deste encontro.'
          : 'Escolha outro filtro para continuar procurando.',
      acao: filtroEhTodos
          ? null
          : TextButton(onPressed: aoVerTodos, child: const Text('Ver todos')),
    );
  }
}

class _AcaoDeConvite extends StatelessWidget {
  const _AcaoDeConvite({
    required this.estaEnviando,
    required this.aoConvidar,
  });

  final bool estaEnviando;
  final VoidCallback aoConvidar;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: const BoxDecoration(
        color: CoresDoAplicativo.fundoPrincipal,
        border: Border(
          top: BorderSide(color: CoresDoAplicativo.bordaSuave),
        ),
      ),
      child: SafeArea(
        top: false,
        child: Padding(
          padding: const EdgeInsets.all(EspacamentosDoAplicativo.padrao),
          child: FilledButton.icon(
            key: const Key('convidar-pessoas'),
            onPressed: estaEnviando ? null : aoConvidar,
            icon: const Icon(Icons.person_add_alt_1_rounded),
            label: const Text('Convidar pessoas'),
          ),
        ),
      ),
    );
  }
}

typedef ResultadoDoConvite = ({bool enviou, String? mensagem});

class _FormularioDeConvite extends StatefulWidget {
  const _FormularioDeConvite({
    required this.pessoasFrequentes,
    required this.pessoasEstaoCarregando,
    required this.aoConvidarPessoaFrequente,
    required this.aoEnviar,
  });

  final List<PessoaFrequente> pessoasFrequentes;
  final bool pessoasEstaoCarregando;
  final Future<ResultadoDoConvite> Function(PessoaFrequente pessoa)
      aoConvidarPessoaFrequente;
  final Future<ResultadoDoConvite> Function(String email) aoEnviar;

  @override
  State<_FormularioDeConvite> createState() => _EstadoDoFormularioDeConvite();
}

class _EstadoDoFormularioDeConvite extends State<_FormularioDeConvite> {
  final GlobalKey<FormState> _chaveDoFormulario = GlobalKey<FormState>();
  final TextEditingController _controladorDoEmail = TextEditingController();
  bool _estaEnviando = false;
  String? _mensagemDeErro;

  @override
  void dispose() {
    _controladorDoEmail.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: EdgeInsets.fromLTRB(
        20,
        0,
        20,
        20 + MediaQuery.viewInsetsOf(context).bottom,
      ),
      child: Form(
        key: _chaveDoFormulario,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: <Widget>[
            Text(
              'Convidar pessoa',
              style: Theme.of(context)
                  .textTheme
                  .titleLarge
                  ?.copyWith(fontWeight: FontWeight.w700),
            ),
            const SizedBox(height: EspacamentosDoAplicativo.minimo),
            const Text(
              'Envie um convite para quem deve participar deste encontro.',
              style: TextStyle(color: CoresDoAplicativo.textoSecundario),
            ),
            if (widget.pessoasEstaoCarregando) ...<Widget>[
              const SizedBox(height: EspacamentosDoAplicativo.padrao),
              const LinearProgressIndicator(),
            ],
            if (widget.pessoasFrequentes.isNotEmpty) ...<Widget>[
              const SizedBox(height: EspacamentosDoAplicativo.padrao),
              Text(
                'Pessoas frequentes',
                style: Theme.of(context).textTheme.titleSmall,
              ),
              const SizedBox(height: EspacamentosDoAplicativo.pequeno),
              ConstrainedBox(
                constraints: const BoxConstraints(maxHeight: 230),
                child: ListView.separated(
                  shrinkWrap: true,
                  itemCount: widget.pessoasFrequentes.length,
                  separatorBuilder: (_, __) => const SizedBox(
                    height: EspacamentosDoAplicativo.pequeno,
                  ),
                  itemBuilder: (BuildContext context, int indice) {
                    PessoaFrequente pessoa = widget.pessoasFrequentes[indice];

                    return _SugestaoDePessoaFrequente(
                      pessoa: pessoa,
                      estaEnviando: _estaEnviando,
                      aoConvidar: () => _confirmePessoaFrequenteAsync(pessoa),
                    );
                  },
                ),
              ),
              const SizedBox(height: EspacamentosDoAplicativo.padrao),
              const Row(
                children: <Widget>[
                  Expanded(child: Divider()),
                  Padding(
                    padding: EdgeInsets.symmetric(
                      horizontal: EspacamentosDoAplicativo.medio,
                    ),
                    child: Text(
                      'ou convide por e-mail',
                      style: TextStyle(
                        color: CoresDoAplicativo.textoTerciario,
                        fontSize: 12,
                      ),
                    ),
                  ),
                  Expanded(child: Divider()),
                ],
              ),
            ],
            const SizedBox(height: EspacamentosDoAplicativo.padrao),
            TextFormField(
              key: const Key('email-do-convidado'),
              controller: _controladorDoEmail,
              autofocus: false,
              enabled: !_estaEnviando,
              keyboardType: TextInputType.emailAddress,
              textInputAction: TextInputAction.done,
              decoration: const InputDecoration(
                labelText: 'E-mail',
                prefixIcon: Icon(Icons.mail_outline_rounded),
              ),
              validator: (String? email) {
                String emailNormalizado = email?.trim() ?? '';
                bool formatoEhValido = RegExp(
                  r'^[^\s@]+@[^\s@]+\.[^\s@]+$',
                ).hasMatch(emailNormalizado);

                return formatoEhValido ? null : 'Informe um e-mail válido.';
              },
              onFieldSubmitted: (_) => _envieAsync(),
            ),
            if (_mensagemDeErro != null) ...<Widget>[
              const SizedBox(height: EspacamentosDoAplicativo.pequeno),
              Text(
                _mensagemDeErro!,
                style: const TextStyle(color: CoresDoAplicativo.coral),
              ),
            ],
            const SizedBox(height: EspacamentosDoAplicativo.padrao),
            FilledButton(
              key: const Key('confirmar-convite'),
              onPressed: _estaEnviando ? null : _envieAsync,
              child: _estaEnviando
                  ? const SizedBox.square(
                      dimension: 20,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Text('Enviar convite'),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _envieAsync() async {
    if (!_chaveDoFormulario.currentState!.validate() || _estaEnviando) {
      return;
    }

    setState(() {
      _estaEnviando = true;
      _mensagemDeErro = null;
    });
    ResultadoDoConvite resultado = await widget.aoEnviar(
      _controladorDoEmail.text.trim(),
    );

    if (!mounted) {
      return;
    }

    if (resultado.enviou) {
      context.pop();
      return;
    }

    setState(() {
      _estaEnviando = false;
      _mensagemDeErro =
          resultado.mensagem ?? 'Não foi possível enviar o convite.';
    });
  }

  Future<void> _confirmePessoaFrequenteAsync(PessoaFrequente pessoa) async {
    bool confirmou = await showDialog<bool>(
          context: context,
          builder: (BuildContext contextoDoDialogo) => AlertDialog(
            title: const Text('Enviar convite?'),
            content: Text(
              '${pessoa.nome} receberá um convite apenas para este encontro.',
            ),
            actions: <Widget>[
              TextButton(
                onPressed: () => Navigator.of(contextoDoDialogo).pop(false),
                child: const Text('Cancelar'),
              ),
              FilledButton(
                key: const Key('confirmar-convite-de-pessoa-frequente'),
                onPressed: () => Navigator.of(contextoDoDialogo).pop(true),
                child: const Text('Enviar convite'),
              ),
            ],
          ),
        ) ??
        false;

    if (!confirmou || _estaEnviando) {
      return;
    }

    setState(() {
      _estaEnviando = true;
      _mensagemDeErro = null;
    });
    ResultadoDoConvite resultado =
        await widget.aoConvidarPessoaFrequente(pessoa);

    if (!mounted) {
      return;
    }

    if (resultado.enviou) {
      context.pop();
      return;
    }

    setState(() {
      _estaEnviando = false;
      _mensagemDeErro =
          resultado.mensagem ?? 'Não foi possível enviar o convite.';
    });
  }
}

class _SugestaoDePessoaFrequente extends StatelessWidget {
  const _SugestaoDePessoaFrequente({
    required this.pessoa,
    required this.estaEnviando,
    required this.aoConvidar,
  });

  final PessoaFrequente pessoa;
  final bool estaEnviando;
  final VoidCallback aoConvidar;

  @override
  Widget build(BuildContext context) {
    return Container(
      key: Key('pessoa-frequente-${pessoa.identificadorDoUsuario}'),
      constraints: const BoxConstraints(minHeight: 58),
      padding: const EdgeInsets.symmetric(
        horizontal: EspacamentosDoAplicativo.pequeno,
        vertical: EspacamentosDoAplicativo.minimo,
      ),
      decoration: BoxDecoration(
        color: CoresDoAplicativo.fundoElevado,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: CoresDoAplicativo.bordaSuave),
      ),
      child: Row(
        children: <Widget>[
          FotoDePerfil(
            url: pessoa.urlDaFotoDePerfil,
            iniciais: pessoa.iniciais,
            dimensao: 42,
          ),
          const SizedBox(width: EspacamentosDoAplicativo.pequeno),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: <Widget>[
                Text(
                  pessoa.nome,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(fontWeight: FontWeight.w600),
                ),
                Text(
                  pessoa.textoDaRecorrencia,
                  style: const TextStyle(
                    color: CoresDoAplicativo.textoSecundario,
                    fontSize: 12,
                  ),
                ),
              ],
            ),
          ),
          IconButton.filledTonal(
            key: Key(
              'convidar-pessoa-frequente-${pessoa.identificadorDoUsuario}',
            ),
            tooltip: 'Convidar ${pessoa.nome}',
            onPressed: estaEnviando ? null : aoConvidar,
            icon: const Icon(Icons.person_add_alt_1_rounded),
          ),
        ],
      ),
    );
  }
}

class _MensagemDaTela extends StatelessWidget {
  const _MensagemDaTela({required this.mensagem, required this.cor});

  final String mensagem;
  final Color cor;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
      child: Text(mensagem,
          textAlign: TextAlign.center, style: TextStyle(color: cor)),
    );
  }
}

class _ErroDeParticipantes extends StatelessWidget {
  const _ErroDeParticipantes({
    required this.mensagem,
    required this.aoVoltar,
    required this.aoTentarNovamente,
  });

  final String mensagem;
  final VoidCallback aoVoltar;
  final VoidCallback aoTentarNovamente;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(EspacamentosDoAplicativo.grande),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: <Widget>[
            const Icon(
              Icons.people_outline_rounded,
              size: 42,
              color: CoresDoAplicativo.coral,
            ),
            const SizedBox(height: EspacamentosDoAplicativo.medio),
            Text(mensagem, textAlign: TextAlign.center),
            const SizedBox(height: EspacamentosDoAplicativo.padrao),
            FilledButton(
              onPressed: aoTentarNovamente,
              child: const Text('Tentar novamente'),
            ),
            TextButton(onPressed: aoVoltar, child: const Text('Voltar')),
          ],
        ),
      ),
    );
  }
}
