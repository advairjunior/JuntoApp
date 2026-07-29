import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cabecalho_da_pagina.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cartao_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/conteudo_responsivo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estado_vazio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/notificacoes/dados/repositorio_de_notificacoes.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/notificacoes/modelos/preferencia_de_notificacao.dart';

class TelaDePreferenciasDeNotificacao extends ConsumerWidget {
  const TelaDePreferenciasDeNotificacao({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    AsyncValue<PreferenciaDeNotificacao> preferencias = ref.watch(
      provedorDasPreferenciasDeNotificacao,
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
                subtitulo: 'Escolha o que merece chamar sua atenção.',
                inicio: IconButton(
                  tooltip: 'Voltar',
                  onPressed: () => context.pop(),
                  icon: const Icon(Icons.arrow_back_ios_new_rounded),
                ),
              ),
              const SizedBox(height: EspacamentosDoAplicativo.grande),
              Expanded(
                child: preferencias.when(
                  loading: () =>
                      const Center(child: CircularProgressIndicator()),
                  error: (_, __) => EstadoVazio(
                    icone: Icons.tune_rounded,
                    titulo: 'Não foi possível carregar',
                    descricao: 'Verifique sua conexão e tente novamente.',
                    acao: FilledButton(
                      onPressed: () => ref.invalidate(
                        provedorDasPreferenciasDeNotificacao,
                      ),
                      child: const Text('Tentar novamente'),
                    ),
                  ),
                  data: (PreferenciaDeNotificacao valor) =>
                      _FormularioDePreferencias(
                    key: ValueKey<PreferenciaDeNotificacao>(valor),
                    preferencias: valor,
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _FormularioDePreferencias extends ConsumerStatefulWidget {
  const _FormularioDePreferencias({
    required this.preferencias,
    super.key,
  });

  final PreferenciaDeNotificacao preferencias;

  @override
  ConsumerState<_FormularioDePreferencias> createState() =>
      _EstadoDoFormularioDePreferencias();
}

class _EstadoDoFormularioDePreferencias
    extends ConsumerState<_FormularioDePreferencias> {
  late bool _convites;
  late bool _lembretes;
  late bool _alteracoes;
  late bool _combinados;
  bool _estaSalvando = false;

  @override
  void initState() {
    super.initState();
    _convites = widget.preferencias.notificacoesDeConviteAtivas;
    _lembretes = widget.preferencias.lembretesDeEncontroAtivos;
    _alteracoes = widget.preferencias.notificacoesDeAlteracaoAtivas;
    _combinados = widget.preferencias.notificacoesDeCombinadosAtivas;
  }

  @override
  Widget build(BuildContext context) {
    return ListView(
      children: <Widget>[
        CartaoDoAplicativo(
          preenchimento: EdgeInsets.zero,
          filho: Column(
            children: <Widget>[
              _Preferencia(
                titulo: 'Convites recebidos',
                descricao: 'Quando alguém chamar você para um encontro.',
                valor: _convites,
                aoAlterar: (bool valor) => setState(() => _convites = valor),
              ),
              const Divider(height: 1, indent: 16, endIndent: 16),
              _Preferencia(
                titulo: 'Lembretes de encontro',
                descricao: 'Avisos perto do horário combinado.',
                valor: _lembretes,
                aoAlterar: (bool valor) => setState(() => _lembretes = valor),
              ),
              const Divider(height: 1, indent: 16, endIndent: 16),
              _Preferencia(
                titulo: 'Alterações no encontro',
                descricao: 'Mudanças de data, local ou cancelamento.',
                valor: _alteracoes,
                aoAlterar: (bool valor) => setState(() => _alteracoes = valor),
              ),
              const Divider(height: 1, indent: 16, endIndent: 16),
              _Preferencia(
                titulo: 'Combinados atribuídos',
                descricao: 'Quando algo ficar sob sua responsabilidade.',
                valor: _combinados,
                aoAlterar: (bool valor) => setState(() => _combinados = valor),
              ),
            ],
          ),
        ),
        const SizedBox(height: EspacamentosDoAplicativo.padrao),
        FilledButton(
          key: const Key('salvar-preferencias-de-notificacao'),
          onPressed: _estaSalvando ? null : _salveAsync,
          child: Text(_estaSalvando ? 'Salvando...' : 'Salvar'),
        ),
      ],
    );
  }

  Future<void> _salveAsync() async {
    setState(() => _estaSalvando = true);

    try {
      PreferenciaDeNotificacao preferencias = PreferenciaDeNotificacao(
        notificacoesDeConviteAtivas: _convites,
        lembretesDeEncontroAtivos: _lembretes,
        notificacoesDeAlteracaoAtivas: _alteracoes,
        notificacoesDeCombinadosAtivas: _combinados,
      );
      await ref
          .read(provedorDoRepositorioDeNotificacoes)
          .atualizePreferenciasAsync(preferencias);
      ref.invalidate(provedorDasPreferenciasDeNotificacao);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Preferências salvas.')),
        );
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Não foi possível salvar suas preferências.'),
          ),
        );
      }
    } finally {
      if (mounted) {
        setState(() => _estaSalvando = false);
      }
    }
  }
}

class _Preferencia extends StatelessWidget {
  const _Preferencia({
    required this.titulo,
    required this.descricao,
    required this.valor,
    required this.aoAlterar,
  });

  final String titulo;
  final String descricao;
  final bool valor;
  final ValueChanged<bool> aoAlterar;

  @override
  Widget build(BuildContext context) {
    return SwitchListTile.adaptive(
      contentPadding: const EdgeInsets.symmetric(
        horizontal: EspacamentosDoAplicativo.padrao,
        vertical: EspacamentosDoAplicativo.pequeno,
      ),
      title: Text(titulo),
      subtitle: Text(descricao),
      value: valor,
      onChanged: aoAlterar,
    );
  }
}
