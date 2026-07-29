const { defineConfig } = require('@playwright/test');

module.exports = defineConfig({
  testDir: './testes',
  fullyParallel: false,
  workers: 1,
  retries: 0,
  timeout: 60000,
  expect: {
    timeout: 10000,
  },
  reporter: [
    ['list'],
    ['html', {
      open: 'never',
      outputFolder: '../../.verificacao/testes-navegador/relatorio',
    }],
  ],
  outputDir: '../../.verificacao/testes-navegador/resultados',
  use: {
    baseURL: 'http://localhost:5391',
    browserName: 'chromium',
    channel: 'chrome',
    locale: 'pt-BR',
    timezoneId: 'America/Sao_Paulo',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
});
