import 'dart:ui';

import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estrutura_responsiva_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/raios_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/sombras_do_aplicativo.dart';

class EstruturaComNavegacao extends StatelessWidget {
  const EstruturaComNavegacao({
    required this.caminhoAtual,
    required this.filho,
    super.key,
  });

  final String caminhoAtual;
  final Widget filho;

  int get _indiceSelecionado {
    if (caminhoAtual.startsWith('/memorias')) {
      return 1;
    }

    if (caminhoAtual.startsWith('/perfil')) {
      return 2;
    }

    return 0;
  }

  @override
  Widget build(BuildContext context) {
    double alturaReservada = EspacamentosDoAplicativo.alturaDoDock +
        (EspacamentosDoAplicativo.margemDoDock * 2) +
        MediaQuery.paddingOf(context).bottom;

    return Scaffold(
      backgroundColor: CoresDoAplicativo.fundoExterno,
      body: EstruturaResponsivaDoAplicativo(
        filho: Stack(
          children: <Widget>[
            Positioned.fill(
              bottom: alturaReservada,
              child: SafeArea(
                bottom: false,
                child: filho,
              ),
            ),
            Positioned(
              left: EspacamentosDoAplicativo.margemDoDock,
              right: EspacamentosDoAplicativo.margemDoDock,
              bottom: EspacamentosDoAplicativo.margemDoDock,
              child: SafeArea(
                top: false,
                minimum: const EdgeInsets.only(
                  bottom: EspacamentosDoAplicativo.minimo,
                ),
                child: _DockDeNavegacao(
                  indiceSelecionado: _indiceSelecionado,
                  aoSelecionar: (int indice) => _navegue(context, indice),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _navegue(BuildContext context, int indice) {
    String destino = switch (indice) {
      0 => '/inicio',
      1 => '/memorias',
      _ => '/perfil',
    };

    if (destino != caminhoAtual) {
      context.go(destino);
    }
  }
}

class _DockDeNavegacao extends StatelessWidget {
  const _DockDeNavegacao({
    required this.indiceSelecionado,
    required this.aoSelecionar,
  });

  final int indiceSelecionado;
  final ValueChanged<int> aoSelecionar;

  @override
  Widget build(BuildContext context) {
    BorderRadius raio = BorderRadius.circular(RaiosDoAplicativo.extraGrande);

    return DecoratedBox(
      decoration: BoxDecoration(
        borderRadius: raio,
        boxShadow: SombrasDoAplicativo.elevada,
      ),
      child: ClipRRect(
        borderRadius: raio,
        child: BackdropFilter(
          filter: ImageFilter.blur(sigmaX: 18, sigmaY: 18),
          child: DecoratedBox(
            decoration: BoxDecoration(
              color: CoresDoAplicativo.fundoElevado.withValues(alpha: 0.9),
              borderRadius: raio,
              border: Border.all(color: CoresDoAplicativo.bordaSuave),
            ),
            child: SizedBox(
              height: EspacamentosDoAplicativo.alturaDoDock,
              child: NavigationBar(
                selectedIndex: indiceSelecionado,
                onDestinationSelected: aoSelecionar,
                destinations: const <NavigationDestination>[
                  NavigationDestination(
                    icon: Icon(Icons.home_outlined),
                    selectedIcon: Icon(Icons.home_rounded),
                    label: 'Início',
                  ),
                  NavigationDestination(
                    icon: Icon(Icons.photo_library_outlined),
                    selectedIcon: Icon(Icons.photo_library_rounded),
                    label: 'Memórias',
                  ),
                  NavigationDestination(
                    icon: Icon(Icons.person_outline_rounded),
                    selectedIcon: Icon(Icons.person_rounded),
                    label: 'Perfil',
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
