import { expect, test } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

interface PublicCard {
  id: string;
  publicReference: string;
  make: string;
  model: string;
}

interface PublicList {
  items: PublicCard[];
  totalCount: number;
}

test.describe('catálogo público', () => {
  test('muestra el inventario y permite filtrar por marca', async ({ page, request }) => {
    const response = await request.get('/api/public/vehicles?pageSize=1');
    expect(response.ok()).toBeTruthy();
    const data = (await response.json()) as PublicList;
    test.skip(data.totalCount === 0, 'No hay vehículos disponibles para probar el catálogo.');

    await page.goto('/');

    await expect(
      page.getByRole('heading', { name: /Encuentra tu próximo auto con confianza/i }),
    ).toBeVisible();
    const cards = page.locator('app-public-vehicle-card');
    await expect(cards.first()).toBeVisible();
    await expect(cards.first().getByText(data.items[0].publicReference)).toBeVisible();

    await page.getByText('Más filtros', { exact: true }).click();
    await page.getByLabel('Marca').selectOption(data.items[0].make);

    await expect(page).toHaveURL(new RegExp(`make=${encodeURIComponent(data.items[0].make)}`));
    await expect(cards.first()).toBeVisible();
    await expect(page.getByText(/vehículo(s)? disponible/i).first()).toBeVisible();
  });

  test('detalle muestra datos, WhatsApp y JSON-LD', async ({ page, request }) => {
    const response = await request.get('/api/public/vehicles?pageSize=1');
    expect(response.ok()).toBeTruthy();
    const data = (await response.json()) as PublicList;
    test.skip(data.totalCount === 0, 'No hay vehículos disponibles para probar el detalle.');
    const card = data.items[0];

    await page.goto(`/vehiculos/${card.id}`);

    await expect(
      page.getByRole('heading', { level: 1, name: new RegExp(card.model, 'i') }),
    ).toBeVisible();
    await expect(page.getByText(`Ref. ${card.publicReference}`)).toBeVisible();
    await expect(page.getByRole('link', { name: /Contactar por WhatsApp/i })).toHaveAttribute(
      'href',
      /^https:\/\/wa\.me\//,
    );

    const jsonLd = await page.locator('#vehicle-json-ld').textContent();
    expect(jsonLd).toBeTruthy();
    expect(JSON.parse(jsonLd!)).toMatchObject({
      '@type': 'Car',
      sku: card.publicReference,
    });

    await page.getByRole('button', { name: 'Solicitar información' }).click();
    await expect(page.getByText('El nombre es requerido.')).toBeVisible();
    await expect(page.getByText('Ingresa un correo válido.')).toBeVisible();
  });

  test('es responsive y no desborda horizontalmente', async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto('/');

    await expect(page.getByRole('link', { name: 'Catálogo', exact: true })).toBeVisible();
    const overflow = await page.evaluate(
      () => document.documentElement.scrollWidth - window.innerWidth,
    );
    expect(overflow).toBeLessThanOrEqual(1);
  });

  test('catálogo y detalle no tienen violaciones de accesibilidad', async ({ page, request }) => {
    const response = await request.get('/api/public/vehicles?pageSize=1');
    const data = (await response.json()) as PublicList;
    test.skip(data.totalCount === 0, 'No hay vehículos disponibles para la auditoría.');

    await page.emulateMedia({ reducedMotion: 'reduce' });
    await page.goto('/');
    await expect(page.locator('app-public-vehicle-card').first()).toBeVisible();

    const catalogResults = await new AxeBuilder({ page }).analyze();
    expect(catalogResults.violations, JSON.stringify(catalogResults.violations, null, 2)).toEqual(
      [],
    );

    await page.goto(`/vehiculos/${data.items[0].id}`);
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible();

    const detailResults = await new AxeBuilder({ page }).analyze();
    expect(detailResults.violations, JSON.stringify(detailResults.violations, null, 2)).toEqual([]);
  });
});
