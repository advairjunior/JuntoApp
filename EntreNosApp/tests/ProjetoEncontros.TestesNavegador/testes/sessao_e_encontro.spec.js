const { test, expect } = require('@playwright/test');

const enderecoDaApi = 'http://localhost:5281';
const senhaDosTestes = 'SenhaE2E!2026';

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

async function cadastreUsuarioAsync(requisicao, identificadorDaExecucao) {
  const email = `qa.${identificadorDaExecucao}@example.test`;
  const resposta = await requisicao.post(
    `${enderecoDaApi}/api/autenticacao/cadastro`,
    {
      data: {
        nome: `QA ${identificadorDaExecucao}`,
        email,
        senha: senhaDosTestes,
      },
    },
  );

  expect(resposta.status()).toBe(201);
  return email;
}

async function entreAsync(pagina, email) {
  await pagina.goto('/entrada');
  await habiliteAcessibilidadeAsync(pagina);
  await pagina.getByRole('textbox', { name: 'E-mail' }).fill(email);
  const campoDaSenha = pagina.getByLabel('Senha');
  await campoDaSenha.click();
  await campoDaSenha.pressSequentially(senhaDosTestes);

  const respostaDoLogin = pagina.waitForResponse(
    (resposta) =>
      resposta.url().endsWith('/api/autenticacao/navegador/login') &&
      resposta.status() === 200,
  );

  await pagina.getByRole('button', { name: 'Entrar', exact: true }).click();
  await respostaDoLogin;
  await expect(pagina).toHaveURL(/\/inicio$/);
}

test('restaura a sessao pelo cookie depois de recarregar a pagina', async ({
  page: pagina,
  request: requisicao,
}) => {
  const identificadorDaExecucao = `sessao.${Date.now()}`;
  const email = await cadastreUsuarioAsync(
    requisicao,
    identificadorDaExecucao,
  );

  await entreAsync(pagina, email);

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
  await expect(
    pagina.getByRole('button', { name: 'Entrar', exact: true }),
  ).toHaveCount(0);
});

test('mostra na tela inicial o encontro criado sem recarga manual', async ({
  page: pagina,
  request: requisicao,
}) => {
  const identificadorDaExecucao = `encontro.${Date.now()}`;
  const email = await cadastreUsuarioAsync(
    requisicao,
    identificadorDaExecucao,
  );
  const titulo = `Encontro E2E ${identificadorDaExecucao}`;

  await entreAsync(pagina, email);
  await habiliteAcessibilidadeAsync(pagina);
  await pagina.getByRole('button', { name: 'Criar encontro' }).click();
  await habiliteAcessibilidadeAsync(pagina);

  const campoDoTitulo = pagina.getByRole('textbox', {
    name: 'Título do encontro',
  });
  await expect(campoDoTitulo).toBeVisible();
  await campoDoTitulo.pressSequentially(titulo);
  const campoDoLocal = pagina.getByRole('textbox', {
    name: 'Nome ou endereço do local',
  });
  await campoDoLocal.pressSequentially(`Local ${identificadorDaExecucao}`);
  const campoDaDescricao = pagina.getByRole('textbox', {
    name: 'Descrição opcional',
  });
  await campoDaDescricao.pressSequentially(
    `Criado pela automacao ${identificadorDaExecucao}`,
  );

  const criacaoDoEncontro = pagina.waitForResponse(
    (resposta) =>
      resposta.url().endsWith('/api/encontros') &&
      resposta.request().method() === 'POST' &&
      resposta.status() === 201,
  );

  await pagina.getByRole('button', {
    name: 'Criar encontro',
    exact: true,
  }).click();
  await criacaoDoEncontro;
  await expect(pagina).toHaveURL(/\/inicio$/);
  await habiliteAcessibilidadeAsync(pagina);
  await expect(
    pagina.getByRole('button', { name: titulo }),
  ).toHaveCount(1);
});
