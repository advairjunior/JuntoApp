import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cabecalho_da_pagina.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/conteudo_responsivo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estado_vazio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/foto_de_perfil.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/raios_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/pessoas_frequentes/dados/repositorio_de_pessoas_frequentes.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/pessoas_frequentes/modelos/pessoa_frequente.dart';

class TelaDePessoas extends ConsumerStatefulWidget {
  const TelaDePessoas({super.key});

  @override
  ConsumerState<TelaDePessoas> createState() => _EstadoDaTelaDePessoas();
}

class _EstadoDaTelaDePessoas extends ConsumerState<TelaDePessoas> {
  final TextEditingController _controladorDaBusca = TextEditingController();
  String _termoDaBusca = '';

  @override
  void dispose() {
    _controladorDaBusca.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    AsyncValue<List<PessoaFrequente>> pessoas =
        ref.watch(provedorDasPessoasFrequentes);

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
            titulo: 'Pessoas',
            subtitulo: 'Quem já compartilhou encontros com você.',
          ),
          const SizedBox(height: EspacamentosDoAplicativo.grande),
          TextField(
            key: const Key('buscar-pessoa'),
            controller: _controladorDaBusca,
            textInputAction: TextInputAction.search,
            onChanged: (String valor) {
              setState(() {
                _termoDaBusca = valor.trim().toLowerCase();
              });
            },
            decoration: InputDecoration(
              hintText: 'Buscar por nome',
              prefixIcon: const Icon(Icons.search_rounded),
              suffixIcon: _termoDaBusca.isEmpty
                  ? null
                  : IconButton(
                      key: const Key('limpar-busca-de-pessoa'),
                      tooltip: 'Limpar busca',
                      onPressed: _limpeBusca,
                      icon: const Icon(Icons.close_rounded),
                    ),
            ),
          ),
          const SizedBox(height: EspacamentosDoAplicativo.grande),
          Expanded(
            child: pessoas.when(
              loading: () => const Center(child: CircularProgressIndicator()),
              error: (_, __) => _ErroAoCarregarPessoas(
                aoTentarNovamente: () =>
                    ref.invalidate(provedorDasPessoasFrequentes),
              ),
              data: _construaPessoas,
            ),
          ),
        ],
      ),
    );
  }

  Widget _construaPessoas(List<PessoaFrequente> pessoas) {
    if (pessoas.isEmpty) {
      return const Center(
        child: EstadoVazio(
          icone: Icons.people_outline_rounded,
          titulo: 'Suas pessoas aparecerão aqui',
          descricao:
              'Depois de compartilhar encontros, você poderá encontrá-las '
              'e convidá-las novamente com facilidade.',
        ),
      );
    }

    List<PessoaFrequente> pessoasFiltradas = pessoas
        .where(
          (PessoaFrequente pessoa) =>
              _termoDaBusca.isEmpty ||
              pessoa.nome.toLowerCase().contains(_termoDaBusca),
        )
        .toList();

    if (pessoasFiltradas.isEmpty) {
      return const Center(
        child: EstadoVazio(
          icone: Icons.person_search_outlined,
          titulo: 'Nenhuma pessoa encontrada',
          descricao: 'Tente buscar por outro nome.',
        ),
      );
    }

    return RefreshIndicator(
      onRefresh: _atualizeAsync,
      child: ListView.separated(
        key: const Key('lista-de-pessoas'),
        physics: const AlwaysScrollableScrollPhysics(),
        itemCount: pessoasFiltradas.length,
        separatorBuilder: (_, __) =>
            const SizedBox(height: EspacamentosDoAplicativo.pequeno),
        itemBuilder: (BuildContext context, int indice) {
          PessoaFrequente pessoa = pessoasFiltradas[indice];

          return _PessoaConhecida(
            pessoa: pessoa,
            aoAbrir: () => _abraPessoaAsync(pessoa),
          );
        },
      ),
    );
  }

  void _limpeBusca() {
    _controladorDaBusca.clear();
    setState(() {
      _termoDaBusca = '';
    });
  }

  Future<void> _atualizeAsync() async {
    ref.invalidate(provedorDasPessoasFrequentes);
    await ref.read(provedorDasPessoasFrequentes.future);
  }

  Future<void> _abraPessoaAsync(PessoaFrequente pessoa) async {
    await context.push<void>(
      '/pessoas/${pessoa.identificadorDoUsuario}',
    );
  }
}

