import 'package:flutter/material.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cabecalho_da_pagina.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cartao_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/conteudo_responsivo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estado_vazio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';

class TelaInicial extends StatelessWidget {
  const TelaInicial({super.key});

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
            titulo: 'Início',
            subtitulo: 'Um resumo simples do que vocês viveram juntos.',
          ),
          SizedBox(height: EspacamentosDoAplicativo.extraGrande),
          Expanded(
            child: Center(
              child: CartaoDoAplicativo(
                filho: EstadoVazio(
                  icone: Icons.event_available_outlined,
                  titulo: 'Seus encontros aparecerão aqui',
                  descricao:
                      'Quando um encontro for criado ou reconhecido em um '
                      'grupo, você verá os registros mais recentes neste espaço.',
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
