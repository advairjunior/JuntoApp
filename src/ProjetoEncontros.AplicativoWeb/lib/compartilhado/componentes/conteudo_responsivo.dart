import 'package:flutter/material.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estrutura_responsiva_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';

class ConteudoResponsivo extends StatelessWidget {
  const ConteudoResponsivo({
    required this.filho,
    this.preenchimento = const EdgeInsets.symmetric(
      horizontal: EspacamentosDoAplicativo.padrao,
      vertical: EspacamentosDoAplicativo.grande,
    ),
    super.key,
  });

  final Widget filho;
  final EdgeInsetsGeometry preenchimento;

  @override
  Widget build(BuildContext context) {
    return EstruturaResponsivaDoAplicativo(
      filho: Padding(padding: preenchimento, child: filho),
    );
  }
}
