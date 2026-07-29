# Projeto Encontros - Instrucoes para agentes

## Idioma do codigo

Todo codigo deste projeto deve ser escrito em portugues.

Nao criar classes, metodos, propriedades, campos, enums, namespaces, pastas de dominio ou nomes de projetos internos em ingles.

Excecoes aceitaveis:

- nomes tecnicos obrigatorios do ecossistema, como `Program.cs`, `appsettings.json`, `Dockerfile`, `Guid`, `DateTimeOffset`, `Task`, `CancellationToken`, `IEnumerable`, `ICollection`, `IList`
- siglas tecnicas amplamente usadas, como API, JWT, HTTP, DTO
- nomes exigidos por bibliotecas, frameworks ou ferramentas

## Nomenclatura geral

- Usar PascalCase para tipos, metodos, propriedades, eventos, campos publicos e valores de enum.
- Usar camelCase para parametros.
- Usar `_camelCase` para campos privados.
- Nao usar `var`.
- Usar `new()` sempre que o tipo ja estiver explicito e isso nao prejudicar a clareza.
- Preferir construtores primarios quando possivel, sem transformar parametros em propriedades publicas desnecessarias.
- Sempre usar chaves em blocos de controle.
- Preferir nomes descritivos baseados no significado.

## Booleanos

Use frases afirmativas.

Exemplos:

- `PodeGerarBoleto`
- `TemVisibilidade`
- `ItemEhPermitido`

Evite nomes negativos como `NaoPodeGerarBoleto`.

## Classes, estruturas e interfaces

- Classes devem usar substantivos ou frases substantivas em portugues.
- Interfaces devem usar prefixo `I`.
- Nao usar prefixos como `CAluno` ou `CProfessor`.
- Quando houver uma interface e uma implementacao padrao, os nomes devem diferir apenas pelo prefixo `I` sempre que fizer sentido.

## Enums

- Usar nome singular.
- Para enums seguros, usar prefixo `EnumeradorDe`.

Exemplos:

- `SituacaoDoAluno`
- `EnumeradorDeEtnia`

## Metodos

- Usar verbos ou frases verbais.
- Preferir terceira pessoa do presente do conjuntivo quando fizer sentido no dominio.

Exemplos:

- `CalculeNossoNumero`
- `CrieConvite`
- `RemovaMembro`

## Propriedades

- Usar substantivo, frase substantiva ou adjetivo.
- Propriedades de colecao devem descrever os itens no plural ou usar formato descritivo como `ColecaoDeItensDeSeguranca`.

## Parametros

- Usar camelCase.
- Usar nomes descritivos.
- Preferir significado ao tipo.

Exemplo:

- `nomeDoAluno` em vez de `valor`

## Arquitetura

Preservar a arquitetura definida na documentacao:

- `Dominio`
- `Aplicacao`
- `Infraestrutura`
- `Api`

O dominio nao deve depender de ASP.NET Core, Entity Framework Core ou infraestrutura.
