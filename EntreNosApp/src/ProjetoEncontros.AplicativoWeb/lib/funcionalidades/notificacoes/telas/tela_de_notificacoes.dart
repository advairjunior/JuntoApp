import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cabecalho_da_pagina.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cartao_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/conteudo_responsivo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estado_vazio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/notificacoes/dados/repositorio_de_notificacoes.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/notificacoes/modelos/notificacao_do_usuario.dart';

class TelaDeNotificacoes extends ConsumerWidget {
  const TelaDeNotificacoes({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    AsyncValue<ListaDeNotificacoes> lista = ref.watch(
      provedorDaListaDeNotificacoes,
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
                titulo: 'Notificações',
                subtitulo: 'O que merece sua atenção, sem excesso.',
                inicio: IconButton(
                  tooltip: 'Voltar',
                  onPressed: () => _volte(context),
                  icon: const Icon(Icons.arrow_back_ios_new_rounded),
                ),
              ),
              const SizedBox(height: EspacamentosDoAplicativo.grande),
              Expanded(
                child: lista.when(
                  loading: () =>
                      const Center(child: CircularProgressIndicator()),
                  error: (_, __) => _ErroDasNotificacoes(
                    aoTentarNovamente: () =>
                        ref.invalidate(provedorDaListaDeNotificacoes),
                  ),
                  data: (ListaDeNotificacoes resposta) {
                    if (resposta.notificacoes.isEmpty) {
                      return const EstadoVazio(
                        icone: Icons.notifications_none_rounded,
                        titulo: 'Tudo tranquilo por aqui',
                        descricao:
                            'Convites, mudanças e combinados importantes aparecerão nesta área.',
                      );
                    }

                    return RefreshIndicator(
                      onRefresh: () async {
                        ref.invalidate(provedorDaListaDeNotificacoes);
                        await ref.read(provedorDaListaDeNotificacoes.future);
                      },
                      child: ListView.separated(
                        physics: const AlwaysScrollableScrollPhysics(),
                        itemCount: resposta.notificacoes.length,
                        separatorBuilder: (_, __) => const SizedBox(
                          height: EspacamentosDoAplicativo.pequeno,
                        ),
                        itemBuilder: (BuildContext context, int indice) {
                          NotificacaoDoUsuario notificacao =
                              resposta.notificacoes[indice];

                          return _CartaoDaNotificacao(
                            notificacao: notificacao,
                            aoAbrir: () => _abraAsync(
                              context,
                              ref,
                              notificacao,
                            ),
                          );
                        },
                      ),
                    );
                  },
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Future<void> _abraAsync(
    BuildContext context,
    WidgetRef ref,
    NotificacaoDoUsuario notificacao,
  ) async {
    if (!notificacao.estaLida) {
      try {
        await ref
            .read(provedorDoRepositorioDeNotificacoes)
            .marqueComoLidaAsync(notificacao.identificador);
        ref.invalidate(provedorDaListaDeNotificacoes);
      } catch (_) {
        if (context.mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Não foi possível atualizar a notificação.'),
            ),
          );
        }
        return;
      }
    }

    if (context.mounted && notificacao.identificadorDoEncontro != null) {
      bool notificacaoEhConvite =
          notificacao.tipo.toLowerCase().contains('convite');
      String complemento =
          notificacaoEhConvite ? '?responder-presenca=true' : '';
      await context.push<void>(
        '/encontros/${notificacao.identificadorDoEncontro}$complemento',
      );
    }
  }

  void _volte(BuildContext context) {
    if (context.canPop()) {
      context.pop();
      return;
    }

    context.go('/inicio');
  }
}

class _CartaoDaNotificacao extends StatelessWidget {
  const _CartaoDaNotificacao({
    required this.notificacao,
    required this.aoAbrir,
  });

  final NotificacaoDoUsuario notificacao;
  final VoidCallback aoAbrir;

  @override
  Widget build(BuildContext context) {
    return CartaoDoAplicativo(
      key: Key('notificacao-${notificacao.identificador}'),
      elevado: !notificacao.estaLida,
      aoTocar: aoAbrir,
      filho: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: <Widget>[
          Container(
            width: 44,
            height: 44,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              color: _cor().withValues(alpha: 0.14),
            ),
            child: Icon(_icone(), color: _cor(), size: 22),
          ),
          const SizedBox(width: EspacamentosDoAplicativo.medio),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: <Widget>[
                Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: <Widget>[
                    Expanded(
                      child: Text(
                        notificacao.titulo,
                        style: Theme.of(context).textTheme.titleSmall?.copyWith(
                              fontWeight: notificacao.estaLida
                                  ? FontWeight.w500
                                  : FontWeight.w700,
                            ),
                      ),
                    ),
                    if (!notificacao.estaLida) ...<Widget>[
                      const SizedBox(width: EspacamentosDoAplicativo.pequeno),
                      Semantics(
                        label: 'Não lida',
                        child: const CircleAvatar(
                          radius: 4,
                          backgroundColor: CoresDoAplicativo.verdeDestaque,
                        ),
                      ),
                    ],
                  ],
                ),
                const SizedBox(height: EspacamentosDoAplicativo.minimo),
                Text(
                  notificacao.mensagem,
                  style: const TextStyle(
                    color: CoresDoAplicativo.textoSecundario,
                  ),
                ),
                const SizedBox(height: EspacamentosDoAplicativo.pequeno),
                Text(
                  DateFormat("dd/MM/yyyy '•' HH:mm")
                      .format(notificacao.criadaEm),
                  style: const TextStyle(
                    color: CoresDoAplicativo.textoTerciario,
                    fontSize: 12,
                  ),
                ),
              ],
            ),
          ),
          if (notificacao.identificadorDoEncontro != null)
            const Padding(
              padding: EdgeInsets.only(top: 10),
              child: Icon(
                Icons.chevron_right_rounded,
                color: CoresDoAplicativo.textoTerciario,
              ),
            ),
        ],
      ),
    );
  }

  IconData _icone() {
    String tipo = notificacao.tipo.toLowerCase();

    if (tipo.contains('convite')) {
      return Icons.person_add_alt_1_rounded;
    }

    if (tipo.contains('item') || tipo.contains('combinado')) {
      return Icons.checklist_rounded;
    }

    if (tipo.contains('lembrete')) {
      return Icons.alarm_rounded;
    }

    if (tipo.contains('cota') || tipo.contains('armazenamento')) {
      return Icons.storage_rounded;
    }

    return Icons.event_note_rounded;
  }

  Color _cor() {
    return notificacao.estaLida
        ? CoresDoAplicativo.textoTerciario
        : CoresDoAplicativo.verdeDestaque;
  }
}

class _ErroDasNotificacoes extends StatelessWidget {
  const _ErroDasNotificacoes({required this.aoTentarNovamente});

  final VoidCallback aoTentarNovamente;

  @override
  Widget build(BuildContext context) {
    return EstadoVazio(
      icone: Icons.notifications_off_outlined,
      titulo: 'Não foi possível carregar',
      descricao: 'Verifique sua conexão e tente novamente.',
      acao: FilledButton(
        onPressed: aoTentarNovamente,
        child: const Text('Tentar novamente'),
      ),
    );
  }
}
