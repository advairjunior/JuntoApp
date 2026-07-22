import 'package:flutter/material.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';

class CabecalhoDaPagina extends StatelessWidget {
  const CabecalhoDaPagina({
    required this.titulo,
    this.subtitulo,
    this.inicio,
    this.acoes = const <Widget>[],
    super.key,
  });

  final String titulo;
  final String? subtitulo;
  final Widget? inicio;
  final List<Widget> acoes;

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.center,
      children: <Widget>[
        if (inicio != null) ...<Widget>[
          inicio!,
          const SizedBox(width: EspacamentosDoAplicativo.medio),
        ],
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: <Widget>[
              Text(
                titulo,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: Theme.of(context).textTheme.headlineSmall,
              ),
              if (subtitulo != null) ...<Widget>[
                const SizedBox(height: EspacamentosDoAplicativo.minimo),
                Text(
                  subtitulo!,
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    color: CoresDoAplicativo.textoSecundario,
                  ),
                ),
              ],
            ],
          ),
        ),
        if (acoes.isNotEmpty) ...<Widget>[
          const SizedBox(width: EspacamentosDoAplicativo.pequeno),
          ...acoes,
        ],
      ],
    );
  }
}
