import 'package:flutter/material.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';

class TituloDeSecao extends StatelessWidget {
  const TituloDeSecao({
    required this.titulo,
    this.subtitulo,
    this.acao,
    super.key,
  });

  final String titulo;
  final String? subtitulo;
  final Widget? acao;

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.end,
      children: <Widget>[
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: <Widget>[
              Text(titulo, style: Theme.of(context).textTheme.titleLarge),
              if (subtitulo != null) ...<Widget>[
                const SizedBox(height: EspacamentosDoAplicativo.minimo),
                Text(
                  subtitulo!,
                  style: const TextStyle(
                    color: CoresDoAplicativo.textoTerciario,
                    fontSize: 13,
                  ),
                ),
              ],
            ],
          ),
        ),
        if (acao != null) ...<Widget>[
          const SizedBox(width: EspacamentosDoAplicativo.medio),
          acao!,
        ],
      ],
    );
  }
}
