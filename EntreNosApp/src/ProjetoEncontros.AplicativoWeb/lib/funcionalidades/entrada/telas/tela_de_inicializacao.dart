import 'package:flutter/material.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';

class TelaDeInicializacao extends StatelessWidget {
  const TelaDeInicializacao({super.key});

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      body: Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: <Widget>[
            Icon(
              Icons.people_alt_rounded,
              size: 56,
              color: CoresDoAplicativo.verdeDestaque,
            ),
            SizedBox(height: 20),
            Text(
              'Juntô',
              style: TextStyle(
                color: CoresDoAplicativo.textoPrincipal,
                fontSize: 32,
                fontWeight: FontWeight.w700,
              ),
            ),
            SizedBox(height: 24),
            SizedBox(
              width: 24,
              height: 24,
              child: CircularProgressIndicator(strokeWidth: 2),
            ),
          ],
        ),
      ),
    );
  }
}
