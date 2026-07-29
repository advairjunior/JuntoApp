import 'package:flutter/material.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/sombras_do_aplicativo.dart';

class EstruturaResponsivaDoAplicativo extends StatelessWidget {
  const EstruturaResponsivaDoAplicativo({
    required this.filho,
    this.corDoConteudo = CoresDoAplicativo.fundoPrincipal,
    super.key,
  });

  final Widget filho;
  final Color corDoConteudo;

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (BuildContext context, BoxConstraints limites) {
        bool ehDesktop = limites.maxWidth > 700;

        return ColoredBox(
          color: CoresDoAplicativo.fundoExterno,
          child: Center(
            child: Container(
              width: double.infinity,
              constraints: const BoxConstraints(
                maxWidth: EspacamentosDoAplicativo.larguraMaximaDoConteudo,
              ),
              decoration: BoxDecoration(
                color: corDoConteudo,
                border: ehDesktop
                    ? const Border.symmetric(
                        vertical: BorderSide(
                          color: CoresDoAplicativo.bordaDiscreta,
                        ),
                      )
                    : null,
                boxShadow: ehDesktop ? SombrasDoAplicativo.elevada : null,
              ),
              child: filho,
            ),
          ),
        );
      },
    );
  }
}
