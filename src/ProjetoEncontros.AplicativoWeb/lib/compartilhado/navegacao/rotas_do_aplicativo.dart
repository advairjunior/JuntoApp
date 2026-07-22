import 'package:flutter/widgets.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/controlador_de_sessao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/estado_da_sessao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/navegacao/estrutura_com_navegacao.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/combinados/telas/tela_de_combinados.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/entrada/telas/tela_de_cadastro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/entrada/telas/tela_de_entrada.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/entrada/telas/tela_de_inicializacao.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/telas/tela_de_criacao_de_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/telas/tela_de_detalhe_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/telas/tela_de_participantes_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/telas/tela_inicial.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/telas/tela_de_memorias.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/telas/tela_de_midias_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/notificacoes/telas/tela_de_notificacoes.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/notificacoes/telas/tela_de_preferencias_de_notificacao.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/perfil/telas/tela_de_perfil.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/publicacoes/telas/tela_de_momentos_do_encontro.dart';

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
      String caminho = estadoDaRota.uri.path;
      bool rotaEhPublica = caminho == '/entrada' || caminho == '/cadastro';
      bool rotaEhInicializacao = caminho == '/inicializacao';

      if (sessao.situacao == SituacaoDaSessao.restaurando) {
        return rotaEhInicializacao ? null : '/inicializacao';
      }

      if (!sessao.usuarioEstaAutenticado) {
        if (rotaEhPublica) {
          return null;
        }

        String retorno = Uri.encodeComponent(estadoDaRota.uri.toString());
        return '/entrada?retorno=$retorno';
      }

      if (rotaEhPublica || rotaEhInicializacao) {
        String? retorno = estadoDaRota.uri.queryParameters['retorno'];

        if (retorno != null &&
            retorno.startsWith('/') &&
            !retorno.startsWith('/entrada')) {
          return retorno;
        }

        return '/inicio';
      }

      return null;
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
          );
        },
      ),
      GoRoute(
        path: '/cadastro',
        builder: (BuildContext context, GoRouterState estado) {
          return const TelaDeCadastro();
        },
      ),
      GoRoute(
        path: '/encontros/novo',
        builder: (BuildContext context, GoRouterState estado) {
          return const TelaDeCriacaoDeEncontro();
        },
      ),
      GoRoute(
        path: '/encontros/:identificadorDoEncontro/editar',
        builder: (BuildContext context, GoRouterState estado) {
          return TelaDeCriacaoDeEncontro(
            identificadorDoEncontro:
                estado.pathParameters['identificadorDoEncontro']!,
          );
        },
      ),
      GoRoute(
        path: '/encontros/:identificadorDoEncontro',
        builder: (BuildContext context, GoRouterState estado) {
          return TelaDeMomentosDoEncontro(
            identificadorDoEncontro:
                estado.pathParameters['identificadorDoEncontro']!,
            soliciteRespostaDePresenca:
                estado.uri.queryParameters['responder-presenca'] == 'true',
          );
        },
      ),
      GoRoute(
        path: '/encontros/:identificadorDoEncontro/informacoes',
        builder: (BuildContext context, GoRouterState estado) {
          return TelaDeDetalheDoEncontro(
            identificadorDoEncontro:
                estado.pathParameters['identificadorDoEncontro']!,
          );
        },
      ),
      GoRoute(
        path: '/encontros/:identificadorDoEncontro/participantes',
        builder: (BuildContext context, GoRouterState estado) {
          return TelaDeParticipantesDoEncontro(
            identificadorDoEncontro:
                estado.pathParameters['identificadorDoEncontro']!,
          );
        },
      ),
      GoRoute(
        path: '/encontros/:identificadorDoEncontro/midias',
        builder: (BuildContext context, GoRouterState estado) {
          return TelaDeMidiasDoEncontro(
            identificadorDoEncontro:
                estado.pathParameters['identificadorDoEncontro']!,
          );
        },
      ),
      GoRoute(
        path: '/encontros/:identificadorDoEncontro/combinados',
        builder: (BuildContext context, GoRouterState estado) {
          return TelaDeCombinados(
            identificadorDoEncontro:
                estado.pathParameters['identificadorDoEncontro']!,
          );
        },
      ),
      GoRoute(
        path: '/notificacoes',
        builder: (BuildContext context, GoRouterState estado) {
          return const TelaDeNotificacoes();
        },
      ),
      GoRoute(
        path: '/perfil/notificacoes',
        builder: (BuildContext context, GoRouterState estado) {
          return const TelaDePreferenciasDeNotificacao();
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
            path: '/memorias',
            builder: (BuildContext context, GoRouterState estado) {
              return const TelaDeMemorias();
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

class NotificadorDeRotas extends ChangeNotifier {
  void notifique() {
    notifyListeners();
  }
}
