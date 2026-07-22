import 'package:flutter/material.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/raios_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/sombras_do_aplicativo.dart';

class CartaoDoAplicativo extends StatelessWidget {
  const CartaoDoAplicativo({
    required this.filho,
    this.preenchimento = const EdgeInsets.all(
      EspacamentosDoAplicativo.padrao,
    ),
    this.elevado = false,
    this.aoTocar,
    super.key,
  });

  final Widget filho;
  final EdgeInsetsGeometry preenchimento;
  final bool elevado;
  final VoidCallback? aoTocar;

  @override
  Widget build(BuildContext context) {
    BorderRadius raio = BorderRadius.circular(RaiosDoAplicativo.grande);
    Widget conteudo = Padding(padding: preenchimento, child: filho);

    if (aoTocar != null) {
      conteudo = InkWell(
        onTap: aoTocar,
        borderRadius: raio,
        child: conteudo,
      );
    }

    return DecoratedBox(
      decoration: BoxDecoration(
        color: elevado
            ? CoresDoAplicativo.fundoElevado
            : CoresDoAplicativo.fundoDoCartao,
        borderRadius: raio,
        border: Border.all(color: CoresDoAplicativo.bordaDiscreta),
        boxShadow: elevado ? SombrasDoAplicativo.baixa : null,
      ),
      child: ClipRRect(borderRadius: raio, child: conteudo),
    );
  }
}
