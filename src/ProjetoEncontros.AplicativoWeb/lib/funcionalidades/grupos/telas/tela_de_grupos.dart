import 'package:flutter/material.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cabecalho_da_pagina.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cartao_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/conteudo_responsivo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estado_vazio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';

class TelaDeGrupos extends StatelessWidget {
  const TelaDeGrupos({super.key});

  @override
  Widget build(BuildContext context) {
    return const ConteudoResponsivo(
      preenchimento: EdgeInsets.fromLTRB(
        EspacamentosDoAplicativo.padrao,
        EspacamentosDoAplicativo.grande,
        EspacamentosDoAplicativo.padrao,
        EspacamentosDoAplicativo.grande,
      ),
      filho: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: <Widget>[
          CabecalhoDaPagina(
            titulo: 'Grupos',
            subtitulo: 'Os círculos de amizade que dão origem aos encontros.',
          ),
          SizedBox(height: EspacamentosDoAplicativo.extraGrande),
          Expanded(
            child: Center(
              child: CartaoDoAplicativo(
                filho: EstadoVazio(
                  icone: Icons.groups_outlined,
                  titulo: 'Seus grupos aparecerão aqui',
                  descricao:
                      'Na próxima etapa você poderá criar um grupo ou importar '
                      'o histórico de uma conversa do WhatsApp.',
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
