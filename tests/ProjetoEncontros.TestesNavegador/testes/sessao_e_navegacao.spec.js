const { test, expect } = require('@playwright/test');

const enderecoDaApi = 'http://localhost:5281';
const pinDosTestes = '123456';

async function habiliteAcessibilidadeAsync(pagina) {
  const arvoreSemantica = pagina.locator(
    'flt-semantics-host flt-semantics',
  );

  if (await arvoreSemantica.count() > 0) {
    return;
  }

  const ativador = pagina.locator('flt-semantics-placeholder');

  try {
    await ativador.waitFor({ state: 'attached', timeout: 15000 });
    await ativador.evaluate((elemento) => elemento.click());
  } catch {
    // A arvore pode permanecer habilitada depois da navegacao interna.
  }
}

function crieDadosDaPessoa() {
  const identificador = Date.now().toString().slice(-8);
  const numeroSemFormatacao = `629${identificador}`;

  return {
    nome: `Pessoa QA ${identificador}`,
    numeroSemFormatacao,
    numeroFormatado:
      `+55 (62) 9${identificador.slice(0, 4)}-${identificador.slice(4)}`,
    numeroNormalizado: `+55${numeroSemFormatacao}`,
  };
}

async function cadastreUsuarioAsync(requisicao, pessoa) {
  const resposta = await requisicao.post(
    `${enderecoDaApi}/api/autenticacao/cadastro`,
    {
      data: {
        nome: pessoa.nome,
        numeroDeCelular: pessoa.numeroSemFormatacao,
        pin: pinDosTestes,
      },
    },
  );

  expect(resposta.status()).toBe(201);
}

async function entreAsync(pagina, numeroDeCelular) {
  await pagina.goto('/entrada');
  await habiliteAcessibilidadeAsync(pagina);
  await pagina.getByRole('textbox', { name: 'Celular' }).fill(
    numeroDeCelular,
  );
  const campoDoPin = pagina.getByLabel('PIN de 6 dígitos');
  await campoDoPin.click();
  await campoDoPin.pressSequentially(pinDosTestes);
  await expect(campoDoPin).toHaveValue(pinDosTestes);

  const respostaDoLogin = pagina.waitForResponse(
    (resposta) =>
      resposta.url().endsWith('/api/autenticacao/navegador/login') &&
      resposta.status() === 200,
  );

  await pagina.getByRole('button', { name: 'Entrar', exact: true }).click();
  await respostaDoLogin;
  await expect(pagina).toHaveURL(/\/inicio$/);
}

test('mantem a sessao e permite navegar somente pelo novo nucleo', async ({
  page: pagina,
  request: requisicao,
}) => {
  const pessoa = crieDadosDaPessoa();
  await cadastreUsuarioAsync(requisicao, pessoa);
  await entreAsync(pagina, pessoa.numeroFormatado);

  await expect(
    pagina.getByText('Seus encontros aparecerão aqui'),
  ).toBeVisible();
  await expect(pagina.getByText(pessoa.numeroNormalizado)).toHaveCount(0);

  const renovacaoDaSessao = pagina.waitForResponse(
    (resposta) =>
      resposta.url().endsWith(
        '/api/autenticacao/navegador/renovar-sessao',
      ) && resposta.status() === 200,
  );

  await pagina.reload();
  await renovacaoDaSessao;
  await habiliteAcessibilidadeAsync(pagina);
  await expect(pagina).toHaveURL(/\/inicio$/);

  await pagina.getByRole('button', { name: 'Grupos' }).click();
  await expect(pagina).toHaveURL(/\/grupos$/);
  await expect(
    pagina.getByText('Seus grupos aparecerão aqui'),
  ).toBeVisible();
  await expect(pagina.getByText(pessoa.numeroNormalizado)).toHaveCount(0);

  await pagina.getByRole('button', { name: 'Pessoas' }).click();
  await expect(pagina).toHaveURL(/\/pessoas$/);
  await expect(
    pagina.getByText('As pessoas aparecerão aqui'),
  ).toBeVisible();
  await expect(pagina.getByText(pessoa.numeroNormalizado)).toHaveCount(0);

  await pagina.getByRole('button', { name: 'Perfil' }).click();
  await expect(pagina).toHaveURL(/\/perfil$/);
  await expect(
    pagina.getByRole('button', { name: new RegExp(pessoa.nome) }),
  ).toBeVisible();
  await expect(pagina.getByText(pessoa.numeroNormalizado)).toBeVisible();
  await expect(pagina.getByText('Memórias')).toHaveCount(0);
  await expect(pagina.getByText('Notificações')).toHaveCount(0);

  await pagina.getByRole('button', { name: 'Sair da conta' }).click();
  await expect(pagina).toHaveURL(/\/entrada(?:\?retorno=\/perfil)?$/);
  await expect(
    pagina.getByRole('button', { name: 'Entrar', exact: true }),
  ).toBeVisible();
});
