import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/navegacao/rotas_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/tema_do_aplicativo.dart';

class Aplicativo extends ConsumerWidget {
  const Aplicativo({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final configuracaoDasRotas = ref.watch(provedorDasRotas);

    return MaterialApp.router(
      title: 'Juntô',
      debugShowCheckedModeBanner: false,
      locale: const Locale('pt', 'BR'),
      supportedLocales: const <Locale>[Locale('pt', 'BR')],
      localizationsDelegates: const <LocalizationsDelegate<dynamic>>[
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      theme: TemaDoAplicativo.escuro,
      routerConfig: configuracaoDasRotas,
    );
  }
}
