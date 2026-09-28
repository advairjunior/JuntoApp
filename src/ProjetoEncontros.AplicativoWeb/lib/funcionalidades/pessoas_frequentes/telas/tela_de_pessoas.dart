import 'package:flutter/material.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cabecalho_da_pagina.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cartao_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/conteudo_responsivo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estado_vazio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';

class TelaDePessoas extends StatelessWidget {
  const TelaDePessoas({super.key});

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
            titulo: 'Pessoas',
            subtitulo:
                'Quem compartilhou encontros com você, em qualquer grupo.',
          ),
          SizedBox(height: EspacamentosDoAplicativo.extraGrande),
          Expanded(
            child: Center(
              child: CartaoDoAplicativo(
                filho: EstadoVazio(
                  icone: Icons.people_outline_rounded,
                  titulo: 'As pessoas aparecerão aqui',
                  descricao: 'O Juntô reunirá em um único perfil os encontros '
                      'compartilhados com cada pessoa, mesmo em grupos diferentes.',
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