class _PessoaConhecida extends StatelessWidget {
  const _PessoaConhecida({
    required this.pessoa,
    required this.aoAbrir,
  });

  final PessoaFrequente pessoa;
  final VoidCallback aoAbrir;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: CoresDoAplicativo.fundoDoCartao,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(RaiosDoAplicativo.medio),
        side: const BorderSide(color: CoresDoAplicativo.bordaDiscreta),
      ),
      clipBehavior: Clip.antiAlias,
      child: InkWell(
        key: Key('abrir-pessoa-${pessoa.identificadorDoUsuario}'),
        onTap: aoAbrir,
        child: Padding(
          padding: const EdgeInsets.all(EspacamentosDoAplicativo.padrao),
          child: Row(
            children: <Widget>[
              FotoDePerfil(
                url: pessoa.urlDaFotoDePerfil,
                iniciais: pessoa.iniciais,
                dimensao: 52,
                tamanhoDasIniciais: 18,
              ),
              const SizedBox(width: EspacamentosDoAplicativo.medio),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: <Widget>[
                    Text(
                      pessoa.nome,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                            fontWeight: FontWeight.w700,
                          ),
                    ),
                    const SizedBox(height: EspacamentosDoAplicativo.minimo),
                    Text(
                      _descrevaUltimaVez(pessoa.ultimoEncontroEm),
                      style: const TextStyle(
                        color: CoresDoAplicativo.textoSecundario,
                      ),
                    ),
                    const SizedBox(height: 2),
                    Text(
                      '${pessoa.textoDaRecorrencia} · '
                      '${DateFormat('dd MMM yyyy', 'pt_BR').format(pessoa.ultimoEncontroEm)}',
                      style: const TextStyle(
                        color: CoresDoAplicativo.textoTerciario,
                        fontSize: 12,
                      ),
                    ),
                    if (pessoa.proximoEncontroEm != null) ...<Widget>[
                      const SizedBox(height: EspacamentosDoAplicativo.pequeno),
                      DecoratedBox(
                        decoration: BoxDecoration(
                          color: CoresDoAplicativo.fundoDaInteracao,
                          borderRadius: BorderRadius.circular(
                            RaiosDoAplicativo.pilula,
                          ),
                        ),
                        child: Padding(
                          padding: const EdgeInsets.symmetric(
                            horizontal: EspacamentosDoAplicativo.pequeno,
                            vertical: 4,
                          ),
                          child: Text(
                            'Próximo em '
                            '${DateFormat('dd MMM', 'pt_BR').format(pessoa.proximoEncontroEm!)}',
                            style: const TextStyle(
                              color: CoresDoAplicativo.azulInteracao,
                              fontSize: 12,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),
                      ),
                    ],
                  ],
                ),
              ),
              const SizedBox(width: EspacamentosDoAplicativo.pequeno),
              const Icon(
                Icons.chevron_right_rounded,
                color: CoresDoAplicativo.textoTerciario,
              ),
            ],
          ),
        ),
      ),
    );
  }

  String _descrevaUltimaVez(DateTime data) {
    DateTime hoje = DateTime.now();
    DateTime diaAtual = DateTime(hoje.year, hoje.month, hoje.day);
    DateTime diaDoEncontro = DateTime(data.year, data.month, data.day);
    int dias = diaAtual.difference(diaDoEncontro).inDays;

    if (dias <= 0) {
      return 'Última vez hoje';
    }

    if (dias == 1) {
      return 'Última vez ontem';
    }

    if (dias < 30) {
      return 'Última vez há $dias dias';
    }

    int meses = (dias / 30).floor();

    if (meses == 1) {
      return 'Última vez há 1 mês';
    }

    if (meses < 12) {
      return 'Última vez há $meses meses';
    }

    int anos = (dias / 365).floor();
    return anos == 1 ? 'Última vez há 1 ano' : 'Última vez há $anos anos';
  }
}

class _ErroAoCarregarPessoas extends StatelessWidget {
  const _ErroAoCarregarPessoas({required this.aoTentarNovamente});

  final VoidCallback aoTentarNovamente;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: EstadoVazio(
        icone: Icons.cloud_off_outlined,
        titulo: 'Não foi possível carregar as pessoas',
        descricao: 'Verifique sua conexão e tente novamente.',
        acao: TextButton(
          onPressed: aoTentarNovamente,
          child: const Text('Tentar novamente'),
        ),
      ),
    );
  }
}
