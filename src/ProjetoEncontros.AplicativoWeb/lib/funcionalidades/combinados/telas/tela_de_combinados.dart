import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cabecalho_da_pagina.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cartao_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/conteudo_responsivo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estado_vazio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/foto_de_perfil.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/combinados/dados/repositorio_de_combinados.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/combinados/modelos/item_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/dados/repositorio_de_encontros.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/encontro_detalhado.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/participante_do_encontro.dart';

final provedorDoEncontroDosCombinados = FutureProvider.autoDispose
    .family<EncontroDetalhado, String>((Ref referencia, String encontro) {
  return referencia
      .watch(provedorDoRepositorioDeEncontros)
      .obtenhaEncontroAsync(
        encontro,
      );
});

class TelaDeCombinados extends ConsumerWidget {
  const TelaDeCombinados({
    required this.identificadorDoEncontro,
    super.key,
  });

  final String identificadorDoEncontro;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    AsyncValue<List<ItemDoEncontro>> combinados = ref.watch(
      provedorDosCombinados(identificadorDoEncontro),
    );
    AsyncValue<EncontroDetalhado> encontro = ref.watch(
      provedorDoEncontroDosCombinados(identificadorDoEncontro),
    );

    return Scaffold(
      body: SafeArea(
        child: ConteudoResponsivo(
          preenchimento: const EdgeInsets.all(
            EspacamentosDoAplicativo.padrao,
          ),
          filho: Column(
            children: <Widget>[
              CabecalhoDaPagina(
                titulo: 'Combinados',
                subtitulo: 'O que precisa ser lembrado para este encontro.',
                inicio: IconButton(
                  tooltip: 'Voltar',
                  onPressed: () => context.pop(),
                  icon: const Icon(Icons.arrow_back_ios_new_rounded),
                ),
                acoes: <Widget>[
                  IconButton.filled(
                    key: const Key('adicionar-combinado'),
                    tooltip: 'Adicionar combinado',
                    onPressed: encontro.valueOrNull == null
                        ? null
                        : () => _editeAsync(
                              context,
                              ref,
                              encontro.valueOrNull!,
                            ),
                    icon: const Icon(Icons.add_rounded),
                  ),
                ],
              ),
              const SizedBox(height: EspacamentosDoAplicativo.grande),
              Expanded(
                child: encontro.hasError || combinados.hasError
                    ? _ErroDosCombinados(
                        aoTentarNovamente: () => _atualize(ref),
                      )
                    : encontro.isLoading || combinados.isLoading
                        ? const Center(child: CircularProgressIndicator())
                        : _ListaDeCombinados(
                            itens: combinados.requireValue,
                            aoAtualizar: () async => _atualize(ref),
                            aoEditar: (ItemDoEncontro item) => _editeAsync(
                              context,
                              ref,
                              encontro.requireValue,
                              item: item,
                            ),
                            aoAlternar: (ItemDoEncontro item) =>
                                _alterneAsync(context, ref, item),
                            aoAdicionar: () => _editeAsync(
                              context,
                              ref,
                              encontro.requireValue,
                            ),
                          ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  void _atualize(WidgetRef ref) {
    ref.invalidate(provedorDosCombinados(identificadorDoEncontro));
    ref.invalidate(provedorDoEncontroDosCombinados(identificadorDoEncontro));
  }

  Future<void> _alterneAsync(
    BuildContext context,
    WidgetRef ref,
    ItemDoEncontro item,
  ) async {
    try {
      await ref.read(provedorDoRepositorioDeCombinados).altereSituacaoAsync(
            identificadorDoEncontro: identificadorDoEncontro,
            identificadorDoItem: item.identificador,
            resolva: !item.estaResolvido,
          );
      ref.invalidate(provedorDosCombinados(identificadorDoEncontro));
    } catch (_) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Não foi possível atualizar o combinado.'),
          ),
        );
      }
    }
  }

  Future<void> _editeAsync(
    BuildContext context,
    WidgetRef ref,
    EncontroDetalhado encontro, {
    ItemDoEncontro? item,
  }) async {
    String descricao = item?.descricao ?? '';
    String responsavel = item?.identificadorDoUsuarioResponsavel ?? '';
    bool? alterou = await showModalBottomSheet<bool>(
      context: context,
      isScrollControlled: true,
      useSafeArea: true,
      builder: (BuildContext contextoDoPainel) {
        bool estaSalvando = false;

        return StatefulBuilder(
          builder: (BuildContext context, StateSetter definaEstado) {
            Future<void> salveAsync() async {
              String texto = descricao.trim();

              if (texto.isEmpty) {
                return;
              }

              definaEstado(() => estaSalvando = true);

              try {
                if (item == null) {
                  await ref.read(provedorDoRepositorioDeCombinados).crieAsync(
                        identificadorDoEncontro: identificadorDoEncontro,
                        descricao: texto,
                        identificadorDoResponsavel:
                            responsavel.isEmpty ? null : responsavel,
                      );
                } else {
                  await ref.read(provedorDoRepositorioDeCombinados).editeAsync(
                        identificadorDoEncontro: identificadorDoEncontro,
                        identificadorDoItem: item.identificador,
                        descricao: texto,
                        identificadorDoResponsavel:
                            responsavel.isEmpty ? null : responsavel,
                      );
                }

                if (contextoDoPainel.mounted) {
                  Navigator.of(contextoDoPainel).pop(true);
                }
              } catch (_) {
                if (contextoDoPainel.mounted) {
                  ScaffoldMessenger.of(contextoDoPainel).showSnackBar(
                    const SnackBar(
                      content: Text('Não foi possível salvar o combinado.'),
                    ),
                  );
                  definaEstado(() => estaSalvando = false);
                }
              }
            }

            Future<void> excluaAsync() async {
              bool confirmou = await showDialog<bool>(
                    context: contextoDoPainel,
                    builder: (BuildContext contextoDoDialogo) => AlertDialog(
                      title: const Text('Excluir combinado?'),
                      content: const Text(
                        'Esse combinado vai sair da lista do encontro.',
                      ),
                      actions: <Widget>[
                        TextButton(
                          onPressed: () =>
                              Navigator.of(contextoDoDialogo).pop(false),
                          child: const Text('Cancelar'),
                        ),
                        FilledButton(
                          key: const Key('confirmar-exclusao-do-combinado'),
                          onPressed: () =>
                              Navigator.of(contextoDoDialogo).pop(true),
                          child: const Text('Excluir'),
                        ),
                      ],
                    ),
                  ) ??
                  false;

              if (!confirmou || item == null) {
                return;
              }

              definaEstado(() => estaSalvando = true);

              try {
                await ref.read(provedorDoRepositorioDeCombinados).removaAsync(
                      identificadorDoEncontro: identificadorDoEncontro,
                      identificadorDoItem: item.identificador,
                    );

                if (contextoDoPainel.mounted) {
                  Navigator.of(contextoDoPainel).pop(true);
                }
              } catch (_) {
                if (contextoDoPainel.mounted) {
                  definaEstado(() => estaSalvando = false);
                }
              }
            }

            return Padding(
              padding: EdgeInsets.fromLTRB(
                EspacamentosDoAplicativo.padrao,
                EspacamentosDoAplicativo.padrao,
                EspacamentosDoAplicativo.padrao,
                MediaQuery.viewInsetsOf(contextoDoPainel).bottom +
                    EspacamentosDoAplicativo.padrao,
              ),
              child: SingleChildScrollView(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: <Widget>[
                    Text(
                      item == null ? 'Novo combinado' : 'Editar combinado',
                      style: Theme.of(context).textTheme.titleLarge,
                    ),
                    const SizedBox(height: EspacamentosDoAplicativo.padrao),
                    TextFormField(
                      key: const Key('descricao-do-combinado'),
                      initialValue: descricao,
                      autofocus: true,
                      maxLength: 140,
                      maxLines: 3,
                      onChanged: (String valor) => descricao = valor,
                      decoration: const InputDecoration(
                        labelText: 'O que precisa ser lembrado?',
                        hintText: 'Ex.: Levar refrigerante',
                      ),
                    ),
                    const SizedBox(height: EspacamentosDoAplicativo.medio),
                    DropdownButtonFormField<String>(
                      value: responsavel,
                      isExpanded: true,
                      decoration: const InputDecoration(
                        labelText: 'Responsável',
                      ),
                      items: <DropdownMenuItem<String>>[
                        const DropdownMenuItem<String>(
                          value: '',
                          child: Text('Sem responsável'),
                        ),
                        ...encontro.participantes.map(
                          (ParticipanteDoEncontro participante) =>
                              DropdownMenuItem<String>(
                            value: participante.identificadorDoUsuario,
                            child: Text(
                              participante.usuarioAtual
                                  ? '${participante.nome} (você)'
                                  : participante.nome,
                              overflow: TextOverflow.ellipsis,
                            ),
                          ),
                        ),
                      ],
                      onChanged: estaSalvando
                          ? null
                          : (String? valor) {
                              definaEstado(() => responsavel = valor ?? '');
                            },
                    ),
                    const SizedBox(height: EspacamentosDoAplicativo.padrao),
                    FilledButton(
                      key: const Key('salvar-combinado'),
                      onPressed: estaSalvando ? null : salveAsync,
                      child: Text(estaSalvando ? 'Salvando...' : 'Salvar'),
                    ),
                    if (item != null) ...<Widget>[
                      const SizedBox(height: EspacamentosDoAplicativo.pequeno),
                      OutlinedButton.icon(
                        onPressed: estaSalvando ? null : excluaAsync,
                        icon: const Icon(Icons.delete_outline_rounded),
                        label: const Text('Excluir combinado'),
                      ),
                    ],
                  ],
                ),
              ),
            );
          },
        );
      },
    );
    if (alterou == true) {
      ref.invalidate(provedorDosCombinados(identificadorDoEncontro));
    }
  }
}

class _ListaDeCombinados extends StatelessWidget {
  const _ListaDeCombinados({
    required this.itens,
    required this.aoAtualizar,
    required this.aoEditar,
    required this.aoAlternar,
    required this.aoAdicionar,
  });

  final List<ItemDoEncontro> itens;
  final Future<void> Function() aoAtualizar;
  final ValueChanged<ItemDoEncontro> aoEditar;
  final ValueChanged<ItemDoEncontro> aoAlternar;
  final VoidCallback aoAdicionar;

  @override
  Widget build(BuildContext context) {
    if (itens.isEmpty) {
      return EstadoVazio(
        icone: Icons.checklist_rounded,
        titulo: 'Ainda não há combinados',
        descricao:
            'Adicione algo simples: levar bebida, comprar gelo ou separar cadeiras.',
        acao: FilledButton.icon(
          onPressed: aoAdicionar,
          icon: const Icon(Icons.add_rounded),
          label: const Text('Adicionar combinado'),
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: aoAtualizar,
      child: ListView.separated(
        physics: const AlwaysScrollableScrollPhysics(),
        itemCount: itens.length,
        separatorBuilder: (_, __) => const SizedBox(
          height: EspacamentosDoAplicativo.pequeno,
        ),
        itemBuilder: (BuildContext context, int indice) {
          ItemDoEncontro item = itens[indice];

          return CartaoDoAplicativo(
            key: Key('combinado-${item.identificador}'),
            aoTocar: () => aoEditar(item),
            filho: Row(
              children: <Widget>[
                IconButton(
                  tooltip: item.estaResolvido
                      ? 'Marcar como pendente'
                      : 'Marcar como resolvido',
                  onPressed: () => aoAlternar(item),
                  icon: Icon(
                    item.estaResolvido
                        ? Icons.check_circle_rounded
                        : Icons.radio_button_unchecked_rounded,
                    color: item.estaResolvido
                        ? CoresDoAplicativo.verdeDestaque
                        : CoresDoAplicativo.ambar,
                  ),
                ),
                const SizedBox(width: EspacamentosDoAplicativo.pequeno),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: <Widget>[
                      Text(
                        item.descricao,
                        style: TextStyle(
                          fontWeight: FontWeight.w600,
                          decoration: item.estaResolvido
                              ? TextDecoration.lineThrough
                              : null,
                        ),
                      ),
                      const SizedBox(height: EspacamentosDoAplicativo.minimo),
                      Text(
                        item.nomeDoResponsavel == null
                            ? 'Sem responsável'
                            : 'Ficou com ${item.nomeDoResponsavel}',
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        style: const TextStyle(
                          color: CoresDoAplicativo.textoSecundario,
                          fontSize: 12,
                        ),
                      ),
                    ],
                  ),
                ),
                if (item.nomeDoResponsavel != null) ...<Widget>[
                  const SizedBox(width: EspacamentosDoAplicativo.pequeno),
                  FotoDePerfil(
                    url: item.urlDaFotoDePerfilDoResponsavel,
                    iniciais: _obtenhaIniciais(item.nomeDoResponsavel!),
                    dimensao: 36,
                    tamanhoDasIniciais: 12,
                  ),
                ],
                const SizedBox(width: EspacamentosDoAplicativo.minimo),
                const Icon(
                  Icons.chevron_right_rounded,
                  color: CoresDoAplicativo.textoTerciario,
                ),
              ],
            ),
          );
        },
      ),
    );
  }

  String _obtenhaIniciais(String nome) {
    List<String> partes = nome
        .trim()
        .split(RegExp(r'\s+'))
        .where((String parte) => parte.isNotEmpty)
        .toList();

    if (partes.isEmpty) {
      return '?';
    }

    if (partes.length == 1) {
      return partes.first.substring(0, 1).toUpperCase();
    }

    return '${partes.first[0]}${partes.last[0]}'.toUpperCase();
  }
}

class _ErroDosCombinados extends StatelessWidget {
  const _ErroDosCombinados({required this.aoTentarNovamente});

  final VoidCallback aoTentarNovamente;

  @override
  Widget build(BuildContext context) {
    return EstadoVazio(
      icone: Icons.playlist_remove_rounded,
      titulo: 'Não foi possível carregar',
      descricao: 'Verifique sua conexão e tente novamente.',
      acao: FilledButton(
        onPressed: aoTentarNovamente,
        child: const Text('Tentar novamente'),
      ),
    );
  }
}
