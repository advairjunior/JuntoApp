import 'package:flutter/material.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';

class EstadoVazio extends StatelessWidget {
  const EstadoVazio({
    required this.icone,
    required this.titulo,
    required this.descricao,
    this.acao,
    super.key,
  });

  final IconData icone;
  final String titulo;
  final String descricao;
  final Widget? acao;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(
        horizontal: EspacamentosDoAplicativo.grande,
        vertical: EspacamentosDoAplicativo.extraGrande,
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: <Widget>[
          Icon(
            icone,
            size: 42,
            color: CoresDoAplicativo.azulInteracao,
          ),
          const SizedBox(height: EspacamentosDoAplicativo.padrao),
          Text(
            titulo,
            textAlign: TextAlign.center,
            style: Theme.of(context).textTheme.titleMedium,
          ),
          const SizedBox(height: EspacamentosDoAplicativo.pequeno),
          Text(
            descricao,
            textAlign: TextAlign.center,
            style: const TextStyle(color: CoresDoAplicativo.textoSecundario),
          ),
          if (acao != null) ...<Widget>[
            const SizedBox(height: EspacamentosDoAplicativo.grande),
            acao!,
          ],
        ],
      ),
    );
  }
}
