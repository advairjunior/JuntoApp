import 'package:flutter/widgets.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/controlador_de_sessao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/estado_da_sessao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/navegacao/estrutura_com_navegacao.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/entrada/telas/tela_de_cadastro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/entrada/telas/tela_de_entrada.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/entrada/telas/tela_de_inicializacao.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/grupos/telas/tela_de_grupos.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/telas/tela_inicial.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/perfil/telas/tela_de_perfil.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/pessoas_frequentes/telas/tela_de_pessoas.dart';

final provedorDasRotas = Provider<GoRouter>((Ref referencia) {
  NotificadorDeRotas notificador = NotificadorDeRotas();

  referencia.listen<EstadoDaSessao>(
    provedorDoControladorDeSessao,
    (EstadoDaSessao? estadoAnterior, EstadoDaSessao novoEstado) {
      notificador.notifique();
    },
  );
  referencia.onDispose(notificador.dispose);

  return GoRouter(
    initialLocation: '/inicializacao',
    refreshListenable: notificador,
    redirect: (BuildContext context, GoRouterState estadoDaRota) {
      EstadoDaSessao sessao = referencia.read(provedorDoControladorDeSessao);
      return redirecioneRota(
        sessao: sessao,
        enderecoDaRota: estadoDaRota.uri,
      );
    },
    routes: <RouteBase>[
      GoRoute(
        path: '/inicializacao',
        builder: (BuildContext context, GoRouterState estado) {
          return const TelaDeInicializacao();
        },
      ),
      GoRoute(
        path: '/entrada',
        builder: (BuildContext context, GoRouterState estado) {
          return TelaDeEntrada(
            cadastroFoiConcluido:
                estado.uri.queryParameters['cadastro'] == 'concluido',
            retorno: estado.uri.queryParameters['retorno'],
          );
        },
      ),
      GoRoute(
        path: '/cadastro',
        builder: (BuildContext context, GoRouterState estado) {
          return TelaDeCadastro(
            retorno: estado.uri.queryParameters['retorno'],
          );
        },
      ),
      ShellRoute(
        builder: (
          BuildContext context,
          GoRouterState estado,
          Widget filho,
        ) {
          return EstruturaComNavegacao(
            caminhoAtual: estado.uri.path,
            filho: filho,
          );
        },
        routes: <RouteBase>[
          GoRoute(
            path: '/inicio',
            builder: (BuildContext context, GoRouterState estado) {
              return const TelaInicial();
            },
          ),
          GoRoute(
            path: '/grupos',
            builder: (BuildContext context, GoRouterState estado) {
              return const TelaDeGrupos();
            },
          ),
          GoRoute(
            path: '/pessoas',
            builder: (BuildContext context, GoRouterState estado) {
              return const TelaDePessoas();
            },
          ),
          GoRoute(
            path: '/perfil',
            builder: (BuildContext context, GoRouterState estado) {
              return const TelaDePerfil();
            },
          ),
        ],
      ),
    ],
  );
});

String? redirecioneRota({
  required EstadoDaSessao sessao,
  required Uri enderecoDaRota,
}) {
  String caminho = enderecoDaRota.path;
  bool rotaEhPublica = caminho == '/entrada' || caminho == '/cadastro';
  bool rotaEhInicializacao = caminho == '/inicializacao';
  String? retorno = _obtenhaRetornoValido(enderecoDaRota);

  if (sessao.situacao == SituacaoDaSessao.restaurando) {
    if (rotaEhInicializacao) {
      return null;
    }

    if (rotaEhPublica) {
      return retorno == null
          ? '/inicializacao'
          : Uri(
              path: '/inicializacao',
              queryParameters: <String, String>{'retorno': retorno},
            ).toString();
    }

    return Uri(
      path: '/inicializacao',
      queryParameters: <String, String>{
        'retorno': enderecoDaRota.toString(),
      },
    ).toString();
  }

  if (!sessao.usuarioEstaAutenticado) {
    if (rotaEhPublica) {
      return null;
    }

    String? destinoAposEntrada =
        rotaEhInicializacao ? retorno : enderecoDaRota.toString();

    if (!_retornoEhValido(destinoAposEntrada)) {
      return '/entrada';
    }

    return Uri(
      path: '/entrada',
      queryParameters: <String, String>{'retorno': destinoAposEntrada!},
    ).toString();
  }

  if (rotaEhPublica || rotaEhInicializacao) {
    return retorno ?? '/inicio';
  }

  if (!_rotaAutenticadaExiste(caminho)) {
    return '/inicio';
  }

  return null;
}

String? _obtenhaRetornoValido(Uri enderecoDaRota) {
  String? retorno = enderecoDaRota.queryParameters['retorno'];
  return _retornoEhValido(retorno) ? retorno : null;
}

bool _retornoEhValido(String? retorno) {
  if (retorno == null || !retorno.startsWith('/')) {
    return false;
  }

  return _rotaAutenticadaExiste(Uri.parse(retorno).path);
}

bool _rotaAutenticadaExiste(String caminho) {
  return caminho == '/inicio' ||
      caminho == '/grupos' ||
      caminho == '/pessoas' ||
      caminho == '/perfil';
}

class NotificadorDeRotas extends ChangeNotifier {
  void notifique() {
    notifyListeners();
  }
}
