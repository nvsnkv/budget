import { expect, test } from '@playwright/test';
import { mockAuthenticatedApi } from '../fixtures/api-mocks';

test.describe('auth and navigation smoke', () => {
  test('shows authenticated shell on operations route', async ({ page }) => {
    await mockAuthenticatedApi(page);
    await page.goto('/budget/budget-1/operations', { waitUntil: 'domcontentloaded' });

    await expect(page.getByTestId('auth-logout-link')).toBeVisible();
    await expect(page.getByTestId('auth-login-link')).toHaveCount(0);
    await expect(page.getByTestId('owner-name')).toHaveText('E2E User');
    await expect(page.getByTestId('budget-selector-trigger')).toContainText('Household');
  });

  test('renders budget navigation links for operations context', async ({ page }) => {
    await mockAuthenticatedApi(page);
    await page.goto('/budget/budget-1/operations', { waitUntil: 'domcontentloaded' });

    await expect(page.getByTestId('nav-link-logbook')).toBeVisible();
    await expect(page.getByTestId('nav-link-operations')).toBeVisible();
    await expect(page.getByTestId('nav-link-transfers')).toBeVisible();
    await expect(page.getByTestId('nav-link-import')).toBeVisible();
    await expect(page.getByTestId('nav-link-delete')).toBeVisible();
    await expect(page.getByTestId('nav-link-details')).toBeVisible();
  });
});
