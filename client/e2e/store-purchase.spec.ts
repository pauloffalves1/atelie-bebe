import { expect, test } from '@playwright/test';

/** Generates a syntactically valid (correct check digits) CPF, unique per test run. */
function generateCpf(): string {
  const digits = Array.from({ length: 9 }, () => Math.floor(Math.random() * 10));
  const checkDigit = (nums: number[], weightStart: number) => {
    let sum = 0;
    let weight = weightStart;
    for (const n of nums) sum += n * weight--;
    const rest = (sum * 10) % 11;
    return rest === 10 ? 0 : rest;
  };
  const d1 = checkDigit(digits, 10);
  const d2 = checkDigit([...digits, d1], 11);
  return [...digits, d1, d2].join('');
}

/**
 * Covers the single most business-critical flow on the site: a visitor personalizing a product
 * (embroidery text + thread color, both mandatory), registering an account (checkout requires a
 * logged-in customer), and completing checkout. A regression anywhere in that chain (product
 * page, cart, registration, checkout form, order creation) breaks the ateliê's only way of
 * taking orders.
 */
test('customer can personalize a product, register, and complete checkout', async ({ page }) => {
  await page.goto('/produto/fralda-de-boca-bordada-florzinha');

  await page.getByPlaceholder('Ex: ANA').fill('LUA');
  await page.getByRole('button', { name: 'Rosa', exact: true }).click();
  await page.getByRole('button', { name: 'Adicionar ao carrinho' }).click();

  await page.goto('/carrinho');
  await expect(page.getByText('LUA')).toBeVisible();
  await page.getByRole('link', { name: 'Finalizar compra' }).click();

  // Checkout requires a logged-in customer; register a fresh one and land back on /checkout.
  await expect(page).toHaveURL(/\/entrar\?returnUrl=%2Fcheckout/);
  await page.getByRole('link', { name: 'Cadastre-se' }).click();
  await expect(page).toHaveURL(/\/cadastro/);

  const uniqueEmail = `teste.e2e.${Date.now()}@example.com`;
  const cpf = generateCpf();
  await page.locator('#register-name').fill('Cliente Teste E2E');
  await page.locator('#register-email').fill(uniqueEmail);
  await page.locator('#register-cpf').fill(cpf);
  await page.locator('#register-phone').fill('11999998888');
  await page.locator('#register-password').fill('senha123');
  await page.locator('#register-zipCode').fill('01000-000');
  await page.locator('#register-street').fill('Praça da Sé');
  await page.locator('#register-number').fill('100');
  await page.locator('#register-neighborhood').fill('Sé');
  await page.locator('#register-city').fill('São Paulo');
  await page.locator('#register-state').fill('SP');
  await page.getByRole('button', { name: 'Criar conta' }).click();

  await expect(page).toHaveURL(/\/checkout/, { timeout: 10_000 });

  await page.locator('#checkout-customerName').fill('Cliente Teste E2E');
  await page.locator('#checkout-customerEmail').fill(uniqueEmail);
  await page.locator('#checkout-customerPhone').fill('11999998888');
  await page.locator('#checkout-customerCpf').fill(cpf);
  await page.locator('#checkout-zipCode').fill('01000-000');
  await page.locator('#checkout-street').fill('Praça da Sé');
  await page.locator('#checkout-number').fill('100');
  await page.locator('#checkout-neighborhood').fill('Sé');
  await page.locator('#checkout-city').fill('São Paulo');
  await page.locator('#checkout-state').fill('SP');

  await page.getByRole('button', { name: 'Confirmar pedido' }).click();

  await expect(page).toHaveURL(/\/pedido\//, { timeout: 15_000 });
  await expect(page.getByRole('heading', { name: 'Pedido recebido!' })).toBeVisible();
});
