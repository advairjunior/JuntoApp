import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cabecalho_da_pagina.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cartao_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/conteudo_responsivo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estado_vazio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/indicador_de_situacao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/configuracao/configuracao_do_ambiente.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/imagem_privada.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/dados/repositorio_da_linha_do_tempo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/modelos/item_da_linha_do_tempo.dart';

class TelaDeMemorias extends ConsumerStatefulWidget {
  const TelaDeMemorias({super.key});

  @override
  ConsumerState<TelaDeMemorias> createState() => _EstadoDaTelaDeMemorias();
}

class _EstadoDaTelaDeMemorias extends ConsumerState<TelaDeMemorias> {
  FiltroDaLinhaDoTempo _filtro = FiltroDaLinhaDoTempo.todos;

  @override
  Widget build(BuildContext context) {
    AsyncValue<LinhaDoTempo> linhaDoTempo = ref.watch(
      provedorDaLinhaDoTempo(_filtro),
    );

    return ConteudoResponsivo(
      preenchimento: const EdgeInsets.fromLTRB(
        EspacamentosDoAplicativo.padrao,
        EspacamentosDoAplicativo.grande,
        EspacamentosDoAplicativo.padrao,
        EspacamentosDoAplicativo.alturaDoDock + 32,
      ),
      filho: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: <Widget>[
          const CabecalhoDaPagina(
            titulo: 'Memórias',
            subtitulo: 'Reencontre os momentos que vocês viveram juntos.',
          ),
          const SizedBox(height: EspacamentosDoAplicativo.grande),
          _FiltrosDaLinhaDoTempo(
            filtro: _filtro,
            aoSelecionar: (FiltroDaLinhaDoTempo filtro) {
              setState(() {
                _filtro = filtro;
              });
            },
          ),
          const SizedBox(height: EspacamentosDoAplicativo.grande),
          Expanded(
            child: linhaDoTempo.when(
              loading: () => const Center(child: CircularProgressIndicator()),
              error: (_, __) => _ErroDaLinhaDoTempo(
                aoTentarNovamente: () =>
                    ref.invalidate(provedorDaLinhaDoTempo(_filtro)),
              ),
              data: (LinhaDoTempo resposta) => _ConteudoDaLinhaDoTempo(
                itens: resposta.itens,
                filtro: _filtro,
                aoVerTodos: () {
                  setState(() {
                    _filtro = FiltroDaLinhaDoTempo.todos;
                  });
                },
                aoAtualizar: () async {
                  ref.invalidate(provedorDaLinhaDoTempo(_filtro));
                  await ref.read(provedorDaLinhaDoTempo(_filtro).future);
                },
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _FiltrosDaLinhaDoTempo extends StatelessWidget {
  const _FiltrosDaLinhaDoTempo({
    required this.filtro,
    required this.aoSelecionar,
  });

  final FiltroDaLinhaDoTempo filtro;
  final ValueChanged<FiltroDaLinhaDoTempo> aoSelecionar;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 42,
      child: ListView.separated(
        key: const Key('filtros-da-linha-do-tempo'),
        scrollDirection: Axis.horizontal,
        itemCount: FiltroDaLinhaDoTempo.values.length,
        separatorBuilder: (_, __) =>
            const SizedBox(width: EspacamentosDoAplicativo.pequeno),
        itemBuilder: (BuildContext context, int indice) {
          FiltroDaLinhaDoTempo item = FiltroDaLinhaDoTempo.values[indice];

          return ChoiceChip(
            key: Key('filtro-${item.valor}'),
            selected: item == filtro,
            onSelected: (_) => aoSelecionar(item),
            label: Text(item.rotulo),
          );
        },
      ),
    );
  }
}

class _ConteudoDaLinhaDoTempo extends StatelessWidget {
  const _ConteudoDaLinhaDoTempo({
    required this.itens,
    required this.filtro,
    required this.aoVerTodos,
    required this.aoAtualizar,
  });

  final List<ItemDaLinhaDoTempo> itens;
  final FiltroDaLinhaDoTempo filtro;
  final VoidCallback aoVerTodos;
  final Future<void> Function() aoAtualizar;

  @override
  Widget build(BuildContext context) {
    if (itens.isEmpty) {
      return _LinhaDoTempoVazia(
        filtro: filtro,
        aoVerTodos: aoVerTodos,
      );
    }

    return RefreshIndicator(
      onRefresh: aoAtualizar,
      child: ListView.builder(
        key: const Key('lista-da-linha-do-tempo'),
        physics: const AlwaysScrollableScrollPhysics(),
        itemCount: itens.length,
        itemBuilder: (BuildContext context, int indice) {
          ItemDaLinhaDoTempo item = itens[indice];
          bool exibaMes = indice == 0 ||
              itens[indice - 1].inicio.month != item.inicio.month ||
              itens[indice - 1].inicio.year != item.inicio.year;

          return Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: <Widget>[
              if (exibaMes) ...<Widget>[
                if (indice > 0)
                  const SizedBox(height: EspacamentosDoAplicativo.grande),
                Text(
                  DateFormat('MMMM \'de\' yyyy', 'pt_BR').format(item.inicio),
                  style: Theme.of(context).textTheme.titleMedium,
                ),
                const SizedBox(height: EspacamentosDoAplicativo.medio),
              ],
              _CartaoDaLinhaDoTempo(item: item),
              const SizedBox(height: EspacamentosDoAplicativo.medio),
            ],
          );
        },
      ),
    );
  }
}

class _CartaoDaLinhaDoTempo extends StatelessWidget {
  const _CartaoDaLinhaDoTempo({required this.item});

  final ItemDaLinhaDoTempo item;

  @override
  Widget build(BuildContext context) {
    String recurso = ConfiguracaoDoAmbiente.crieUrlAbsoluta(item.urlDaImagem);

    return CartaoDoAplicativo(
      key: Key('memoria-${item.identificadorDoEncontro}'),
      preenchimento: EdgeInsets.zero,
      aoTocar: () => context.push<void>(
        '/encontros/${item.identificadorDoEncontro}',
      ),
      filho: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: <Widget>[
          SizedBox(
            height: 180,
            width: double.infinity,
            child: Stack(
              fit: StackFit.expand,
              children: <Widget>[
                if (recurso.isEmpty)
                  const _FundoDaMemoria()
                else
                  ImagemPrivada(
                    recurso: recurso,
                    construaSubstituta: (_) => const _FundoDaMemoria(),
                  ),
                const DecoratedBox(
                  decoration: BoxDecoration(
                    gradient: LinearGradient(
                      begin: Alignment.topCenter,
                      end: Alignment.bottomCenter,
                      colors: <Color>[
                        Colors.transparent,
                        Color(0xE6000B08),
                      ],
                    ),
                  ),
                ),
                Positioned(
                  left: EspacamentosDoAplicativo.padrao,
                  right: EspacamentosDoAplicativo.padrao,
                  bottom: EspacamentosDoAplicativo.padrao,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: <Widget>[
                      IndicadorDeSituacao(
                        texto: item.situacao,
                        cor: _corDaSituacao(item.situacao),
                        icone: Icons.history_rounded,
                      ),
                      const SizedBox(height: EspacamentosDoAplicativo.pequeno),
                      Text(
                        item.titulo,
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                        style: Theme.of(context)
                            .textTheme
                            .titleLarge
                            ?.copyWith(color: Colors.white),
                      ),
                      const SizedBox(height: EspacamentosDoAplicativo.minimo),
                      Text(
                        DateFormat("dd/MM/yyyy '•' HH:mm", 'pt_BR')
                            .format(item.inicio),
                        style: const TextStyle(
                          color: CoresDoAplicativo.textoSecundario,
                          fontSize: 12,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(EspacamentosDoAplicativo.padrao),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: <Widget>[
                if (item.descricao != null && item.descricao!.trim().isNotEmpty)
                  Text(
                    item.descricao!,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      color: CoresDoAplicativo.textoSecundario,
                    ),
                  ),
                if (item.local != null &&
                    item.local!.trim().isNotEmpty) ...<Widget>[
                  const SizedBox(height: EspacamentosDoAplicativo.pequeno),
                  _InformacaoDaMemoria(
                    icone: Icons.location_on_outlined,
                    texto: item.local!,
                  ),
                ],
                if (item
                    .nomesDosParticipantesEmDestaque.isNotEmpty) ...<Widget>[
                  const SizedBox(height: EspacamentosDoAplicativo.pequeno),
                  Row(
                    children: <Widget>[
                      const Icon(
                        Icons.favorite_border_rounded,
                        size: 17,
                        color: CoresDoAplicativo.verdeDestaque,
                      ),
                      const SizedBox(width: EspacamentosDoAplicativo.minimo),
                      Expanded(
                        child: Text(
                          _descrevaPessoas(),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(
                            color: CoresDoAplicativo.textoSecundario,
                          ),
                        ),
                      ),
                    ],
                  ),
                ],
                const SizedBox(height: EspacamentosDoAplicativo.medio),
                Row(
                  children: <Widget>[
                    _InformacaoDaMemoria(
                      icone: Icons.people_outline_rounded,
                      texto: '${item.quantidadeDeParticipantes}',
                    ),
                    const SizedBox(width: EspacamentosDoAplicativo.padrao),
                    _InformacaoDaMemoria(
                      icone: Icons.photo_library_outlined,
                      texto: '${item.quantidadeDeMemorias}',
                    ),
                    const Spacer(),
                    if (item.quantidadeDeMemorias > 0)
                      IconButton.filledTonal(
                        key: Key(
                          'abrir-galeria-${item.identificadorDoEncontro}',
                        ),
                        tooltip: 'Abrir galeria',
                        onPressed: () => context.push<void>(
                          '/encontros/${item.identificadorDoEncontro}/midias',
                        ),
                        icon: const Icon(Icons.photo_library_outlined),
                      )
                    else
                      const Icon(
                        Icons.chevron_right_rounded,
                        color: CoresDoAplicativo.textoTerciario,
                      ),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  String _descrevaPessoas() {
    List<String> nomes = item.nomesDosParticipantesEmDestaque.take(2).toList();
    int quantidadeRestante = item.quantidadeDeParticipantes - nomes.length;
    String pessoas = nomes.join(' e ');

    return quantidadeRestante > 0
        ? 'Com $pessoas e mais $quantidadeRestante'
        : 'Com $pessoas';
  }

  Color _corDaSituacao(String situacao) {
    return switch (situacao.toLowerCase()) {
      'realizado' => CoresDoAplicativo.verdeDestaque,
      'cancelado' => CoresDoAplicativo.coral,
      _ => CoresDoAplicativo.ambar,
    };
  }
}

class _InformacaoDaMemoria extends StatelessWidget {
  const _InformacaoDaMemoria({required this.icone, required this.texto});

  final IconData icone;
  final String texto;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: <Widget>[
        Icon(icone, size: 17, color: CoresDoAplicativo.verdeDestaque),
        const SizedBox(width: EspacamentosDoAplicativo.minimo),
        Text(
          texto,
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
          style: const TextStyle(color: CoresDoAplicativo.textoSecundario),
        ),
      ],
    );
  }
}

class _FundoDaMemoria extends StatelessWidget {
  const _FundoDaMemoria();

  @override
  Widget build(BuildContext context) {
    return const DecoratedBox(
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: <Color>[
            CoresDoAplicativo.verdeEscuro,
            CoresDoAplicativo.fundoDoCartaoSuave,
            Color(0xFF5A3B12),
          ],
        ),
      ),
      child: Center(
        child: Icon(
          Icons.auto_awesome_outlined,
          size: 48,
          color: CoresDoAplicativo.ambar,
        ),
      ),
    );
  }
}

class _LinhaDoTempoVazia extends StatelessWidget {
  const _LinhaDoTempoVazia({
    required this.filtro,
    required this.aoVerTodos,
  });

  final FiltroDaLinhaDoTempo filtro;
  final VoidCallback aoVerTodos;

  @override
  Widget build(BuildContext context) {
    return EstadoVazio(
      icone: Icons.auto_awesome_outlined,
      titulo: filtro == FiltroDaLinhaDoTempo.todos
          ? 'Suas memórias começam nos encontros'
          : 'Nenhuma memória neste período',
      descricao: filtro == FiltroDaLinhaDoTempo.todos
          ? 'Quando vocês viverem momentos juntos, eles ficarão guardados aqui.'
          : 'Escolha outro filtro para rever mais encontros.',
      acao: filtro == FiltroDaLinhaDoTempo.todos
          ? null
          : TextButton(
              onPressed: aoVerTodos,
              child: const Text('Ver todos'),
            ),
    );
  }
}

class _ErroDaLinhaDoTempo extends StatelessWidget {
  const _ErroDaLinhaDoTempo({required this.aoTentarNovamente});

  final VoidCallback aoTentarNovamente;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: CartaoDoAplicativo(
        filho: EstadoVazio(
          icone: Icons.cloud_off_outlined,
          titulo: 'Não foi possível carregar suas memórias',
          descricao: 'Verifique sua conexão e tente novamente.',
          acao: FilledButton(
            onPressed: aoTentarNovamente,
            child: const Text('Tentar novamente'),
          ),
        ),
      ),
    );
  }
}
