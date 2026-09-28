import 'package:flutter/material.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/foto_de_perfil.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/participante_do_encontro.dart';

Future<List<String>?> mostreSeletorDePessoasNaMidiaAsync(
  BuildContext context, {
  required List<ParticipanteDoEncontro> participantes,
  required Set<String> identificadoresSelecionados,
}) {
  return showModalBottomSheet<List<String>>(
    context: context,
    isScrollControlled: true,
    useSafeArea: true,
    builder: (BuildContext context) => _SeletorDePessoasNaMidia(
      participantes: participantes,
      identificadoresSelecionados: identificadoresSelecionados,
    ),
  );
}

class _SeletorDePessoasNaMidia extends StatefulWidget {
  const _SeletorDePessoasNaMidia({
    required this.participantes,
    required this.identificadoresSelecionados,
  });

  final List<ParticipanteDoEncontro> participantes;
  final Set<String> identificadoresSelecionados;

  @override
  State<_SeletorDePessoasNaMidia> createState() =>
      _EstadoDoSeletorDePessoasNaMidia();
}

class _EstadoDoSeletorDePessoasNaMidia extends State<_SeletorDePessoasNaMidia> {
  late final Set<String> _selecionados;
  String _busca = '';

  @override
  void initState() {
    super.initState();
    _selecionados = Set<String>.from(widget.identificadoresSelecionados);
  }

  @override
  Widget build(BuildContext context) {
    String buscaNormalizada = _busca.trim().toLowerCase();
    List<ParticipanteDoEncontro> participantes = widget.participantes
        .where(
          (ParticipanteDoEncontro participante) =>
              buscaNormalizada.isEmpty ||
              participante.nome.toLowerCase().contains(buscaNormalizada),
        )
        .toList()
      ..sort(
        (ParticipanteDoEncontro primeira, ParticipanteDoEncontro segunda) =>
            primeira.nome.toLowerCase().compareTo(segunda.nome.toLowerCase()),
      );

    return FractionallySizedBox(
      heightFactor: 0.82,
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
                    'Marcar pessoas',
                    style: Theme.of(context).textTheme.titleLarge,
                  ),
                ),
                TextButton(
                  key: const Key('concluir-marcacao-de-pessoas'),
                  onPressed: () => Navigator.of(context).pop(
                    _selecionados.toList(),
                  ),
                  child: const Text('Concluir'),
                ),
              ],
            ),
          ),
          Padding(
            padding: const EdgeInsets.symmetric(
              horizontal: EspacamentosDoAplicativo.padrao,
            ),
            child: TextField(
              key: const Key('buscar-pessoa-para-marcar'),
              autofocus: false,
              textInputAction: TextInputAction.search,
              decoration: const InputDecoration(
                hintText: 'Buscar participante',
                prefixIcon: Icon(Icons.search_rounded),
              ),
              onChanged: (String valor) {
                setState(() {
                  _busca = valor;
                });
              },
            ),
          ),
          const SizedBox(height: EspacamentosDoAplicativo.pequeno),
          Expanded(
            child: participantes.isEmpty
                ? const Center(
                    child: Text('Nenhum participante encontrado.'),
                  )
                : ListView.builder(
                    keyboardDismissBehavior:
                        ScrollViewKeyboardDismissBehavior.onDrag,
                    itemCount: participantes.length,
                    itemBuilder: (BuildContext context, int indice) {
                      ParticipanteDoEncontro participante =
                          participantes[indice];
                      bool estaSelecionado = _selecionados.contains(
                        participante.identificadorDoUsuario,
                      );

                      return CheckboxListTile(
                        key: Key(
                          'marcar-pessoa-${participante.identificadorDoUsuario}',
                        ),
                        value: estaSelecionado,
                        controlAffinity: ListTileControlAffinity.trailing,
                        secondary: FotoDePerfil(
                          url: participante.urlDaFotoDePerfil,
                          iniciais: participante.iniciais,
                          dimensao: 40,
                        ),
                        title: Text(participante.nome),
                        onChanged: (bool? valor) {
                          setState(() {
                            if (valor ?? false) {
                              _selecionados.add(
                                participante.identificadorDoUsuario,
                              );
                            } else {
                              _selecionados.remove(
                                participante.identificadorDoUsuario,
                              );
                            }
                          });
                        },
                      );
                    },
                  ),
          ),
        ],
      ),
    );
  }
}
