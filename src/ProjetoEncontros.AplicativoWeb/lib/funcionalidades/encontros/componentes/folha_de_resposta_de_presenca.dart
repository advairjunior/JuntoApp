import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';

Future<String?> mostreFolhaDeRespostaDePresencaAsync(
  BuildContext context,
) {
  return showModalBottomSheet<String>(
    context: context,
    backgroundColor: CoresDoAplicativo.fundoDoCartao,
    showDragHandle: true,
    isScrollControlled: true,
    builder: (BuildContext context) {
      return SafeArea(
        child: SingleChildScrollView(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(20, 0, 20, 20),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: <Widget>[
                Text(
                  'Você vai participar?',
                  style: Theme.of(context).textTheme.titleLarge,
                ),
                const SizedBox(height: EspacamentosDoAplicativo.minimo),
                const Text(
                  'Sua resposta será visível para os outros participantes.',
                  style: TextStyle(color: CoresDoAplicativo.textoSecundario),
                ),
                const SizedBox(height: EspacamentosDoAplicativo.padrao),
                _OpcaoDePresenca(
                  chave: const Key('presenca-confirmado'),
                  titulo: 'Vou',
                  descricao: 'Estarei presente neste encontro.',
                  icone: Icons.check_circle_outline_rounded,
                  cor: CoresDoAplicativo.verdeDestaque,
                  aoPressionar: () => context.pop('Confirmado'),
                ),
                _OpcaoDePresenca(
                  chave: const Key('presenca-talvez'),
                  titulo: 'Talvez',
                  descricao: 'Ainda não tenho certeza.',
                  icone: Icons.help_outline_rounded,
                  cor: CoresDoAplicativo.ambar,
                  aoPressionar: () => context.pop('Talvez'),
                ),
                _OpcaoDePresenca(
                  chave: const Key('presenca-nao-vai'),
                  titulo: 'Não vou',
                  descricao: 'Não poderei participar.',
                  icone: Icons.cancel_outlined,
                  cor: CoresDoAplicativo.coral,
                  aoPressionar: () => context.pop('NaoVai'),
                ),
              ],
            ),
          ),
        ),
      );
    },
  );
}

class _OpcaoDePresenca extends StatelessWidget {
  const _OpcaoDePresenca({
    required this.chave,
    required this.titulo,
    required this.descricao,
    required this.icone,
    required this.cor,
    required this.aoPressionar,
  });

  final Key chave;
  final String titulo;
  final String descricao;
  final IconData icone;
  final Color cor;
  final VoidCallback aoPressionar;

  @override
  Widget build(BuildContext context) {
    return ListTile(
      key: chave,
      onTap: aoPressionar,
      contentPadding: EdgeInsets.zero,
      leading: Icon(icone, color: cor, size: 30),
      title: Text(titulo),
      subtitle: Text(descricao),
    );
  }
}
